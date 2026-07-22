using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.Domain.Entities;
using SmartAttendance.Domain.Enums;
using SmartAttendance.Domain.ValueObjects;
using SmartAttendance.Infrastructure.Identity.Models;

namespace SmartAttendance.Infrastructure.Persistence.Seed;

public static class ApplicationDbContextSeed
{
    public static async Task SeedAsync(
        ApplicationDbContext context, 
        RoleManager<ApplicationRole> roleManager, 
        UserManager<ApplicationUser> userManager)
    {
        await SeedPermissionsAsync(context);
        await SeedRolesAndPermissionsAsync(roleManager, context);
        var companyId = await SeedDefaultEntitiesAsync(context);
        await SeedSuperAdminAsync(context, userManager, roleManager, companyId);
    }

    private static async Task SeedPermissionsAsync(ApplicationDbContext context)
    {
        if (await context.Permissions.AnyAsync())
            return;

        var modules = new[]
        {
            "Companies", "Branches", "Departments", "Employees",
            "Attendance", "AttendancePolicies", "WorkSchedules", "Shifts", "Breaks", "Leaves", "Holidays",
            "Notifications", "AuditLogs", "Reports", "Users", "Roles",
            "Permissions", "Settings"
        };

        var actions = new[] { "View", "Create", "Update", "Delete" };
        var permissions = new List<Permission>();

        foreach (var module in modules)
        {
            foreach (var action in actions)
            {
                var code = $"{module}.{action}";
                var name = $"{action} {module}";
                permissions.Add(Permission.Create(name, code, module, $"Permission to {action.ToLower()} {module.ToLower()}", true));
            }

            if (module is "Attendance" or "Leaves")
            {
                permissions.Add(Permission.Create($"Approve {module}", $"{module}.Approve", module, $"Permission to approve {module.ToLower()}", true));
                permissions.Add(Permission.Create($"Reject {module}", $"{module}.Reject", module, $"Permission to reject {module.ToLower()}", true));
            }
            if (module is "Reports" or "AuditLogs" or "Attendance" or "Leaves")
            {
                permissions.Add(Permission.Create($"Export {module}", $"{module}.Export", module, $"Permission to export {module.ToLower()}", true));
            }
            if (module is "Settings" or "Companies")
            {
                permissions.Add(Permission.Create($"Manage {module}", $"{module}.Manage", module, $"Permission to manage {module.ToLower()}", true));
            }
        }

        await context.Permissions.AddRangeAsync(permissions);
        await context.SaveChangesAsync();
    }

