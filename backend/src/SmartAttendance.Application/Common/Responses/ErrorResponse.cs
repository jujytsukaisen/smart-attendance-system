using System;

namespace SmartAttendance.Application.Common.Responses;

public class ErrorResponse : ApiResponse
{
    public string[] Errors { get; set; } = Array.Empty<string>();
}
