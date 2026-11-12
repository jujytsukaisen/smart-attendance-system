using System.Reflection;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Domain.Entities;
using SmartAttendance.Infrastructure.Persistence.Extensions;

using SmartAttendance.Infrastructure.Identity.Models;

namespace SmartAttendance.Infrastructure.Persistence;

/// <summary>
/// The EF Core DbContext for the Smart Attendance System.
/// Implements <see cref="IApplicationDbContext"/> and <see cref="IUnitOfWork"/>
/// to serve as the single persistence gateway for the application layer.
/// </summary>
public class ApplicationDbContext : global::Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext<ApplicationUser, ApplicationRole, Guid, ApplicationUserClaim, ApplicationUserRole, ApplicationUserLogin, ApplicationRoleClaim, ApplicationUserToken>, IApplicationDbContext, IUnitOfWork
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ISystemExecutionContext _systemExecutionContext;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService currentUserService,
        ISystemExecutionContext systemExecutionContext)
        : base(options)
    {
        _currentUserService = currentUserService;
        _systemExecutionContext = systemExecutionContext;
    }

    // ──────────────────────────────────────────────
    // DbSets — matching IApplicationDbContext
    // ──────────────────────────────────────────────
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Employee> Employees => Set<Employee>();
    public new DbSet<User> Users => Set<User>();
    public new DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<FaceProfile> FaceProfiles => Set<FaceProfile>();
    public DbSet<WorkSchedule> WorkSchedules => Set<WorkSchedule>();
    public DbSet<Shift> Shifts => Set<Shift>();
    public DbSet<Break> Breaks => Set<Break>();
    public DbSet<AttendancePolicy> AttendancePolicies => Set<AttendancePolicy>();
    public DbSet<AttendanceDevice> AttendanceDevices => Set<AttendanceDevice>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<AttendanceEvent> AttendanceEvents => Set<AttendanceEvent>();
    public DbSet<Leave> Leaves => Set<Leave>();
    public DbSet<Holiday> Holidays => Set<Holiday>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();

    // ──────────────────────────────────────────────
    // IUnitOfWork implementation
    // ──────────────────────────────────────────────
    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        return await SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        HandleSqliteRowVersion();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        HandleSqliteRowVersion();
        return base.SaveChanges();
    }

    private void HandleSqliteRowVersion()
    {
        if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                {
                    entry.Entity.RowVersion = Guid.NewGuid().ToByteArray();
                }
            }
        }
    }

    // ──────────────────────────────────────────────
    // Model Configuration
    // ──────────────────────────────────────────────
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all IEntityTypeConfiguration<T> classes from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Ignore DomainEvents — they are transient and not persisted
        modelBuilder.IgnoreDomainEventProperties();

        // Apply global query filters
        ApplyGlobalFilters(modelBuilder);
    }

    public Guid CurrentTenantId => Guid.TryParse(_currentUserService.TenantId, out var tenantId) ? tenantId : Guid.Empty;
    public bool IsSuperAdmin => _currentUserService.Roles.Contains("SuperAdmin");
    public bool IsSystemOperation => _systemExecutionContext.IsSystemOperation;

    private void ApplyGlobalFilters(ModelBuilder modelBuilder)
    {
        var method = typeof(ApplicationDbContext).GetMethod(nameof(ApplyTenantFilterGeneric), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.IsOwned()) continue;

            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var builder = modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(BaseEntity.RowVersion));

                if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
                {
                    builder.IsConcurrencyToken().ValueGeneratedNever();
                }
                else
                {
                    builder.IsRowVersion();
                }
            }

            if (typeof(BaseTenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                method?.MakeGenericMethod(entityType.ClrType).Invoke(this, new object[] { modelBuilder });
            }
            else if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                // Soft delete filter for non-tenant entities
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var property = System.Linq.Expressions.Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                var condition = System.Linq.Expressions.Expression.Equal(property, System.Linq.Expressions.Expression.Constant(false));
                var lambda = System.Linq.Expressions.Expression.Lambda(condition, parameter);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }

    private void ApplyTenantFilterGeneric<T>(ModelBuilder modelBuilder) where T : BaseTenantEntity
    {
        modelBuilder.Entity<T>().HasQueryFilter(e => !e.IsDeleted && (IsSystemOperation || IsSuperAdmin || e.CompanyId == CurrentTenantId));
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        // Global convention: all string properties max 500 characters unless overridden
        configurationBuilder.Properties<string>()
            .HaveMaxLength(500);

        // Global convention: all decimal properties with precision 18,2 unless overridden
        configurationBuilder.Properties<decimal>()
            .HavePrecision(18, 2);
    }
}
