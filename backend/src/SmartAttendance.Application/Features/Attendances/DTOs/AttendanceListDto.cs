using System;
using AutoMapper;
using SmartAttendance.Application.Common.Mappings;
using SmartAttendance.Domain.Entities;
using SmartAttendance.Domain.Enums;

namespace SmartAttendance.Application.Features.Attendances.DTOs;

public class AttendanceListDto : IMapFrom<Attendance>
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeNumber { get; set; } = string.Empty;
    public DateTime CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public AttendanceStatus AttendanceStatus { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Attendance, AttendanceListDto>()
            .ForMember(d => d.EmployeeName, opt => opt.MapFrom(s => s.Employee != null ? (s.Employee.FirstName + " " + s.Employee.LastName).Trim() : string.Empty))
            .ForMember(d => d.EmployeeNumber, opt => opt.MapFrom(s => s.Employee != null ? s.Employee.EmployeeNumber : string.Empty));
    }
}
