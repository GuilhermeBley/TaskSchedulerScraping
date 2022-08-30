using System.Net;
using System.Runtime.Serialization;

namespace TaskSchedulerScraping.Application.Exceptions;

/// <summary>
/// Conflict on create or update
/// </summary>
public class BadRequestTssException : TssException
{
    public override int StatusCode => (int)HttpStatusCode.BadRequest;
    
    public BadRequestTssException()
    {
    }

    public BadRequestTssException(string message) : base(message)
    {
    }

    public BadRequestTssException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public BadRequestTssException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
