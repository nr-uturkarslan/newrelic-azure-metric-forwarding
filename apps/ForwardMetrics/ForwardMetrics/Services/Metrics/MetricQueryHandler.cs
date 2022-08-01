using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ForwardMetrics.Commons.Logging;
using ForwardMetrics.Config.Postgres;
using Microsoft.Extensions.Logging;

namespace ForwardMetrics.Services.Metrics;

public interface IMetricQueryHandler
{
    Task Run();
}

public class MetricQueryHandler : IMetricQueryHandler
{
    private readonly string[] POSTGRES_RESOURCE_IDS = new string[]
    {
        "/subscriptions/<SUBSCRIPTION_ID>" +
        "/resourceGroups/<RESOURCE_GROUP_NAME>" +
        "/providers/Microsoft.DBforPostgreSQL/flexibleServers/<POSTGRES_DB_NAME>",
    };

    private readonly CustomLogger _customLogger;

    public MetricQueryHandler()
    {
        _customLogger = new CustomLogger();
    }

    public async Task Run()
    {
        LogProcessingStarted();

        var metricProcessingTasks = new List<Task>();

        var postgresConfig = new PostgresConfig
        {
            Subscriptions = new List<Subscription>
            {
                new Subscription
                {
                    Id = "",
                    ResourceGroups = new List<ResourceGroup>
                    {
                        new ResourceGroup
                        {
                            Name = "",
                            PostgresDatabaseNames = new List<string>
                            {
                                "",
                            },
                        },
                    },
                },
            },
        };

        foreach (var subscription in postgresConfig.Subscriptions)
        {
            foreach (var resourceGroup in subscription.ResourceGroups)
            {
                foreach (var postgresDatabaseName in resourceGroup.PostgresDatabaseNames)
                {
                    metricProcessingTasks.Add(
                        Task.Run(async () =>
                        {
                            await new MetricProcessor(
                                subscription.Id,
                                resourceGroup.Name,
                                postgresDatabaseName
                            ).Run();
                        }));
                }
            }
        }

        await Task.WhenAll(metricProcessingTasks);

        LogProcessingFinished();
    }

    private void LogProcessingStarted()
    {
        _customLogger.Log(new CustomLog
        {
            ClassName = nameof(MetricQueryHandler),
            MehtodName = nameof(Run),
            LogLevel = LogLevel.Information,
            TimeUtc = DateTime.UtcNow,
            Message = $"Processing all Postgres DB metrics in parallel has started.",
        });
    }

    private void LogProcessingFinished()
    {
        _customLogger.Log(new CustomLog
        {
            ClassName = nameof(MetricQueryHandler),
            MehtodName = nameof(Run),
            LogLevel = LogLevel.Information,
            TimeUtc = DateTime.UtcNow,
            Message = $"Processing all Postgres DB metrics in parallel has finished.",
        });
    }
}

