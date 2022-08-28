using System.Net;

namespace TaskSchedulerScraping.Application.Exceptions;

/// <summary>
/// Invalid model state
/// </summary>
public class InvalidModelTssException : TssException
{
    private readonly IEnumerable<string> _errors;

    public override int StatusCode => (int)HttpStatusCode.BadRequest;

    public override object Body => _errors;

    public InvalidModelTssException(IEnumerable<string> errors)
    {
        _errors = errors;
    }
}
