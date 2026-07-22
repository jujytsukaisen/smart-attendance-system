using System;
using Microsoft.AspNetCore.Identity;

namespace SmartAttendance.Infrastructure.Identity.Models;

public class ApplicationRole : IdentityRole<Guid>
{
    public string? Description { get; set; }
}
