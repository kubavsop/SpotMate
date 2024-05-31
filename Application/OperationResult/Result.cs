namespace SpotMate.Application.OperationResult;

public class Result
{
    protected Result(Exception exception)
    {
        IsSuccess = false;
        _exception = exception;
    }
    protected Result()
    {
        IsSuccess = true;
        _exception = null;
    }
    
    protected Exception? _exception { get; }
    public bool IsSuccess { get; }
    
    public bool IsFailure => !IsSuccess;

    public static Result Success() => new Result();
    
    public static implicit operator Result(Exception exception) => new (exception);
    
    public Exception Exception => IsFailure
        ? _exception!
        : throw new InvalidOperationException("The result is not failure");

    public TResult Match<TResult>(Func<TResult> onSuccess, Func<Exception, TResult> onFailure) =>
        IsSuccess ? onSuccess() : onFailure(_exception);
}