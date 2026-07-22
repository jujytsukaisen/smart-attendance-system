using System;
using AutoMapper;
using SmartAttendance.Application.Common.Mappings;
using SmartAttendance.Domain.Entities;

namespace SmartAttendance.Application.Features.Companies.DTOs;

public class CompanyListDto : IMapFrom<Company>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? LogoUrl { get; set; }
    public DateTime FoundedDate { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Company, CompanyListDto>()
            .ForMember(d => d.Email, opt => opt.MapFrom(s => s.Email != null ? s.Email.Value : string.Empty))
            .ForMember(d => d.PhoneNumber, opt => opt.MapFrom(s => s.PhoneNumber != null ? s.PhoneNumber.Value : string.Empty));
    }
}
