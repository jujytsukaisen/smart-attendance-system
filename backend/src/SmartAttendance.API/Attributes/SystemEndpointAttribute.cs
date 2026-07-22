using System;

namespace SmartAttendance.API.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class SystemEndpointAttribute : Attribute
{
}
