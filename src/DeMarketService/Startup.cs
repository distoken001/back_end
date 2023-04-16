using deMarketService.DbContext;
using deMarketServiceApis.Filters;
using deMarketService.Common;
using deMarketService.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using System.Text;

namespace deMarketService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            //var builder = new ConfigurationBuilder()
            //    .AddApollo(configuration.GetSection("apollo"))
            //    .AddNamespace("backend.share")
            //    .AddDefault();

            //Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            EncodingProvider provider = CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(provider);
            var identityConn = "Server=39.98.64.53:3306;Database=identities;Uid=buqun;Pwd=Md__1208;sslMode=None;";//Configuration[StringConstant.DatabaseConnectionString];
            var identityConnRead = "Server=39.98.64.53:3306;Database=identities;Uid=buqun;Pwd=Md__1208;sslMode=None;";//Configuration[StringConstant.DatabaseConnectionString_ReadOnly];
            services.AddScoped<ExLogFilter>();

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
                .AddDbContextPool<MySqlMasterDbContext>(options => options.UseMySql(identityConn), 800)
                .AddDbContextPool<MySqlSlaveDbContext>(options => options.UseMySql(identityConnRead), 800)
                .AddMvc(options => options.Filters.Add<ExLogFilter>())
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                    options.JsonSerializerOptions.PropertyNamingPolicy = null;
                });
                //.AddJsonOptions(options => options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver())//JSON首字母小写解决
                //.SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            //Swagger 配置
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "deMarketService API"
                });
            })
             .ConfigureSwaggerGen(options =>
             {
                 options.IncludeXmlComments(System.IO.Path.Combine(Microsoft.Extensions.PlatformAbstractions.PlatformServices.Default.Application.ApplicationBasePath, "deMarketService.xml"));
             });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, Microsoft.AspNetCore.Hosting.IApplicationLifetime lifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //跨域设置
            app.UseCors("CorsPolicy");

            //app.usehealth(configuration[stringconstant.metaenv], configuration[stringconstant.metaversion]);

            app.UseStaticFiles();

            //Swagger 配置
            app.UseSwagger()
            .UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"{(env.IsDevelopment() ? "" : "/" + Configuration["Consul:ServiceName:deMarketService"])}/swagger/v1/swagger.json", "deMarketService");
            });

            //Swagger 配置
            app.UseRouting();
            app.UseEndpoints(routes => routes.MapControllers());
            //   app.UseMvc(routes => routes.MapRoute(name: "default", template: "api/{controller=Home}/{action=Index}/{id?}"));

            //app.RegisterConsul(lifetime, Configuration, Configuration[StringConstant.DeMarketServiceConsulServiceName]);
        }
    }
}
