using Com.Ctrip.Framework.Apollo;
using CommonLibrary.DbContext;
using ListenService.Chains;
using ListenService.Repository.Implements;
using ListenService.Repository.Interfaces;
using ListenService.Service;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Net.WebSockets;
using System.Text;

namespace ListenService
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostEnvironment env)
        {
            //输出debug日志在控制台，方便查找问题
            Com.Ctrip.Framework.Apollo.Logging.LogManager.UseConsoleLogging(Com.Ctrip.Framework.Apollo.Logging.LogLevel.Warning);
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
                //.AddApollo(configuration.GetSection("apollo"))
                //.AddNamespace("backend.share")
                //.AddDefault();
            Configuration = builder.Build();
        }

        public static ServiceProvider privider;
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ClientManage>(provider =>
            {
                var nodeUrl = Configuration["BSC:WSS_URL"];
                var clientManage = new ClientManage(nodeUrl);

                Task.Run(async () =>
                {
                    while (true)
                    {
                        try
                        {
                            if (clientManage.GetClient().WebSocketState != WebSocketState.Open && clientManage.GetClient().WebSocketState != WebSocketState.Connecting)
                            {
                                Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "连接断开，正在重连..." + clientManage.GetClient().WebSocketState);
                                await clientManage.GetClient().StartAsync();
                                for (int i = 0; i < 5; i++)
                                {
                                    if (clientManage.GetClient().WebSocketState == WebSocketState.Open)
                                    {
                                        Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "连接成功！");
                                        break;
                                    }
                                    if (clientManage.GetClient().WebSocketState == WebSocketState.CloseReceived || clientManage.GetClient().WebSocketState == WebSocketState.CloseSent)
                                    {
                                        //clientManage.GetClient().Dispose();
                                        clientManage.ReplaceClient(new WebSocketClientBsc(nodeUrl));
                                    }
                                    await Task.Delay(500).ConfigureAwait(false);
                                }
                            }
                            await Task.Delay(1000).ConfigureAwait(false); // 检查间隔
                        }
                        catch (Exception ex)
                        {
                            //clientManage.GetClient().Dispose();
                            clientManage.ReplaceClient(new WebSocketClientBsc(nodeUrl));
                            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + $"连接错误: {ex.Message}");
                            await Task.Delay(1000).ConfigureAwait(false); // 延迟重试
                        }
                    }
                });

                return clientManage;
            });

            //解决文件上传Multipart body length limit 134217728 exceeded
            services.Configure<FormOptions>(x =>
             {
                 x.ValueLengthLimit = int.MaxValue;
                 x.MultipartBodyLengthLimit = int.MaxValue;
                 x.MemoryBufferThreshold = int.MaxValue;
             });

            EncodingProvider provider = CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(provider);
            //var deMarketConn = "Server=97.74.86.12;Database=ebay;Uid=dev;Pwd=Dev@1234;sslMode=None;";//Configuration[StringConstant.DatabaseConnectionString];
            var deMarketConn = Configuration["DbConnecting"];

            services.AddControllers();
            services.AddDirectoryBrowser();
            services.AddSingleton<IConfiguration>(Configuration);
            //redis
            var redisConn = Configuration["ConnectionStrings:RedisConnection"];
            services.AddSingleton<IDatabase>(ConnectionMultiplexer.Connect(redisConn).GetDatabase());

            services.AddSingleton<IBoxMinted, BoxMinted>();
            services.AddSingleton<IBoxGifted, BoxGifted>();
            services.AddSingleton<IBoxTypeRemoved, BoxTypeRemoved>();
            services.AddSingleton<IBoxTypeAdded, BoxTypeAdded>();
            services.AddSingleton<IPrizeClaimed, PrizeClaimed>();

            services.AddSingleton<IEbayAddOrder, EbayAddOrder>();
            services.AddSingleton<IEbaySetStatus, EbaySetStatus>();

            services.AddSingleton<IPostAddOrder, PostAddOrder>();
            services.AddSingleton<IPostSetStatus, PostSetStatus>();

            //services.AddSingleton<IAuctionAddOrder, AuctionAddOrder>();
            //services.AddSingleton<IAuctionSetOrderInfo, AuctionSetOrderInfo>();

            services.AddSingleton<ISendMessage, SendMessage>();

            //services.AddHostedService<AuctionAddOrderService>();
            //services.AddHostedService<AuctionSetOrderInfoService>();

            services.AddHostedService<EbayAddOrderService>();
            services.AddHostedService<EbaySetStatusService>();

            services.AddHostedService<PostAddOrderService>();
            services.AddHostedService<PostSetStatusService>();

            services.AddHostedService<PrizeClaimedService>();
            services.AddHostedService<BoxTypeRemovedService>();
            services.AddHostedService<BoxGiftedService>();
            services.AddHostedService<BoxTypeAddedService>();
            services.AddHostedService<BoxMintService>();
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
            app.UseStaticFiles();
            //string grandparentDirectory = Directory.GetParent(Directory.GetParent(env.ContentRootPath).FullName).FullName;
            //定时任务
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //跨域设置
            app.UseCors("CorsPolicy");

            //app.usehealth(configuration[stringconstant.metaenv], configuration[stringconstant.metaversion]);

            app.UseStaticFiles();

            //Swagger 配置
            app.UseRouting();
            app.UseEndpoints(routes => routes.MapControllers());
            //   app.UseMvc(routes => routes.MapRoute(name: "default", template: "api/{controller=Home}/{action=Index}/{id?}"));

            //app.RegisterConsul(lifetime, Configuration, Configuration[StringConstant.DeMarketServiceConsulServiceName]);
            app.UseHttpsRedirection();
        }
    }
}