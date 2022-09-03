using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ForwardMetrics.Commons.Logging;

public class CustomLogger
{
    private const string NEW_RELIC_LOGS_API = "https://log-api.eu.newrelic.com/log/v1";

    private readonly HttpClient _httpClient;
    private readonly List<CustomLog> _logs;

    public CustomLogger()
    {
        _httpClient = new HttpClient();
        _logs = new List<CustomLog>();
    }

    public void Log(
        CustomLog customLog
    )
        => _logs.Add(customLog);

    public async Task FlushLogsToNewRelic()
    {
        var requestDto = FormatLogsAccordingToNewRelic();
        var requestDtoAsString = JsonConvert.SerializeObject(
            requestDto,
            new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

        var stringContent = new StringContent(
            requestDtoAsString,
            Encoding.UTF8,
            "application/json"
        );

        var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            NEW_RELIC_LOGS_API
        )
        {
            Content = stringContent
        };

        httpRequest.Headers.Add("Api-Key",
            Environment.GetEnvironmentVariable("NEW_RELIC_LICENSE_KEY"));

        await _httpClient.SendAsync(httpRequest);
    }

    private List<LogApiRequestDto> FormatLogsAccordingToNewRelic()
    {
        var requestDto = new List<LogApiRequestDto>
        {
            new LogApiRequestDto
            {
                Logs = new List<CustomNewRelicLog>()
            }
        };

        foreach (var log in _logs)
        {
            var attributes = new Dictionary<string, string>
            {
                { "className", log.ClassName },
                { "methodName", log.MethodName },
                { "logLevel", log.LogLevel.ToString() },
            };

            if (!string.IsNullOrEmpty(log.SubscriptionId))
                attributes.Add("subscriptionId", log.SubscriptionId);

            if (!string.IsNullOrEmpty(log.ResourceGroupName))
                attributes.Add("resourceGroupName", log.ResourceGroupName);

            if (!string.IsNullOrEmpty(log.PostgresDatabaseName))
                attributes.Add("postgresDatabaseName", log.PostgresDatabaseName);

            if (!string.IsNullOrEmpty(log.Exception))
                attributes.Add("exception", log.Exception);

            if (!string.IsNullOrEmpty(log.StackTrace))
                attributes.Add("stackTrace", log.StackTrace);

            requestDto[0].Logs.Add(new CustomNewRelicLog
            {
                Message = log.Message,
                Timestamp = log.TimeUtc.ToUnixTimeMilliseconds(),
                Attributes = attributes,
            });
        }

        return requestDto;
    }
}

