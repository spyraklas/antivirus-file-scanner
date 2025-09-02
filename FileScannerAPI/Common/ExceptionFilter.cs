using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace FileScannerAPI.Common
{
    public class ExceptionFilter: ExceptionFilterAttribute
    {

        private readonly ILogger _logger;

        public ExceptionFilter(ILogger<ExceptionFilter> logger)
        {
            _logger = logger;
        }

        public override void OnException(ExceptionContext context)
        {
            var exception = context.Exception;
            EventId eventId = new EventId(1333, "Application error: " + exception.Message);
            _logger.LogError(eventId, exception, $" {context.HttpContext.Session.Id}  | {context.HttpContext.TraceIdentifier} | {exception.Message}");
        }

    }
}
