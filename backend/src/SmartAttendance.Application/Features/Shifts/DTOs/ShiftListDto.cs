using System;

using SmartAttendance.Application.Common.Mappings;
using SmartAttendance.Domain.Entities;
using SmartAttendance.Domain.Enums;
namespace SmartAttendance.Application.Features.Shifts.DTOs;

public class ShiftListDto : IMapFrom<Shift>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public ShiftType ShiftType { get; set; }
    public bool IsActive { get; set; }
    public Guid WorkScheduleId { get; set; }
}