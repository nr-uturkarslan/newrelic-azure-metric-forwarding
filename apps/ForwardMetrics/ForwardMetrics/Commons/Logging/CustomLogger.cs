using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace ForwardMetrics.Commons.Logging;

public class CustomLogger
{
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

    public async Task FlushToNewRelic()
    {

    }
}
