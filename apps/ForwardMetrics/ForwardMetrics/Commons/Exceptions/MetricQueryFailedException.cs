using System;

namespace ForwardMetrics.Commons.Exceptions;

public class MetricQueryFailedException : Exception
{
    private readonly string _message;

    public MetricQueryFailedException(
        string message
    )
    {
        _message = message;
    }

    public string GetMessage()
        => _message;
}

