using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ForwardMetrics.Services.Metrics;
using ForwardMetrics.Commons.Logging;

namespace ForwardMetrics
{
    public class ForwardMetrics
    {
        private const string FUNCTION_NAME = "ForwardMetrics";

        private readonly CustomLogger _customLogger;

        private readonly IMetricQueryHandler _metricQueryHandler;

        public ForwardMetrics(
            IMetricQueryHandler metricQueryHandler
        )
        {
            _metricQueryHandler = metricQueryHandler;

            _customLogger = new CustomLogger();
        }

        [FunctionName(FUNCTION_NAME)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger logger)
        {
            LogFunctionStarted();

            await _metricQueryHandler.Run();

            LogFunctionFinished();

            await _customLogger.FlushLogsToNewRelic();

            return new OkObjectResult("");
        }

        private void LogFunctionStarted()
        {
            _customLogger.Log(new CustomLog
            {
                ClassName = nameof(ForwardMetrics),
                MethodName = nameof(Run),
                LogLevel = LogLevel.Information,
                TimeUtc = DateTimeOffset.UtcNow,
                Message = $"{FUNCTION_NAME} function is started.",
            });
        }

        private void LogFunctionFinished()
        {
            _customLogger.Log(new CustomLog
            {
                ClassName = nameof(ForwardMetrics),
                MethodName = nameof(Run),
                LogLevel = LogLevel.Information,
                TimeUtc = DateTimeOffset.UtcNow,
                Message = $"{FUNCTION_NAME} function is finished.",
            });
        }
    }
}

