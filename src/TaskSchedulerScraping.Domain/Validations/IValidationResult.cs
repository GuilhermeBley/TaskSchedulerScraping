namespace TaskSchedulerScraping.Domain.Validations;

public interface IValidationResult<T> where T : class
{
    /// <summary>
    /// If ok validation, Result is not null
    /// </summary>
    T? Result { get; }

    /// <summary>
    /// Valid if validation is sucess
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Errors
    /// </summary>
    public IEnumerable<Exception> Errors { get; }

    /// <summary>
    /// Messages if isn't get success
    /// </summary>
    public IEnumerable<string> Messages { get; }
}