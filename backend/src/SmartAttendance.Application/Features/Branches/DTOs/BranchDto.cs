using System;
using AutoMapper;
using SmartAttendance.Application.Common.Mappings;
using SmartAttendance.Domain.Entities;

namespace SmartAttendance.Application.Features.Branches.DTOs;

public class BranchDto : IMapFrom<Branch>
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double RadiusInMeters { get; set; }
    public string TimeZone { get; set; } = string.Empty;
    public bool IsHeadOffice { get; set; }
    public bool IsActive { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Branch, BranchDto>()
            .ForMember(d => d.Email, opt => opt.MapFrom(s => s.Email != null ? s.Email.Value : string.Empty))
            .ForMember(d => d.PhoneNumber, opt => opt.MapFrom(s => s.PhoneNumber != null ? s.PhoneNumber.Value : string.Empty))
            .ForMember(d => d.Address, opt => opt.MapFrom(s => s.Address != null ? s.Address.ToString() : string.Empty));
    }
}
