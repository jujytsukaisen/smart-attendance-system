using System.Collections.Generic;
using System;

namespace SmartAttendance.Application.Common.Responses;

public class ValidationErrorResponse : ApiResponse
{
    public IDictionary<string, string[]> ValidationErrors { get; set; } = new Dictionary<string, string[]>();
}
