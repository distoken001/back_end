using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using Serilog.Events;

namespace TelegramService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = "TelegramService";

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override(
                    "Microsoft.AspNetCore.Authentication",
                    LogEventLevel.Information
                )
                .Enrich.FromLogContext()
                .WriteTo.File($"logs/telegram_service_{DateTime.Now.ToString("yyMMddHHmm")}.log")
                .CreateLogger();

            var host = CreateWebHostBuilder(args);

            host.Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost
                .CreateDefaultBuilder(args)
                .ConfigureLogging(builder =>
                {
                    builder.ClearProviders();
                    builder.AddSerilog();
                })
                .UseStartup<Startup>();
        }
    }
}
