namespace WorkforceExecution.Application.Common;

// Tum handler'larin dondugu generic sonuc sarmalayici.
public class Result<T>
{
    public bool IsSuccess { get; init; }
    public string? Error { get; init; }
    public T? Data { get; init; }

    public static Result<T> Success(T data) => new() { IsSuccess = true, Data = data };
    public static Result<T> Failure(string error) => new() { IsSuccess = false, Error = error };
}
