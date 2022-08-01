using System.Threading.Tasks;
using Azure.Identity;
using Azure.Monitor.Query;
using Microsoft.Extensions.Logging;

namespace ForwardMetrics.Services.Metrics;

public interface IMetricQueryHandler
{
    Task Run(
        ILogger logger
    );
}

public class MetricQueryHandler : IMetricQueryHandler
{
    public MetricQueryHandler()
    {
    }

    public async Task Run(
        ILogger logger
    )
    {
        var resourceId =
            "/subscriptions/e1db9504-a667-4b67-bf49-22abe8f772c7/resourceGroups/rgugureuwmetricsd001/providers/Microsoft.Web/sites/funcugureuwmetricsd001";

        var metricsClient = new MetricsQueryClient(new DefaultAzureCredential());

        var results = await metricsClient.QueryResourceAsync(
            resourceId,
            new[] { "Requests", "BytesReceived", "BytesSent", "Http2xx", "Http3xx", "Http4xx", "Http5xx"}
        );

        foreach (var metric in results.Value.Metrics)
        {
            logger.LogInformation(metric.Name);

            foreach (var element in metric.TimeSeries)
            {
                logger.LogInformation("Dimensions: " + string.Join(",", element.Metadata));

                foreach (var metricValue in element.Values)
                {
                    logger.LogInformation(metricValue.ToString());
                }
            }
        }
    }
}

