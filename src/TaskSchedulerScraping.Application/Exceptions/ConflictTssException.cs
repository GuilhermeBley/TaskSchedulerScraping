using System.Net;
using System.Runtime.Serialization;

namespace TaskSchedulerScraping.Application.Exceptions;

/// <summary>
/// Conflict on create or update
/// </summary>
public class ConflictTssException : TssException
{
    public override int StatusCode => (int)HttpStatusCode.Conflict;
    
    public ConflictTssException()
    {
    }

    public ConflictTssException(string message) : base(message)
    {
    }

    public ConflictTssException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public ConflictTssException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
