using System.Collections.Generic;
using Newtonsoft.Json;

namespace ForwardMetrics.Services.Metrics;

public class MetricApiRequestDto
{
    [JsonProperty("common")]
    public CommonMetricProperties Common { get; set; }

    [JsonProperty("metrics")]
    public List<CustomMetric> Metrics { get; set; }
}

public class CommonMetricProperties
{
    [JsonProperty("attributes")]
    public Dictionary<string, string> Attributes { get; set; }
}

public class CustomMetric
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("value")]
    public double Value { get; set; }

    [JsonProperty("timestamp")]
    public long Timestamp { get; set; }
}