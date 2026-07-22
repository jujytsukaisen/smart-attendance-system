using System;
using AutoMapper;
using SmartAttendance.Application.Common.Mappings;
using SmartAttendance.Domain.Entities;
using SmartAttendance.Domain.Enums;

namespace SmartAttendance.Application.Features.Employees.DTOs;

public class EmployeeListDto : IMapFrom<Employee>
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid BranchId { get; set; }
    public Guid DepartmentId { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public EmployeeStatus EmployeeStatus { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Employee, EmployeeListDto>()
            .ForMember(d => d.Email, opt => opt.MapFrom(s => s.Email.Value));
    }
}
