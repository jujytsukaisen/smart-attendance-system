using System;
using AutoMapper;
using SmartAttendance.Application.Common.Mappings;
using SmartAttendance.Domain.Entities;

namespace SmartAttendance.Application.Features.Departments.DTOs;

public class DepartmentDto : IMapFrom<Department>
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ManagerId { get; set; }
    public bool IsActive { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Department, DepartmentDto>();
    }
}
