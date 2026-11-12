using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Domain.Entities;

namespace SmartAttendance.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Company> Companies { get; }
    DbSet<Branch> Branches { get; }
    DbSet<Department> Departments { get; }
    DbSet<Employee> Employees { get; }
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<FaceProfile> FaceProfiles { get; }
    DbSet<WorkSchedule> WorkSchedules { get; }
    DbSet<Shift> Shifts { get; }
    DbSet<Break> Breaks { get; }
    DbSet<AttendancePolicy> AttendancePolicies { get; }
    DbSet<AttendanceDevice> AttendanceDevices { get; }
    DbSet<Attendance> Attendances { get; }
    DbSet<AttendanceEvent> AttendanceEvents { get; }
    DbSet<Leave> Leaves { get; }
    DbSet<Holiday> Holidays { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<AppSetting> AppSettings { get; }
    
    Guid CurrentTenantId { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
