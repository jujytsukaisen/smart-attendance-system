using System;
using System.Collections.Generic;
using SmartAttendance.Application.Features.Shifts.DTOs;

using SmartAttendance.Application.Common.Mappings;
using SmartAttendance.Domain.Entities;
namespace SmartAttendance.Application.Features.WorkSchedules.DTOs;

public class WorkScheduleDetailsDto : WorkScheduleDto, IMapFrom<WorkSchedule>
{
    public List<ShiftDto> Shifts { get; set; } = new();
}
