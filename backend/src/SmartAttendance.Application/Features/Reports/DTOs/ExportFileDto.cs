using System;

namespace SmartAttendance.Application.Features.Reports.DTOs;

public class ExportFileDto
{
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}
