using CommonLibrary.DbContext;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Com.Ctrip.Framework.Apollo;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Http.Features;
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
            //services.AddScoped<CustomerRebateService>();
            services.AddScoped<ReadNFTService>();
            //services.AddScoped<TronReadNFTService>();
            services
                .AddHttpClient()
                //.AddSingleton(Configuration)
                //.AddSingleton<ApolloConfigs>()
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
            string grandparentDirectory = Directory.GetParent(Directory.GetParent(env.ContentRootPath).FullName).FullName;
            //app.UseDirectoryBrowser(new DirectoryBrowserOptions
            //{
            //    FileProvider = new PhysicalFileProvider(Path.Combine(grandparentDirectory, "uploads")),
            //    RequestPath = "/uploads"
            //});
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(grandparentDirectory, "docs")),
                RequestPath = new PathString("/docs")//对外的访问路径
            });
            //定时任务
            QuartzStartup.Run().Wait();
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
            //app.UseMvc();
            app.Map("/trigger/manage", AppMap.ManageQuartz);
            app.Map("/trigger/state", AppMap.GetQuartzState);
            app.Map("/job/running", AppMap.Runningjobs);
        }
    }
}
