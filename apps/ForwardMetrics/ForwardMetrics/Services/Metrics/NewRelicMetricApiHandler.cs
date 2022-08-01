using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Azure.Monitor.Query.Models;
using ForwardMetrics.Commons.Logging;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ForwardMetrics.Services.Metrics;

public class NewRelicMetricApiHandler
{
    private const string NEW_RELIC_METRICS_API = "https://metric-api.eu.newrelic.com/metric/v1";

    private readonly string _subscriptionId;
    private readonly string _resourceGroupName;
    private readonly string _postgresDatabaseName;

    private readonly CustomLogger _customLogger;
    private readonly HttpClient _httpClient;

    public NewRelicMetricApiHandler(
        string subscriptionId,
        string resourceGroupName,
        string postgresDatabaseName
    )
    {
        _subscriptionId = subscriptionId;
        _resourceGroupName = resourceGroupName;
        _postgresDatabaseName = postgresDatabaseName;

        _customLogger = new CustomLogger();
        _httpClient = new HttpClient();
    }

    public async Task Run(
        IReadOnlyList<MetricResult> metricResults
    )
    {
        var requestDto = PrepareRequestDto(metricResults);
        await FlushMetricsToNewRelic(requestDto);
    }

    public List<MetricApiRequestDto> PrepareRequestDto(
        IReadOnlyList<MetricResult> metricResults
    )
    {
        LogPreparingRequestDto();

        var requestDto = new List<MetricApiRequestDto>
        {
            new MetricApiRequestDto
            {
                Common = new CommonMetricProperties
                {
                    Attributes = new Dictionary<string, string>
                    {
                        { "subscriptionId", _subscriptionId },
                        { "resourceGroupName", _resourceGroupName },
                        { "postgresDatabaseName", _postgresDatabaseName },
                    }
                },
                Metrics = new List<CustomMetric>(),
            }
        };

        if (metricResults.Count > 0)
            requestDto[0].Common.Attributes.Add("metricUnit", metricResults[0].Unit.ToString());

        foreach (var metricResult in metricResults)
        {
            var metricName = metricResult.Name;

            foreach (var element in metricResult.TimeSeries)
            {
                foreach (var metricValue in element.Values)
                {
                    if (metricValue.Average.HasValue)
                    {
                        requestDto[0].Metrics.Add(new CustomMetric
                        {
                            Name = metricName,
                            Type = "gauge",
                            Value = metricValue.Average.Value,
                            Timestamp = metricValue.TimeStamp.ToUnixTimeMilliseconds(),
                        });
                    }
                }
            }
        }

        LogRequestDtoPrepared();

        return requestDto;
    }
    private async Task<HttpResponseMessage> FlushMetricsToNewRelic(
        List<MetricApiRequestDto> requestDto
    )
    {
        LogFlushingMetrics();

        var requestDtoAsString = JsonConvert.SerializeObject(requestDto);

        var stringContent = new StringContent(
            requestDtoAsString,
            Encoding.UTF8,
            "application/json"
        );

        var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            NEW_RELIC_METRICS_API
        )
        {
            Content = stringContent
        };

        httpRequest.Headers.Add("Api-Key", "");

        var response = await _httpClient.SendAsync(httpRequest);

        if (response.IsSuccessStatusCode)
            LogMetricsFlushed();

        else
            LogMetricsNotFlushed(
                await response.Content.ReadAsStringAsync()
            );
            
        return response;
    }

    private void LogPreparingRequestDto()
    {
        _customLogger.Log(new CustomLog
        {
            ClassName = nameof(NewRelicMetricApiHandler),
            MehtodName = nameof(PrepareRequestDto),
            LogLevel = LogLevel.Information,
            TimeUtc = DateTime.UtcNow,
            Message = $"Preparing request dto for New Relic metrics API...",

            SubscriptionId = _subscriptionId,
            ResourceGroupName = _resourceGroupName,
            PostgresDatabaseName = _postgresDatabaseName,
        });
    }

    private void LogRequestDtoPrepared()
    {
        _customLogger.Log(new CustomLog
        {
            ClassName = nameof(NewRelicMetricApiHandler),
            MehtodName = nameof(PrepareRequestDto),
            LogLevel = LogLevel.Information,
            TimeUtc = DateTime.UtcNow,
            Message = $"Request dto for New Relic metrics API is prepared.",

            SubscriptionId = _subscriptionId,
            ResourceGroupName = _resourceGroupName,
            PostgresDatabaseName = _postgresDatabaseName,
        });
    }

    private void LogFlushingMetrics()
    {
        _customLogger.Log(new CustomLog
        {
            ClassName = nameof(NewRelicMetricApiHandler),
            MehtodName = nameof(FlushMetricsToNewRelic),
            LogLevel = LogLevel.Information,
            TimeUtc = DateTime.UtcNow,
            Message = $"Flushing metrics to New Relic.",

            SubscriptionId = _subscriptionId,
            ResourceGroupName = _resourceGroupName,
            PostgresDatabaseName = _postgresDatabaseName,
        });
    }

    private void LogMetricsFlushed()
    {
        _customLogger.Log(new CustomLog
        {
            ClassName = nameof(NewRelicMetricApiHandler),
            MehtodName = nameof(FlushMetricsToNewRelic),
            LogLevel = LogLevel.Information,
            TimeUtc = DateTime.UtcNow,
            Message = $"Metrics are flushed to New Relic.",

            SubscriptionId = _subscriptionId,
            ResourceGroupName = _resourceGroupName,
            PostgresDatabaseName = _postgresDatabaseName,
        });
    }

    private void LogMetricsNotFlushed(
        string exception
    )
    {
        _customLogger.Log(new CustomLog
        {
            ClassName = nameof(NewRelicMetricApiHandler),
            MehtodName = nameof(FlushMetricsToNewRelic),
            LogLevel = LogLevel.Error,
            TimeUtc = DateTime.UtcNow,
            Message = $"Metrics could not be flushed to New Relic.",
            Exception = exception,

            SubscriptionId = _subscriptionId,
            ResourceGroupName = _resourceGroupName,
            PostgresDatabaseName = _postgresDatabaseName,
        });
    }
}

