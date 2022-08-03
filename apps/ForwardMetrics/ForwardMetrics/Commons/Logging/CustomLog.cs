using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ForwardMetrics.Commons.Logging;

public class CustomLog
{
    // Code related properties
    [JsonProperty("className")]
    public string ClassName { get; set; }

    [JsonProperty("methodName")]
    public string MethodName { get; set; }

    [JsonProperty("logLevel")]
    public LogLevel LogLevel { get; set; }

    [JsonProperty("timeUtc")]
    public DateTimeOffset TimeUtc { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; }

    [JsonProperty("exception")]
    public string Exception { get; set; }

    [JsonProperty("stackTrace")]
    public string StackTrace { get; set; }

    // Azure related properties
    [JsonProperty("subscriptionId")]
    public string SubscriptionId { get; set; }

    [JsonProperty("resourceGroupName")]
    public string ResourceGroupName { get; set; }

    [JsonProperty("postgresDatabaseName")]
    public string PostgresDatabaseName { get; set; }
}
