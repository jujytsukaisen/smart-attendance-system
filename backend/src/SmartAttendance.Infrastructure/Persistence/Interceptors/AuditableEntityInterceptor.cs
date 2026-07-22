using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SmartAttendance.Application.Common.Interfaces;
using SmartAttendance.Domain.Entities;

namespace SmartAttendance.Infrastructure.Persistence.Interceptors;

/// <summary>
/// EF Core SaveChanges interceptor that automatically stamps audit fields
/// (CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, DeletedBy) on all tracked entities
/// deriving from <see cref="BaseEntity"/> and <see cref="BaseAuditableEntity"/>.
/// Also converts hard-deletes into soft-deletes for entities with IsDeleted support.
/// </summary>
public sealed class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AuditableEntityInterceptor(
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTimeProvider)
    {
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ApplyAuditInfo(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyAuditInfo(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyAuditInfo(DbContext? context)
    {
        if (context is null) return;

        var utcNow = _dateTimeProvider.UtcNow;
        var userId = _currentUserService.UserId;

        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = new DateTimeOffset(utcNow, TimeSpan.Zero);
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = new DateTimeOffset(utcNow, TimeSpan.Zero);
                    break;

                case EntityState.Deleted:
                    // Convert hard-delete to soft-delete
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.UpdatedAt = new DateTimeOffset(utcNow, TimeSpan.Zero);

                    // Ensure owned entities aren't hard-deleted either
                    foreach (var reference in entry.References.Where(r => r.TargetEntry != null && r.TargetEntry.Metadata.IsOwned()))
                    {
                        reference.TargetEntry!.State = EntityState.Modified;
                    }
                    break;
            }
        }

        foreach (var entry in context.ChangeTracker.Entries<BaseAuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedBy = userId;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedBy = userId;

                    // If soft-deleted in this save, stamp DeletedBy
                    if (entry.Entity.IsDeleted && entry.Property(nameof(BaseEntity.IsDeleted)).IsModified)
                    {
                        entry.Entity.DeletedBy = userId;
                    }
                    break;
            }
        }
    }
}