    private static async Task SeedRolesAndPermissionsAsync(RoleManager<ApplicationRole> roleManager, ApplicationDbContext context)
    {
        var rolesToSeed = new Dictionary<string, Func<Permission, bool>>
        {
            { "SuperAdmin", _ => true },
            { "CompanyAdmin", p => p.Module != "Permissions" && p.Module != "AuditLogs" },
            { "HRManager", p => new[] { "Employees", "Attendance", "Leaves", "Reports", "Holidays", "Notifications" }.Contains(p.Module) },
            { "BranchManager", p => new[] { "Branches", "Attendance", "Employees" }.Contains(p.Module) && p.Code != "Employees.Delete" },
            { "Supervisor", p => p.Code == "Attendance.Approve" || p.Code == "Reports.View" },
            { "Employee", p => p.Code == "Attendance.View" || p.Code == "Leaves.View" || p.Code == "Notifications.View" }
        };

        var allPermissions = await context.Permissions.ToListAsync();

        foreach (var roleMap in rolesToSeed)
        {
            var roleName = roleMap.Key;
            var role = await roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                role = new ApplicationRole { Name = roleName, NormalizedName = roleName.ToUpper() };
                await roleManager.CreateAsync(role);
            }

            var existingClaims = await roleManager.GetClaimsAsync(role);
            var expectedPermissions = allPermissions.Where(roleMap.Value).ToList();

            foreach (var permission in expectedPermissions)
            {
                if (!existingClaims.Any(c => c.Type == "Permission" && c.Value == permission.Code))
                {
                    await roleManager.AddClaimAsync(role, new Claim("Permission", permission.Code));
                }
            }
        }
    }

    private static async Task<Guid> SeedDefaultEntitiesAsync(ApplicationDbContext context)
    {
        var company = await context.Companies.FirstOrDefaultAsync();
        if (company == null)
        {
            var email = new Email("admin@smartattendance.local");
            var phone = new PhoneNumber("+1234567890");
            var address = new Address("123 Main St", "Metropolis", "NY", "USA", "10001");

            company = Company.Create(
                "Smart Attendance Demo",
                "Smart Attendance Demo LLC",
                "REG-001",
                "TAX-001",
                email,
                phone,
                address,
                DateTime.UtcNow.AddYears(-1));

            // Force activation
            company.Activate();
            await context.Companies.AddAsync(company);
            await context.SaveChangesAsync();
        }

        var branch = await context.Branches.FirstOrDefaultAsync(b => b.CompanyId == company.Id);
        if (branch == null)
        {
            var email = new Email("admin@smartattendance.local");
            var phone = new PhoneNumber("+1234567890");
            var address = new Address("123 Main St", "Metropolis", "NY", "USA", "10001");

            branch = Branch.Create(
                company.Id,
                "Head Office",
                "HO-001",
                address,
                phone,
                email,
                0,
                0,
                500,
                "UTC");

            await context.Branches.AddAsync(branch);
            await context.SaveChangesAsync();
        }

        var department = await context.Departments.FirstOrDefaultAsync(d => d.CompanyId == company.Id);
        if (department == null)
        {
            department = Department.Create(
                company.Id,
                branch.Id,
                "Administration",
                "DEPT-ADMIN",
                "Administration Department");

            await context.Departments.AddAsync(department);
            await context.SaveChangesAsync();
        }

        var attendancePolicy = await context.AttendancePolicies.FirstOrDefaultAsync(p => p.CompanyId == company.Id);
        if (attendancePolicy == null)
        {
            attendancePolicy = AttendancePolicy.Create(
                company.Id,
                "Standard Policy",
                "POL-STD",
                "Standard working policy",
                AttendanceMode.Hybrid,
                requireFaceRecognition: true,
                requireGPS: true,
                requireApprovedDevice: false,
                requireCompanyNetwork: false,
                allowMobileCheckIn: true,
                allowDesktopCheckIn: true,
                allowWebCheckIn: true,
                allowOfflineAttendance: false,
                allowedRadiusMeters: 500,
                allowMultipleCheckIn: false,
                allowManualAttendance: true,
                requireManagerApproval: true,
                maximumDistanceMeters: 5000);

            attendancePolicy.MarkAsDefault();

            await context.AttendancePolicies.AddAsync(attendancePolicy);
            await context.SaveChangesAsync();
        }

        var workSchedule = await context.WorkSchedules.FirstOrDefaultAsync(w => w.CompanyId == company.Id);
        if (workSchedule == null)
        {
            workSchedule = WorkSchedule.Create(
                company.Id,
                "Standard Schedule",
                "Default standard schedule",
                isFlexible: false,
                weeklyWorkingHours: 40,
                gracePeriodMinutes: 15,
                allowEarlyCheckInMinutes: 30,
                allowLateCheckOutMinutes: 30);

            await context.WorkSchedules.AddAsync(workSchedule);
            await context.SaveChangesAsync();
        }

        var shift = await context.Shifts.FirstOrDefaultAsync(s => s.CompanyId == company.Id);
        if (shift == null)
        {
            shift = Shift.Create(
                company.Id,
                workSchedule.Id,
                "Morning Shift",
                "SH-MORN",
                "Standard morning shift",
                new TimeOnly(8, 0),
                new TimeOnly(17, 0),
                ShiftType.Morning,
                isFlexible: false,
                minimumWorkingHours: 8,
                maximumWorkingHours: 9,
                gracePeriodMinutes: 15,
                allowEarlyCheckInMinutes: 30,
                allowLateCheckOutMinutes: 30);

            await context.Shifts.AddAsync(shift);
            await context.SaveChangesAsync();
        }

        return company.Id;
    }

    private static async Task SeedSuperAdminAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, Guid companyId)
    {
        var superAdminEmail = "admin@smartattendance.local";
        var superAdminUser = await userManager.FindByEmailAsync(superAdminEmail);

        if (superAdminUser == null)
        {
            var domainUser = await context.Users.FirstOrDefaultAsync(u => u.Email.Value == superAdminEmail);
            if (domainUser == null)
            {
                domainUser = User.Create(
                    companyId,
                    "admin",
                    new Email(superAdminEmail));

                domainUser.Activate();

                await context.Users.AddAsync(domainUser);
                await context.SaveChangesAsync();
            }

            superAdminUser = new ApplicationUser
            {
                UserName = "admin",
                Email = superAdminEmail,
                FirstName = "System",
                LastName = "Administrator",
                EmailConfirmed = true,
                CompanyId = companyId,
                DomainUserId = domainUser.Id
            };

            var result = await userManager.CreateAsync(superAdminUser, "Admin@123456");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(superAdminUser, "SuperAdmin");
            }
        }
    }
}
