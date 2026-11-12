using System;
using SmartAttendance.Application.Common.Interfaces;

namespace SmartAttendance.Infrastructure.Services;

public class SystemExecutionContext : ISystemExecutionContext
{
    public bool IsSystemOperation { get; private set; }

    public IDisposable BeginSystemOperation()
    {
        IsSystemOperation = true;
        return new SystemOperationScope(this);
    }

    private class SystemOperationScope : IDisposable
    {
        private readonly SystemExecutionContext _context;

        public SystemOperationScope(SystemExecutionContext context)
        {
            _context = context;
        }

        public void Dispose()
        {
            _context.IsSystemOperation = false;
        }
    }
}
