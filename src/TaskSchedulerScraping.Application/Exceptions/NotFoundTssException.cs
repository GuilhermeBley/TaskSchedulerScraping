using System.Net;
using System.Runtime.Serialization;

namespace TaskSchedulerScraping.Application.Exceptions;

/// <summary>
/// Data not found
/// </summary>
public class NotFoundTssException : TssException
{
    public override int StatusCode => (int)HttpStatusCode.NotFound;

    public NotFoundTssException()
    {
    }

    public NotFoundTssException(string message) : base(message)
    {
    }

    public NotFoundTssException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public NotFoundTssException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
