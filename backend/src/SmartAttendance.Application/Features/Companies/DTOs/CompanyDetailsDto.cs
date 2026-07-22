using System;
using AutoMapper;
using SmartAttendance.Application.Common.Mappings;
using SmartAttendance.Domain.Entities;

namespace SmartAttendance.Application.Features.Companies.DTOs;

public class CompanyDetailsDto : IMapFrom<Company>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LegalName { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string TaxNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    
    // Address fields
    public string AddressStreet { get; set; } = string.Empty;
    public string AddressCity { get; set; } = string.Empty;
    public string AddressState { get; set; } = string.Empty;
    public string AddressCountry { get; set; } = string.Empty;
    public string AddressZipCode { get; set; } = string.Empty;

    public string? Website { get; set; }
    public string? LogoUrl { get; set; }
    public string TimeZone { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime FoundedDate { get; set; }

    // Navigation Counts
    public int BranchesCount { get; set; }
    public int DepartmentsCount { get; set; }
    public int EmployeesCount { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Company, CompanyDetailsDto>()
            .ForMember(d => d.Email, opt => opt.MapFrom(s => s.Email != null ? s.Email.Value : string.Empty))
            .ForMember(d => d.PhoneNumber, opt => opt.MapFrom(s => s.PhoneNumber != null ? s.PhoneNumber.Value : string.Empty))
            .ForMember(d => d.AddressStreet, opt => opt.MapFrom(s => s.Address != null ? s.Address.Street : string.Empty))
            .ForMember(d => d.AddressCity, opt => opt.MapFrom(s => s.Address != null ? s.Address.City : string.Empty))
            .ForMember(d => d.AddressState, opt => opt.MapFrom(s => s.Address != null ? s.Address.State : string.Empty))
            .ForMember(d => d.AddressCountry, opt => opt.MapFrom(s => s.Address != null ? s.Address.Country : string.Empty))
            .ForMember(d => d.AddressZipCode, opt => opt.MapFrom(s => s.Address != null ? s.Address.ZipCode : string.Empty))
            .ForMember(d => d.BranchesCount, opt => opt.MapFrom(s => s.Branches.Count))
            .ForMember(d => d.DepartmentsCount, opt => opt.MapFrom(s => s.Departments.Count))
            .ForMember(d => d.EmployeesCount, opt => opt.MapFrom(s => s.Employees.Count));
    }
}
