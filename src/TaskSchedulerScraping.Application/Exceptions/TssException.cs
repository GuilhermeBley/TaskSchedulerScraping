using System.Runtime.Serialization;

namespace TaskSchedulerScraping.Application.Exceptions;

public abstract class TssException : Exception
{
    /// <summary>
    /// Code identifier exception
    /// </summary>
    /// <Remarks>
    ///     <para>should be between codes <b>4XX</b></para>
    /// </Remarks>
    public abstract int StatusCode { get; }

    protected TssException()
    {
    }

    protected TssException(string message) : base(message)
    {
    }

    protected TssException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    protected TssException(string message, Exception innerException) : base(message, innerException)
    {
    }
}