using System;

namespace SmartAttendance.Application.Common.Interfaces;

/// <summary>
/// Execution context to bypass tenant filters during system operations 
/// (e.g., seeding, background jobs).
/// </summary>
public interface ISystemExecutionContext
{
    bool IsSystemOperation { get; }
    IDisposable BeginSystemOperation();
}
