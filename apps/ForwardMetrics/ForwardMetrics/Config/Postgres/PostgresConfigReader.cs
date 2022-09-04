using System;
using System.Text;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Storage.Blobs;
using ForwardMetrics.Commons.Logging;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ForwardMetrics.Config.Postgres;

public interface IPostgresConfigReader
{
    Task<PostgresConfig> Run();
}

public class PostgresConfigReader : IPostgresConfigReader
{
    private const string BLOB_CONTAINER_NAME = "config";
    private const string CONFIG_FILE_NAME = "config.json";

    private readonly BlobContainerClient _containerClient;

    private readonly CustomLogger _customLogger;

    public PostgresConfigReader()
    {
        _containerClient = new BlobContainerClient(
            new Uri(Environment.GetEnvironmentVariable("CONFIG_BLOB_URI")),
            new DefaultAzureCredential()
        );

        _customLogger = new CustomLogger();
    }

    public async Task<PostgresConfig> Run()
    {
        LogReadingConfigFile();

        try
        {
            var blob = await _containerClient.GetBlobClient(CONFIG_FILE_NAME)
                .DownloadContentAsync();

            var blobAsString = Encoding.UTF8.GetString(blob.Value.Content);
            var config = JsonConvert.DeserializeObject<PostgresConfig>(blobAsString);

            LogConfigFileRead();

            return config;
        }
        catch (Exception e)
        {
            LogUnexpectedErrorOccurred(e);
            return null;
        }
        finally
        {
            await _customLogger.FlushLogsToNewRelic();
        }
    }

    private void LogReadingConfigFile()
    {
        _customLogger.Log(new CustomLog
        {
            ClassName = nameof(PostgresConfigReader),
            MethodName = nameof(Run),
            LogLevel = LogLevel.Information,
            TimeUtc = DateTimeOffset.UtcNow,
            Message = $"Reading configuration file...",
        });
    }

    private void LogConfigFileRead()
    {
        _customLogger.Log(new CustomLog
        {
            ClassName = nameof(PostgresConfigReader),
            MethodName = nameof(Run),
            LogLevel = LogLevel.Information,
            TimeUtc = DateTimeOffset.UtcNow,
            Message = $"Configuration file is read.",
        });
    }

    private void LogUnexpectedErrorOccurred(
        Exception exception
    )
    {
        _customLogger.Log(new CustomLog
        {
            ClassName = nameof(PostgresConfigReader),
            MethodName = nameof(Run),
            LogLevel = LogLevel.Error,
            TimeUtc = DateTimeOffset.UtcNow,
            Message = "Unexpected error occurred.",
            Exception = exception.Message,
            StackTrace = exception.StackTrace.ToString(),
        });
    }
}

