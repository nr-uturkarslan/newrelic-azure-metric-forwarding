using System.Collections.Generic;
using Newtonsoft.Json;

namespace ForwardMetrics.Services.Metrics;

public class MetricApiRequestDto
{
    [JsonProperty("common")]
    public CommonNewRelicMetricProperties Common { get; set; }

    [JsonProperty("metrics")]
    public List<CustomNewRelicMetric> Metrics { get; set; }
}

public class CommonNewRelicMetricProperties
{
    [JsonProperty("attributes")]
    public Dictionary<string, string> Attributes { get; set; }
}

public class CustomNewRelicMetric
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