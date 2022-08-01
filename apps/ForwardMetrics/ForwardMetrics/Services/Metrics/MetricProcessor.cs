using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure;
using Azure.Identity;
using Azure.Monitor.Query;
using Azure.Monitor.Query.Models;
using ForwardMetrics.Commons.Exceptions;
using ForwardMetrics.Commons.Logging;
using Microsoft.Extensions.Logging;

namespace ForwardMetrics.Services.Metrics;

public class MetricProcessor
{
    private readonly string[] POSTGRES_METRIC_NAMES = new string[]
    {
        "IOPS",
    };

    private readonly string _subscriptionId;
    private readonly string _resourceGroupName;
    private readonly string _postgresDatabaseName;

    private readonly string _resourceId;

    private readonly CustomLogger _customLogger;
    private readonly MetricsQueryClient _metricsClient;

    public MetricProcessor(
        string subscriptionId,
        string resourceGroupName,
        string postgresDatabaseName
    )
    {
        _subscriptionId = subscriptionId;
        _resourceGroupName = resourceGroupName;
        _postgresDatabaseName = postgresDatabaseName;

        _resourceId = $"/subscriptions/{subscriptionId}" +
            $"/resourceGroups/{resourceGroupName}" +
            $"/providers/Microsoft.DBforPostgreSQL/flexibleServers/{postgresDatabaseName}";

        _customLogger = new CustomLogger();
        _metricsClient = new MetricsQueryClient(new DefaultAzureCredential());
    }

    public async Task Run()
    {
        try
        {
            var metricsQueryOptions = CreateMetricQueryOptions();

            var results = await PerformMetricQuery(metricsQueryOptions);

            await FlushMetricsToNewRelic(results.Value.Metrics);
        }
        catch (MetricQueryFailedException e)
        {
            LogMetricQueryNotPerformed(e.GetMessage());
        }
        catch (Exception e)
        {
            LogUnexpectedErrorOccurred(e);
        }
    }

    private MetricsQueryOptions CreateMetricQueryOptions()
    {
        var startTime = new DateTimeOffset(
            DateTime.UtcNow.AddMinutes(-5)
        );
        var endTime = new DateTimeOffset(
            DateTime.UtcNow
        );

        var queryTimeRange = new QueryTimeRange(
            startTime,
            endTime
        );

        var metricQueryOpts = new MetricsQueryOptions
        {
            TimeRange = queryTimeRange,
            Granularity = new TimeSpan(0, 1, 0), // aggregate every minute
        };

        LogMetricQueryOptionsPrepared();

        return metricQueryOpts;
    }

    private async Task<Response<MetricsQueryResult>> PerformMetricQuery(
        MetricsQueryOptions metricsQueryOptions
    )
    {
        LogPerformingMetricQuery();

        try
        {
            var results = await _metricsClient.QueryResourceAsync(
                _resourceId,
                POSTGRES_METRIC_NAMES,
                metricsQueryOptions
            );

            LogMetricQueryPerformed();

            return results;
        }
        catch (Exception e)
        {
            throw new MetricQueryFailedException(e.Message);
        }
    }

    private async Task FlushMetricsToNewRelic(
        IReadOnlyList<MetricResult> metricResults
    )
    {
        await new NewRelicMetricApiHandler(
                _subscriptionId,
                _resourceGroupName,
                _postgresDatabaseName
            )
                .Run(metricResults);
    }
    private void LogMetricQueryOptionsPrepared()
    {
        _customLogger.Log(new CustomLog
        {
            ClassName = nameof(MetricProcessor),
            MehtodName = nameof(CreateMetricQueryOptions),
            LogLevel = LogLevel.Information,
            TimeUtc = DateTime.UtcNow,
            Message = "Metric query options are prepared.",

            SubscriptionId = _subscriptionId,
            ResourceGroupName = _resourceGroupName,
            PostgresDatabaseName = _postgresDatabaseName,
        });
    }

    private void LogPerformingMetricQuery()
    {
        _customLogger.Log(new CustomLog
        {
            ClassName = nameof(MetricProcessor),
            MehtodName = nameof(PerformMetricQuery),
            LogLevel = LogLevel.Information,
            TimeUtc = DateTime.UtcNow,
            Message = "Performing metric query...",

            SubscriptionId = _subscriptionId,
            ResourceGroupName = _resourceGroupName,
            PostgresDatabaseName = _postgresDatabaseName,
        });
    }

    private void LogMetricQueryNotPerformed(
        string exception
    )
    {
        _customLogger.Log(new CustomLog
        {
            ClassName = nameof(MetricProcessor),
            MehtodName = nameof(PerformMetricQuery),
            LogLevel = LogLevel.Information,
            TimeUtc = DateTime.UtcNow,
            Message = "Metric query is performed.",
            Exception = exception,

            SubscriptionId = _subscriptionId,
            ResourceGroupName = _resourceGroupName,
            PostgresDatabaseName = _postgresDatabaseName,
        });
    }

    private void LogMetricQueryPerformed()
    {
        _customLogger.Log(new CustomLog
        {
            ClassName = nameof(MetricProcessor),
            MehtodName = nameof(PerformMetricQuery),
            LogLevel = LogLevel.Information,
            TimeUtc = DateTime.UtcNow,
            Message = "Metric query is performed.",

            SubscriptionId = _subscriptionId,
            ResourceGroupName = _resourceGroupName,
            PostgresDatabaseName = _postgresDatabaseName,
        });
    }

    private void LogUnexpectedErrorOccurred(
        Exception exception
    )
    {
        _customLogger.Log(new CustomLog
        {
            ClassName = nameof(MetricProcessor),
            MehtodName = nameof(Run),
            LogLevel = LogLevel.Information,
            TimeUtc = DateTime.UtcNow,
            Message = "Unexpected error occurred.",
            Exception = exception.Message,
            StackTrace = exception.StackTrace.ToString(),

            SubscriptionId = _subscriptionId,
            ResourceGroupName = _resourceGroupName,
            PostgresDatabaseName = _postgresDatabaseName,
        });
    }
}

