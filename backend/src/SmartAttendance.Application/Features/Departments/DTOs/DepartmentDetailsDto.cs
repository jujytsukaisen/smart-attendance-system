using System;
using System.Collections.Generic;
using AutoMapper;
using SmartAttendance.Application.Common.Mappings;
using SmartAttendance.Domain.Entities;

namespace SmartAttendance.Application.Features.Departments.DTOs;

public class DepartmentDetailsDto : IMapFrom<Department>
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ManagerId { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Department, DepartmentDetailsDto>()
            .ForMember(d => d.BranchName, opt => opt.MapFrom(s => s.Branch != null ? s.Branch.Name : string.Empty));
    }
}
