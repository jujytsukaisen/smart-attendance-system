using System;

namespace SmartAttendance.Domain.Entities;

public abstract class BaseTenantEntity : BaseAuditableEntity
{
    public Guid CompanyId { get; protected set; }
}
