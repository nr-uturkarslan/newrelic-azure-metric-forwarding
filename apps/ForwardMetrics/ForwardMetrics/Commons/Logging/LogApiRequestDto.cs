using System.Collections.Generic;
using Newtonsoft.Json;

namespace ForwardMetrics.Commons.Logging;

public class LogApiRequestDto
{
    [JsonProperty("common")]
    public CommonNewRelicLogProperties Common { get; set; }

    [JsonProperty("logs")]
    public List<CustomNewRelicLog> Logs { get; set; }
}

public class CommonNewRelicLogProperties
{
    [JsonProperty("attributes")]
    public Dictionary<string, string> Attributes { get; set; }
}

public class CustomNewRelicLog
{
    [JsonProperty("message")]
    public string Message { get; set; }

    [JsonProperty("timestamp")]
    public long Timestamp { get; set; }

    [JsonProperty("attributes")]
    public Dictionary<string, string> Attributes { get; set; }
}

