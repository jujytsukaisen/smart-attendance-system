using AutoMapper;
using SmartAttendance.Application.Features.Companies.DTOs;
using SmartAttendance.Application.Features.Branches.DTOs;
using SmartAttendance.Application.Features.Departments.DTOs;
using SmartAttendance.Application.Features.Employees.DTOs;
using SmartAttendance.Application.Features.WorkSchedules.DTOs;
using SmartAttendance.Application.Features.Shifts.DTOs;
using SmartAttendance.Application.Features.Breaks.DTOs;
using SmartAttendance.Application.Features.Attendances.DTOs;
using SmartAttendance.Application.Features.AttendancePolicies.DTOs;
using SmartAttendance.Domain.Entities;
using System.Reflection;

namespace SmartAttendance.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {


        ApplyMappingsFromAssembly(Assembly.GetExecutingAssembly());
    }

    private void ApplyMappingsFromAssembly(Assembly assembly)
    {
        var types = assembly.GetExportedTypes()
            .Where(t => t.GetInterfaces().Any(i => 
                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMapFrom<>)))
            .ToList();

        foreach (var type in types)
        {
            var instance = System.Activator.CreateInstance(type);

            var methodInfo = type.GetMethod("Mapping") 
                ?? type.GetInterface("IMapFrom`1")?.GetMethod("Mapping");

            methodInfo?.Invoke(instance, new object[] { this });
        }
    }
}
