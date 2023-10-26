using CommonLibrary.DbContext;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text;
using AutoMapper;
using Microsoft.Extensions.FileProviders;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Com.Ctrip.Framework.Apollo;
using Jobs.jobs;

namespace Jobs
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostEnvironment env)
        {
            //输出debug日志在控制台，方便查找问题
            Com.Ctrip.Framework.Apollo.Logging.LogManager.UseConsoleLogging(Com.Ctrip.Framework.Apollo.Logging.LogLevel.Debug);
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddApollo(configuration.GetSection("apollo"))
                .AddNamespace("backend.share")
                .AddDefault();
            Configuration = builder.Build();
        }
        public static ServiceProvider privider;
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            EncodingProvider provider = CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(provider);
           
            var deMarketConn = Configuration["DbConnecting"];

            services.AddSingleton<IConfiguration>(Configuration);

            //services.AddScoped<CustomerRebateService>();ˆ
            services.AddScoped<ReadNFTService>();
            //services.AddScoped<TronReadNFTService>();
            services
                .AddHttpClient()

                .AddCors(options =>
                {
                    options.AddPolicy("CorsPolicy", builder =>
                    {
                        builder.SetIsOriginAllowed((x) => true)
                   .AllowAnyOrigin()
                   .AllowAnyHeader()
                   .AllowAnyMethod();
                    });
                })
                //.AddDbContext<MySqlMasterDbContext>(options => options.UseMySql(identityConn))
                .AddDbContext<MySqlMasterDbContext>(options => options.UseMySql(deMarketConn, builder => builder.EnableRetryOnFailure()));


            privider = services.BuildServiceProvider();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, Microsoft.AspNetCore.Hosting.IApplicationLifetime lifetime)
        {
            app.UseHttpsRedirection();
        
            //定时任务
            QuartzStartup.Run().Wait();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //跨域设置
            app.UseCors("CorsPolicy");
            app.UseHttpsRedirection();
            app.Map("/trigger/manage", AppMap.ManageQuartz);
            app.Map("/trigger/state", AppMap.GetQuartzState);
            app.Map("/job/running", AppMap.Runningjobs);
        }
    }
}
