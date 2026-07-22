using System;
using SmartAttendance.Domain.Interfaces;

namespace SmartAttendance.Domain.Events;

public abstract class DomainEvent : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
