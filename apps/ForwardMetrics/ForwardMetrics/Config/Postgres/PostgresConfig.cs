using System.Collections.Generic;
using Newtonsoft.Json;

namespace ForwardMetrics.Config.Postgres;

public class PostgresConfig
{
    [JsonProperty("subscriptions")]
    public List<Subscription> Subscriptions { get; set; }
}

public class Subscription
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("resourceGroups")]
    public List<ResourceGroup> ResourceGroups { get; set; }
}

public class ResourceGroup
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("postgresDatabaseNames")]
    public List<string> PostgresDatabaseNames { get; set; }
}
