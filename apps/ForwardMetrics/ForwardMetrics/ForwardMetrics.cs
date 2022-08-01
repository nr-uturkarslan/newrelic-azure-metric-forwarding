using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Identity;
using Azure.Monitor.Query;
using ForwardMetrics.Services.Metrics;

namespace ForwardMetrics
{
    public class ForwardMetrics
    {
        private readonly IMetricQueryHandler _metricQueryHandler;

        public ForwardMetrics(
            IMetricQueryHandler metricQueryHandler
        )
        {
            _metricQueryHandler = metricQueryHandler;
        }

        [FunctionName("ForwardMetrics")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger logger)
        {
            logger.LogInformation("C# HTTP trigger function processed a request.");

            await _metricQueryHandler.Run(logger);

            return new OkObjectResult("");
        }

        //public async Task<string> GetAccessToken()
        //{
        //    var credentials = new DefaultAzureCredential();

        //    //ClientCredential cc = new ClientCredential(AzureDetails.ClientID, AzureDetails.ClientSecret);
        //    //var context = new AuthenticationContext("https://login.microsoftonline.com/" + AzureDetails.TenantID);
        //    //var result = context.AcquireTokenAsync("https://management.azure.com/", credentials);
        //    //if (result == null)
        //    //{
        //    //    throw new InvalidOperationException("Failed to obtain the Access token");
        //    //}
        //    //AzureDetails.AccessToken = result.Result.AccessToken;
        //}

        private async Task GetMetrics()
        {
            string resourceId =
                "/subscriptions/<subscription_id>/resourceGroups/<resource_group_name>/providers/<resource_provider>/<resource>";

            var metricsClient = new MetricsQueryClient(new DefaultAzureCredential());

            var results = await metricsClient.QueryResourceAsync(
                resourceId,
                new[] { "Microsoft.OperationalInsights/workspaces" }
            );

            foreach (var metric in results.Value.Metrics)
            {
                Console.WriteLine(metric.Name);
                foreach (var element in metric.TimeSeries)
                {
                    Console.WriteLine("Dimensions: " + string.Join(",", element.Metadata));

                    foreach (var metricValue in element.Values)
                    {
                        Console.WriteLine(metricValue);
                    }
                }
            }
        }
    }
}

