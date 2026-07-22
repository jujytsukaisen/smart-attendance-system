using System;
using AutoMapper;
using SmartAttendance.Domain.Entities;

namespace SmartAttendance.Application.Features.Branches.DTOs;

public class BranchDetailsDto : BranchDto
{
    public int TotalDepartments { get; set; }
    public int TotalEmployees { get; set; }
    public int TotalAttendanceDevices { get; set; }

    public new void Mapping(Profile profile)
    {
        profile.CreateMap<Branch, BranchDetailsDto>()
            .IncludeBase<Branch, BranchDto>()
            .ForMember(d => d.TotalDepartments, opt => opt.MapFrom(s => s.Departments.Count))
            .ForMember(d => d.TotalEmployees, opt => opt.MapFrom(s => s.Employees.Count))
            .ForMember(d => d.TotalAttendanceDevices, opt => opt.MapFrom(s => s.AttendanceDevices.Count));
    }
}
