using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Jobs;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace Jobs
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = "deMarketService";

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.File($"logs/jobs_{DateTime.Now.ToString("yyMMddHHmm")}.log")
                .CreateLogger();

            var host = CreateWebHostBuilder(args);

            host.Build().Run();
        }

  

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {

            string appRoot = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return WebHost.CreateDefaultBuilder(args)
            .ConfigureLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddSerilog();

            })
         
            .UseContentRoot(appRoot)
            .UseStartup<Startup>();
        }
    }
}