using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SmartAttendance.Domain.Entities;

namespace SmartAttendance.Infrastructure.Persistence.Interceptors;

/// <summary>
/// EF Core SaveChanges interceptor that dispatches domain events collected from
/// all tracked <see cref="BaseEntity"/> instances after a successful save.
/// Events are cleared from the entities before dispatch to prevent re-publishing.
/// </summary>
public sealed class DomainEventDispatcherInterceptor : SaveChangesInterceptor
{
    private readonly IPublisher _publisher;

    public DomainEventDispatcherInterceptor(IPublisher publisher)
    {
        _publisher = publisher;
    }

    public override int SavedChanges(
        SaveChangesCompletedEventData eventData,
        int result)
    {
        DispatchDomainEventsAsync(eventData.Context).GetAwaiter().GetResult();
        return base.SavedChanges(eventData, result);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        await DispatchDomainEventsAsync(eventData.Context, cancellationToken);
        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private async Task DispatchDomainEventsAsync(
        DbContext? context,
        CancellationToken cancellationToken = default)
    {
        if (context is null) return;

        var entities = context.ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = entities
            .SelectMany(e => e.DomainEvents)
            .ToList();

        // Clear events before dispatching to prevent infinite loops
        foreach (var entity in entities)
        {
            entity.ClearDomainEvents();
        }

        foreach (var domainEvent in domainEvents)
        {
            await _publisher.Publish(domainEvent, cancellationToken);
        }
    }
}
