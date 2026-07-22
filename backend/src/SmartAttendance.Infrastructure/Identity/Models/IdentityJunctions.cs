using System;
using Microsoft.AspNetCore.Identity;

namespace SmartAttendance.Infrastructure.Identity.Models;

public class ApplicationUserClaim : IdentityUserClaim<Guid>
{
}

public class ApplicationRoleClaim : IdentityRoleClaim<Guid>
{
}

public class ApplicationUserRole : IdentityUserRole<Guid>
{
}

public class ApplicationUserLogin : IdentityUserLogin<Guid>
{
}

public class ApplicationUserToken : IdentityUserToken<Guid>
{
}
