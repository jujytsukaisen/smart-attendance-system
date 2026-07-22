using System;
using Microsoft.AspNetCore.Identity;

namespace SmartAttendance.Infrastructure.Identity.Models;

public class ApplicationUser : IdentityUser<Guid>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? CompanyId { get; set; } // For Tenant mapping if needed
    public Guid DomainUserId { get; set; } // FK to Domain User (single source of truth link)
}
