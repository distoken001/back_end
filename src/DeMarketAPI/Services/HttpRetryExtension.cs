using Microsoft.Extensions.DependencyInjection;
using Polly;
using System;
using System.Linq;
using System.Net.Http;

namespace DeMarketAPI.Services
{
    public static class HttpRetryExtension
    {
        public static void AddMyProxy<TCient>(this IServiceCollection services) where TCient : class
        {
            //重试策略
            var retryPolicy = Policy.Handle<HttpRequestException>()//指定要处理的异常
                .Or<Exception>()
                .OrResult<HttpResponseMessage>(res => { return res.StatusCode != System.Net.HttpStatusCode.OK; })//返回结果不是Ok，也重试
                .WaitAndRetryAsync(
                    //指定重试次数
                    sleepDurations: new[]
                    {
                        TimeSpan.FromMilliseconds(100),
                        TimeSpan.FromMilliseconds(200),
                        TimeSpan.FromMilliseconds(500)
                    },
                    //出异常会执行以下代码
                    onRetry: (exception, ts, context) =>
                    {
                        Console.WriteLine($"【2】polly.retry：exMsg={ exception.Exception?.Message}, {ts.Minutes * 60 + ts.Seconds}秒后重试");
                    });

            //超时策略
            var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(10);

            services.AddHttpClient<TCient>()
                .AddPolicyHandler(retryPolicy)
                .AddPolicyHandler(timeoutPolicy);
        }

        /// <summary>
        /// 添加可重试的异步HttpClient
        /// 
        /// 网络故障（System.Net.Http.HttpRequestException）
        /// HTTP 5XX状态代码（服务器错误）
        /// HTTP 408状态码（请求超时）
        /// </summary>
        /// <typeparam name="TClient">要注入的异步HttpClient</typeparam>
        /// <param name="services">注入集合</param>
        /// <param name="retryCount">重试次数</param>
        /// <param name="exs">要捕获的异常</param>
        /// <param name="res">要捕获的结果</param>
        /// <returns></returns>
        public static IHttpClientBuilder HttpErrorRetryAsyncPolicy<TClient>(
            this IServiceCollection services, int retryCount, Func<Exception, bool>[] exs = null,
            Func<HttpResponseMessage, bool>[] res = null) where TClient : class
        {
            return services
                .AddHttpClient<TClient>()
                .AddTransientHttpErrorPolicy(builder =>
                {
                    exs?.ToList().ForEach(ex =>
                    {
                        builder = builder.Or(ex);
                    });
                    res?.ToList().ForEach(re =>
                    {
                        builder = builder.OrResult(re);
                    });
                    return builder.RetryAsync(retryCount);
                });
        }

        /// <summary>
        /// 添加可重试的HttpClient
        /// 
        /// 网络故障（System.Net.Http.HttpRequestException）
        /// HTTP 5XX状态代码（服务器错误）
        /// HTTP 408状态码（请求超时）
        /// </summary>
        /// <typeparam name="TClient">要注入的HttpClient</typeparam>
        /// <param name="services">注入集合</param>
        /// <param name="retryCount">重试次数</param>
        /// <param name="exs">要捕获的异常</param>
        /// <param name="res">要捕获的结果</param>
        /// <returns></returns>
        public static IHttpClientBuilder HttpErrorRetryPolicy<TClient>(this IServiceCollection services, int retryCount, Func<Exception, bool>[] exs = null,
            Func<HttpResponseMessage, bool>[] res = null) where TClient : class
        {
            return services
             .AddHttpClient<TClient>()
             .AddTransientHttpErrorPolicy(builder =>
             {
                 exs?.ToList().ForEach(ex =>
                 {
                     builder = builder.Or(ex);
                 });
                 res?.ToList().ForEach(re =>
                 {
                     builder = builder.OrResult(re);
                 });
                 return builder.RetryAsync(retryCount);
             });
        }
    }
}
