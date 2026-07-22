using System;
using AutoMapper;
using SmartAttendance.Application.Common.Mappings;
using SmartAttendance.Domain.Entities;

namespace SmartAttendance.Application.Features.Branches.DTOs;

public class BranchListDto : IMapFrom<Branch>
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsHeadOffice { get; set; }
    public bool IsActive { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Branch, BranchListDto>();
    }
}
