using Microsoft.AspNetCore.Mvc;
using VeterinaryApi.Common.Errors;

namespace VeterinaryApi.Common.Result;

public class Result
{
    public bool IsSuccess { get; private set; }
    public Error? Error { get; private set; }

    protected Result(bool isSuccess, Error? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }
    public static Result Success()
    {
        return new Result(true, null);
    }
    public static Result Failure(Error error)
    {
        return new Result(false, error);
    }
}
