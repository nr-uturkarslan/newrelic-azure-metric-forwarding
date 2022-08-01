using System.Collections.Generic;
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
    private readonly string[] POSTGRES_RESOURCE_IDS = new string[]
    {
        "/subscriptions/<SUBSCRIPTION_ID>" +
        "/resourceGroups/<RESOURCE_GROUP_NAME>" +
        "/providers/Microsoft.DBforPostgreSQL/flexibleServers/<POSTGRES_DB_NAME>",
    };

    private readonly MetricsQueryClient _metricsClient;

    public MetricQueryHandler()
    {
        _metricsClient = new MetricsQueryClient(new DefaultAzureCredential());
    }

    public async Task Run(
        ILogger logger
    )
    {
        var metricProcessingTasks = new List<Task>();
        foreach (var resourceId in POSTGRES_RESOURCE_IDS)
        {
            metricProcessingTasks.Add(Task.Run(async () =>
            {
                await new MetricProcessor().Run(resourceId);
            }));
        }

        await Task.WhenAll(metricProcessingTasks);
    }
}

