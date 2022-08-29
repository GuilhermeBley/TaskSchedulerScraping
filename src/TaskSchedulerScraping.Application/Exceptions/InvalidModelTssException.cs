using System.Net;

namespace TaskSchedulerScraping.Application.Exceptions;

/// <summary>
/// Invalid model state
/// </summary>
public class InvalidModelTssException : TssException
{
    private readonly IEnumerable<Exception> _errors;
    private readonly IEnumerable<string> _messages;

    public override int StatusCode => (int)HttpStatusCode.BadRequest;

    public IEnumerable<Exception> Errors => _errors;
    public override object Body => string.Join('\n', _messages);

    public InvalidModelTssException(IEnumerable<Exception> errors)
    {
        _errors = errors;
        _messages = errors.Select(e => e.Message);
    }
}
