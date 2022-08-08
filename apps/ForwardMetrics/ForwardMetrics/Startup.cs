using ForwardMetrics.Config.Postgres;
using ForwardMetrics.Services.Metrics;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(ForwardMetrics.Startup))]

namespace ForwardMetrics;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddHttpClient();

        builder.Services.AddScoped<IPostgresConfigReader, PostgresConfigReader>();
        builder.Services.AddScoped<IMetricQueryHandler, MetricQueryHandler>();
    }
}



