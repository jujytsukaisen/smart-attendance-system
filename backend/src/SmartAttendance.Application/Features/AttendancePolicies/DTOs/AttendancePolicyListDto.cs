using System;

using SmartAttendance.Application.Common.Mappings;
using SmartAttendance.Domain.Entities;
namespace SmartAttendance.Application.Features.AttendancePolicies.DTOs;

public class AttendancePolicyListDto : IMapFrom<AttendancePolicy>
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public SmartAttendance.Domain.Enums.AttendanceMode AttendanceMode { get; set; }
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
}
