using System;
using System.Collections.Generic;
using SmartAttendance.Application.Features.Breaks.DTOs;

using SmartAttendance.Application.Common.Mappings;
using SmartAttendance.Domain.Entities;
using SmartAttendance.Domain.Enums;
namespace SmartAttendance.Application.Features.Shifts.DTOs;

public class ShiftDetailsDto : IMapFrom<Shift>
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid WorkScheduleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public ShiftType ShiftType { get; set; }
    public bool IsNightShift { get; set; }
    public bool IsFlexible { get; set; }
    public double MinimumWorkingHours { get; set; }
    public double MaximumWorkingHours { get; set; }
    public int GracePeriodMinutes { get; set; }
    public int AllowEarlyCheckInMinutes { get; set; }
    public int AllowLateCheckOutMinutes { get; set; }
    public int MaxBreakDurationMinutes { get; set; }
    public bool IsActive { get; set; }

    public IReadOnlyCollection<BreakDto> Breaks { get; set; } = Array.Empty<BreakDto>();
}