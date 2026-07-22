using System;
using System.Collections.Generic;
using SmartAttendance.Domain.Interfaces;

namespace SmartAttendance.Domain.Entities;

public abstract class BaseEntity
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTimeOffset CreatedAt { get; internal set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; internal set; }
    public bool IsDeleted { get; internal set; }
    public DateTimeOffset? DeletedAt { get; internal set; }
    public byte[] RowVersion { get; internal set; } = null!;

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public override bool Equals(object? obj)
    {
        if (obj is not BaseEntity other)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (GetType() != other.GetType())
        {
            return false;
        }

        return Id != Guid.Empty && Id == other.Id;
    }

    public override int GetHashCode()
    {
        return (GetType().ToString() + Id).GetHashCode();
    }

    public static bool operator ==(BaseEntity? left, BaseEntity? right)
    {
        if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
        {
            return true;
        }

        if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
        {
            return false;
        }

        return left.Equals(right);
    }

    public static bool operator !=(BaseEntity? left, BaseEntity? right)
    {
        return !(left == right);
    }
}

