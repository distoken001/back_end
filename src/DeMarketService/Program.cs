using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace deMarketService
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
                .WriteTo.File($"logs/deMarketService_{DateTime.Now.ToString("yyMMddHHmm")}.log")
                //.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Literate)
                .CreateLogger();

            var host = CreateWebHostBuilder(args);

            host.Build().Run();
        }

        //public static IHostBuilder CreateHostBuilder(string[] args) =>
        //  Host.CreateDefaultBuilder(args)
        //      .ConfigureWebHostDefaults(webBuilder =>
        //      {
        //          webBuilder.ConfigureLogging(options =>
        //          {
        //              options.ClearProviders();
        //              options.AddSerilog();
        //          });
        //          webBuilder.ConfigureKestrel(options =>
        //          {
        //              options.ListenAnyIP(5000);
        //          });
        //          webBuilder.UseStartup<Startup>();
        //      });

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {

            string appRoot = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return WebHost.CreateDefaultBuilder(args)
            .ConfigureLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddSerilog();

            })
            //.ConfigureKestrel(options =>
            //{
            //    options.ListenAnyIP(5000);

            //})
            .UseContentRoot(appRoot)
            .UseStartup<Startup>();
        }
    }
}