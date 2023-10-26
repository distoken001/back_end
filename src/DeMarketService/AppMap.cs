using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace deMarketService
{
    public static class AppMap
    {
        public static void ManageQuartz(IApplicationBuilder app)
        {
            app.Run(async context =>
            {
                context.Response.ContentType = "text/html; charset=utf-8";
                var command = context.Request.Query["command"];
                await QuartzStartup.PauseOrResumeSchedulerAsync(command);
                await context.Response.WriteAsync((await QuartzStartup.GetBuiltyTriggerStateAsync()));
            });
        }

        public static void GetQuartzState(IApplicationBuilder app)
        {
            app.Run(async context =>
            {
                context.Response.ContentType = "text/html; charset=utf-8";
                await context.Response.WriteAsync((await QuartzStartup.GetBuiltyTriggerStateAsync()).ToString());
            });
        }

        public static void Runningjobs(IApplicationBuilder app)
        {
            app.Run(async context =>
            {
                context.Response.ContentType = "text/html; charset=utf-8";
                var result = await QuartzStartup.GetBuiltyCurrentlyExecutingJobsAsync();
                await context.Response.WriteAsync(string.IsNullOrEmpty(result) ? "无" : result);
            });
        }
    }
}
