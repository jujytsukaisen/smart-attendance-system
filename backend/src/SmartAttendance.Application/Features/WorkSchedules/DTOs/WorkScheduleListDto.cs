using System;

using SmartAttendance.Application.Common.Mappings;
using SmartAttendance.Domain.Entities;
namespace SmartAttendance.Application.Features.WorkSchedules.DTOs;

public class WorkScheduleListDto : IMapFrom<WorkSchedule>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public bool IsFlexible { get; set; }
    public bool IsActive { get; set; }
    public double WeeklyWorkingHours { get; set; }
}
