using System;
using SmartAttendance.Domain.Enums;

using SmartAttendance.Application.Common.Mappings;
using SmartAttendance.Domain.Entities;
namespace SmartAttendance.Application.Features.Breaks.DTOs;

public class BreakDetailsDto : IMapFrom<Break>
{
    public Guid Id { get; set; }
    public Guid ShiftId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int DurationMinutes { get; set; }
    public BreakType BreakType { get; set; }
    public bool IsPaid { get; set; }
    public bool IsMandatory { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
}
