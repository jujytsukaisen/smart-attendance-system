using System;
using SmartAttendance.Domain.Enums;

using SmartAttendance.Application.Common.Mappings;
using SmartAttendance.Domain.Entities;
namespace SmartAttendance.Application.Features.AttendanceDevices.DTOs;

public class AttendanceDeviceDto : IMapFrom<AttendanceDevice>
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? SerialNumber { get; set; }
    
    public DeviceType DeviceType { get; set; }
    public DeviceStatus DeviceStatus { get; set; }
    public ConnectionStatus ConnectionStatus { get; set; }
    
    public bool IsActive { get; set; }
}
