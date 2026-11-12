using System;
using AutoMapper;
using SmartAttendance.Application.Common.Mappings;
using SmartAttendance.Domain.Entities;
using SmartAttendance.Domain.Enums;

namespace SmartAttendance.Application.Features.Attendances.DTOs;

public class AttendanceDto : IMapFrom<Attendance>
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeNumber { get; set; } = string.Empty;
    public Guid AttendancePolicyId { get; set; }
    public Guid ShiftId { get; set; }
    public DateTime CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public AttendanceStatus AttendanceStatus { get; set; }
    public int WorkingMinutes { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Attendance, AttendanceDto>()
            .ForMember(d => d.EmployeeName, opt => opt.MapFrom(s => s.Employee != null ? (s.Employee.FirstName + " " + s.Employee.LastName).Trim() : string.Empty))
            .ForMember(d => d.EmployeeNumber, opt => opt.MapFrom(s => s.Employee != null ? s.Employee.EmployeeNumber : string.Empty));
    }
}
