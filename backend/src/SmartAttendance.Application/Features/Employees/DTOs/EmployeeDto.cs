using System;
using AutoMapper;
using SmartAttendance.Application.Common.Mappings;
using SmartAttendance.Domain.Entities;
using SmartAttendance.Domain.Enums;

namespace SmartAttendance.Application.Features.Employees.DTOs;

public class EmployeeDto : IMapFrom<Employee>
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid BranchId { get; set; }
    public Guid DepartmentId { get; set; }
    public Guid AttendancePolicyId { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string? PreferredName { get; set; }
    public string? NationalId { get; set; }
    public string? PassportNumber { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public DateTime HireDate { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    
    // Address
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;

    public EmployeeStatus EmployeeStatus { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Employee, EmployeeDto>()
            .ForMember(d => d.Email, opt => opt.MapFrom(s => s.Email.Value))
            .ForMember(d => d.PhoneNumber, opt => opt.MapFrom(s => s.PhoneNumber.Value))
            .ForMember(d => d.Street, opt => opt.MapFrom(s => s.Address.Street))
            .ForMember(d => d.City, opt => opt.MapFrom(s => s.Address.City))
            .ForMember(d => d.State, opt => opt.MapFrom(s => s.Address.State))
            .ForMember(d => d.ZipCode, opt => opt.MapFrom(s => s.Address.ZipCode))
            .ForMember(d => d.Country, opt => opt.MapFrom(s => s.Address.Country));
    }
}
