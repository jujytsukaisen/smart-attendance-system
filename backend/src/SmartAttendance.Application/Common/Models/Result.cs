using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartAttendance.Application.Common.Models;

public class Result
{
    public bool Succeeded { get; protected set; }
    public string[] Errors { get; protected set; }

    internal Result(bool succeeded, IEnumerable<string> errors)
    {
        Succeeded = succeeded;
        Errors = errors.ToArray();
    }

    public static Result Success() => new(true, Array.Empty<string>());
    public static Result Failure(IEnumerable<string> errors) => new(false, errors);
    public static Result Forbidden() => new Result(false, new[] { "Access denied." }) { IsForbidden = true };

    public bool IsForbidden { get; protected set; }
}
