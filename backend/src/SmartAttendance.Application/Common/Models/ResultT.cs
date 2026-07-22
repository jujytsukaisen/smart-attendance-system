using System;
using System.Collections.Generic;

namespace SmartAttendance.Application.Common.Models;

public class Result<T> : Result
{
    public T? Data { get; private set; }

    private Result(bool succeeded, T? data, IEnumerable<string> errors) : base(succeeded, errors)
    {
        Data = data;
    }

    public static Result<T> Success(T data) => new(true, data, Array.Empty<string>());
    public static new Result<T> Failure(IEnumerable<string> errors) => new(false, default, errors);
}
