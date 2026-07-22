using System;

using SmartAttendance.Application.Common.Mappings;
using SmartAttendance.Domain.Entities;
namespace SmartAttendance.Application.Features.AttendancePolicies.DTOs;

public class AttendancePolicyDetailsDto : AttendancePolicyDto, IMapFrom<AttendancePolicy>
{
    // Any extra nested information if required, currently mapped same as Dto
}
