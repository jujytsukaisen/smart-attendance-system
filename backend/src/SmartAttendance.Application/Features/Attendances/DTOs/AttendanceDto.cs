using System;
using SmartAttendance.Application.Common.Mappings;
using SmartAttendance.Domain.Entities;
using SmartAttendance.Domain.Enums;

namespace SmartAttendance.Application.Features.Attendances.DTOs;

public class AttendanceDto : IMapFrom<Attendance>
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public Guid AttendancePolicyId { get; set; }
    public Guid ShiftId { get; set; }
    public DateTime CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public AttendanceStatus AttendanceStatus { get; set; }
    public int WorkingMinutes { get; set; }
}
