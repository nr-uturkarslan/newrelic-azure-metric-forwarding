using System.Threading.Tasks;
using Azure.Identity;
using Azure.Monitor.Query;

namespace ForwardMetrics.Services.Metrics;

public class MetricProcessor
{
    private readonly string[] POSTGRES_METRIC_NAMES = new string[]
    {
        "IOPS",
    };

    private readonly MetricsQueryClient _metricsClient;

    public MetricProcessor()
    {
        _metricsClient = new MetricsQueryClient(new DefaultAzureCredential());
    }
    public async Task Run(
        string resourceId
    )
    {
        var results = await _metricsClient.QueryResourceAsync(
            resourceId,
            POSTGRES_METRIC_NAMES
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

