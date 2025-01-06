using Microsoft.AspNetCore;
using Serilog;
using Serilog.Events;

namespace ListenService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = "ListenService";

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.File($"logs/listen_service_{DateTime.Now.ToString("yyMMddHHmm")}.log")
                .CreateLogger();

            var host = CreateWebHostBuilder(args);

            host.Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
            .ConfigureLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddSerilog();
            })
            .UseStartup<Startup>();
        }
    }
}