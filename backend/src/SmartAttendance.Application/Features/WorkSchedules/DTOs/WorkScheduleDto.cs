using System;

using SmartAttendance.Application.Common.Mappings;
using SmartAttendance.Domain.Entities;
namespace SmartAttendance.Application.Features.WorkSchedules.DTOs;

public class WorkScheduleDto : IMapFrom<WorkSchedule>
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDefault { get; set; }
    public bool IsFlexible { get; set; }
    public bool IsActive { get; set; }
    public double WeeklyWorkingHours { get; set; }
    public int GracePeriodMinutes { get; set; }
    public int AllowEarlyCheckInMinutes { get; set; }
    public int AllowLateCheckOutMinutes { get; set; }
}
