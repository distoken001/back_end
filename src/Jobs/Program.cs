using Microsoft.AspNetCore;
using Serilog;
using Serilog.Events;
using System.Reflection;

namespace Jobs
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = "Jobs";

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