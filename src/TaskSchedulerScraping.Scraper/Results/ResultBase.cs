namespace TaskSchedulerScraping.Scraper.Results;

public class ResultBase
{
    private readonly object? _result;
    private readonly bool _isSucess;
    public bool IsSucess => _isSucess;
    public virtual object? ResultBaseObj => _result;

    protected ResultBase(bool isSucess, object? result)
    {
        _result = result;
        _isSucess = isSucess;
    }

    public static ResultBase GetSucess(object? result = null)
    {
        return new ResultBase(true, result);
    }

    public static ResultBase GetWithError(object? result = null)
    {
        return new ResultBase(false, result);
    }
}

public class ResultBase<T> : ResultBase
{
    private T _result;
    public T Result => _result;
    public override object ResultBaseObj => _result!;

    protected ResultBase(bool isSucess, object? resultBase, T result) : base(isSucess, resultBase)
    {
        _result = result;
    }

    public static ResultBase<T> GetSucess(T result)
    {
        return new ResultBase<T>(true, result, result);
    }

    public static ResultBase<T> GetWithError(T result)
    {
        return new ResultBase<T>(false, result, result);
    }
}