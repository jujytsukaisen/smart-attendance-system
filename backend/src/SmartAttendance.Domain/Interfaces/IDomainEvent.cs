using System;

namespace SmartAttendance.Domain.Interfaces;

public interface IDomainEvent
{
    DateTimeOffset OccurredOn { get; }
}
