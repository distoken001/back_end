using DeMarketAPI.Common.Model.HttpApiModel.ResponseModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace DeMarketAPIApis.Filters
{
    public class ExLogFilter : IExceptionFilter
    {
        private readonly ILogger _logger;

        public ExLogFilter(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ExLogFilter>();
        }

        public void OnException(ExceptionContext context)
        {
            _logger.LogInformation($"request method:{context.HttpContext?.Request?.Path},token：{context.HttpContext?.Request?.Headers?["Authorization"]}");

            _logger.LogError(context.Exception, context.Exception.Message);

            context.Result = new JsonResult(new WebApiResult(-1, context.Exception.Message));

            context.ExceptionHandled = true;
        }
    }
}