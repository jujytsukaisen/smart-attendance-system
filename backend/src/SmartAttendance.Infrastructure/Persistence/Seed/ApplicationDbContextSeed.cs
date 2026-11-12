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
        await SeedDefaultSettingsAsync(context, companyId);
        await FixPendingAttendancesAsync(context);
    }

    private static async Task SeedPermissionsAsync(ApplicationDbContext context)
    {
        var modules = new[]
        {
            "Companies", "Branches", "Departments", "Employees",
            "Attendance", "AttendancePolicies", "AttendanceDevices", "WorkSchedules", "Shifts", "Breaks", "Leaves", "Holidays",
            "Notifications", "AuditLogs", "Reports", "Users", "Roles",
            "Permissions", "Settings", "FaceProfiles", "Dashboard"
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

        // ⚠️ تصحيح جوهري: بدل "أضف الكل أو لا شيء" (كان يمنع أي إضافة لاحقة لوحدات جديدة)
        // نضيف فقط الصلاحيات التي لم تكن موجودة أصلاً في قاعدة البيانات
        var existingCodes = (await context.Permissions.Select(p => p.Code).ToListAsync()).ToHashSet();
        var missingPermissions = permissions.Where(p => !existingCodes.Contains(p.Code)).ToList();

        if (missingPermissions.Count > 0)
        {
            await context.Permissions.AddRangeAsync(missingPermissions);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedRolesAndPermissionsAsync(RoleManager<ApplicationRole> roleManager, ApplicationDbContext context)
    {
        // ─── نظام الأدوار المبسّط: دورين فقط ───────────────────────────────
        // Admin    = كل الصلاحيات
        // Employee = صلاحيات الموظف الخاصة به فقط (حضوره + إجازاته + إشعاراته)
        var rolesToSeed = new Dictionary<string, Func<Permission, bool>>
        {
            { "Admin", _ => true },
            { "Employee", p =>
                p.Code == "Attendance.View"    ||
                p.Code == "Attendance.Create"  ||
                p.Code == "Leaves.View"        ||
                p.Code == "Leaves.Create"      ||
                p.Code == "Notifications.View"
            }
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

    var branch = await context.Branches
            .FirstOrDefaultAsync(b => b.CompanyId == company.Id);

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

        var department = await context.Departments
            .FirstOrDefaultAsync(d => d.CompanyId == company.Id);
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
                new TimeOnly(16, 0),
                ShiftType.Morning,
                isFlexible: false,
                minimumWorkingHours: 8,
                maximumWorkingHours: 8,
                gracePeriodMinutes: 15,
                allowEarlyCheckInMinutes: 30,
                allowLateCheckOutMinutes: 30,
                maxBreakDurationMinutes: 60);

            await context.Shifts.AddAsync(shift);
            await context.SaveChangesAsync();
        }

        var device = await context.AttendanceDevices.FirstOrDefaultAsync(d => d.CompanyId == company.Id);
        if (device == null)
        {
            device = AttendanceDevice.Create(
                company.Id,
                "Main Web Kiosk",
                "DEV-WEB-001",
                "WEB-001-SERIAL",
                DeviceType.Web,
                "Web Browser",
                "1.0.0",
                branch.Id,
                null,
                null,
                null,
                null,
                null,
                false,
                true,
                true,
                false,
                true,
                false,
                true,
                false,
                "127.0.0.1",
                80,
                CommunicationProtocol.HTTPS);

            device.Activate();
            await context.AttendanceDevices.AddAsync(device);
            await context.SaveChangesAsync();
        }

        return company.Id;
    }

    private static async Task SeedSuperAdminAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, Guid companyId)
    {
        // 1. Ensure mm0571918@gmail.com exists and has the Employee role so they can create leave requests
        var empEmail = "mm0571918@gmail.com";
        var empDomainUser = await context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Email.Value == empEmail);
        if (empDomainUser == null)
        {
            empDomainUser = User.Create(companyId, empEmail.Split('@')[0], new Email(empEmail));
            empDomainUser.Activate();
            await context.Users.AddAsync(empDomainUser);
            await context.SaveChangesAsync();
        }

        var targetUser = await userManager.FindByEmailAsync(empEmail);
        if (targetUser == null)
        {
            targetUser = new ApplicationUser
            {
                UserName = empEmail.Split('@')[0],
                Email = empEmail,
                FirstName = "موظف",
                LastName = "جديد",
                EmailConfirmed = true,
                CompanyId = companyId,
                DomainUserId = empDomainUser.Id
            };
            await userManager.CreateAsync(targetUser, "Employee@123456");
        }

        if (!await userManager.IsInRoleAsync(targetUser, "Employee"))
        {
            await userManager.AddToRoleAsync(targetUser, "Employee");
        }

        var employeeRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Employee");
        if (employeeRole != null && !empDomainUser.Roles.Any(r => r.Name == "Employee"))
        {
            empDomainUser.AssignRole(employeeRole);
            context.Users.Update(empDomainUser);
            await context.SaveChangesAsync();
        }

        var employee = await context.Employees.FirstOrDefaultAsync(e => e.UserId == empDomainUser.Id);
        if (employee == null)
        {
            var branch = await context.Branches.FirstOrDefaultAsync(b => b.CompanyId == companyId);
            var dept = await context.Departments.FirstOrDefaultAsync(d => d.CompanyId == companyId);
            var policy = await context.AttendancePolicies.FirstOrDefaultAsync(p => p.CompanyId == companyId);

            if (branch != null && dept != null && policy != null)
            {
                employee = Employee.Create(
                    companyId,
                    "EMP-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                    "موظف",
                    "جديد",
                    new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    Gender.Male,
                    DateTime.UtcNow.AddYears(-1),
                    "مطور برمجيات",
                    branch.Id,
                    dept.Id,
                    policy.Id,
                    new Email(empEmail),
                    new PhoneNumber("+1234567890"),
                    new Address("123", "City", "State", "Country", "12345"));
                
                employee.AssignUser(empDomainUser.Id);
                employee.Activate();
                await context.Employees.AddAsync(employee);
                await context.SaveChangesAsync();
            }
        }

        var adminEmails = new[] { "admin@smartattendance.local" };

        foreach (var adminEmail in adminEmails)
        {
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            var domainUser = await context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Email.Value == adminEmail);
            var adminDomainRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");

            if (domainUser == null)
            {
                domainUser = User.Create(
                    companyId,
                    adminEmail.Split('@')[0],
                    new Email(adminEmail));

                domainUser.Activate();

                await context.Users.AddAsync(domainUser);
                await context.SaveChangesAsync();
            }

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail.Split('@')[0],
                    Email = adminEmail,
                    FirstName = "المدير",
                    LastName = "العام",
                    EmailConfirmed = true,
                    CompanyId = companyId,
                    DomainUserId = domainUser.Id
                };

                await userManager.CreateAsync(adminUser, "Admin@123456");
            }
            else if (adminUser.DomainUserId == Guid.Empty)
            {
                adminUser.DomainUserId = domainUser.Id;
                adminUser.CompanyId = companyId;
                await userManager.UpdateAsync(adminUser);
            }

            // تأكد من أن المستخدم لديه دور Admin فقط (وليس الأدوار القديمة)
            var oldRoles = new[] { "SuperAdmin", "CompanyAdmin", "HRManager", "BranchManager", "Supervisor" };
            foreach (var oldRole in oldRoles)
            {
                if (await userManager.IsInRoleAsync(adminUser, oldRole))
                    await userManager.RemoveFromRoleAsync(adminUser, oldRole);
            }

            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                await userManager.AddToRoleAsync(adminUser, "Admin");

            if (adminDomainRole != null)
            {
                await context.Entry(domainUser).Collection(u => u.Roles).LoadAsync();

                // إزالة الأدوار القديمة من domain user
                var oldDomainRoles = domainUser.Roles
                    .Where(r => oldRoles.Contains(r.Name))
                    .ToList();
                foreach (var old in oldDomainRoles)
                    domainUser.RemoveRole(old);

                if (!domainUser.Roles.Any(r => r.Name == "Admin"))
                {
                    domainUser.AssignRole(adminDomainRole);
                }

                context.Users.Update(domainUser);
                await context.SaveChangesAsync();
            }
        }
    }

    private static async Task SeedDefaultSettingsAsync(ApplicationDbContext context, Guid companyId)
    {
        // Only seed if no settings exist for this company yet
        var hasSettings = await context.AppSettings.AnyAsync(s => s.CompanyId == companyId);
        if (hasSettings) return;

        var defaults = new List<(string Category, string Key, string Value)>
        {
            // General
            ("General", "CompanyName", "Smart Attendance Demo"),
            ("General", "DateFormat", "DD/MM/YYYY"),
            ("General", "TimeFormat", "HH:mm"),
            ("General", "TimeZone", "UTC"),
            ("General", "Language", "en"),
            ("General", "Theme", "Light"),
            // Attendance
            ("Attendance", "WorkingHoursStart", "08:00"),
            ("Attendance", "WorkingHoursEnd", "17:00"),
            ("Attendance", "GracePeriodMinutes", "15"),
            ("Attendance", "LateThresholdMinutes", "30"),
            ("Attendance", "MinimumHoursForHalfDay", "4"),
            ("Attendance", "MinimumHoursForFullDay", "8"),
            ("Attendance", "MaximumOvertimeHours", "3"),
            ("Attendance", "WeekendRules", "Saturday,Sunday"),
            ("Attendance", "HolidayRules", "National"),
            // Security
            ("Security", "PasswordPolicy", "Strong"),
            ("Security", "SessionTimeoutMinutes", "60"),
            ("Security", "JwtLifetimeMinutes", "60"),
            ("Security", "RefreshTokenLifetimeDays", "7"),
            ("Security", "TwoFactorAuthEnabled", "False"),
            ("Security", "MaxLoginAttempts", "5"),
            ("Security", "LockoutDurationMinutes", "15"),
            // Notifications
            ("Notifications", "EmailNotificationsEnabled", "True"),
            ("Notifications", "SmsNotificationsEnabled", "False"),
            ("Notifications", "PushNotificationsEnabled", "True"),
            ("Notifications", "AttendanceAlertsEnabled", "True"),
            ("Notifications", "LeaveAlertsEnabled", "True"),
            ("Notifications", "DeviceAlertsEnabled", "True"),
            // Devices
            ("Devices", "DefaultDeviceTimeoutSeconds", "30"),
            ("Devices", "HeartbeatIntervalSeconds", "60"),
            ("Devices", "AutoSyncEnabled", "True"),
            ("Devices", "RetryAttempts", "3"),
            ("Devices", "ConnectionTimeoutSeconds", "10"),
            // Email
            ("Email", "SmtpHost", "smtp.example.com"),
            ("Email", "SmtpPort", "587"),
            ("Email", "EnableSsl", "True"),
            ("Email", "Username", ""),
            ("Email", "SenderName", "SmartAttendance"),
            ("Email", "SenderEmail", "noreply@smartattendance.local"),
            // SMS
            ("Sms", "Provider", "Twilio"),
            ("Sms", "ApiKey", ""),
            ("Sms", "SenderName", "SmartAttendance"),
            // Backup
            ("Backup", "AutomaticBackupEnabled", "False"),
            ("Backup", "BackupTime", "00:00"),
            ("Backup", "RetentionDays", "30"),
            ("Backup", "StorageProvider", "Local"),
            // FaceRecognition
            ("FaceRecognition", "RecognitionThreshold", "0.8"),
            ("FaceRecognition", "MinimumFaceQuality", "0.6"),
            ("FaceRecognition", "LivenessDetectionEnabled", "True"),
            ("FaceRecognition", "MaskDetectionEnabled", "False"),
            ("FaceRecognition", "SpoofDetectionEnabled", "True"),
        };

        var settings = defaults.Select(d => AppSetting.Create(companyId, d.Category, d.Key, d.Value)).ToList();
        await context.AppSettings.AddRangeAsync(settings);
        await context.SaveChangesAsync();
    }

    private static async Task FixPendingAttendancesAsync(ApplicationDbContext context)
    {
        var shifts = await context.Shifts.Where(s => s.Code == "SH-MORN").ToListAsync();
        foreach (var s in shifts)
        {
            s.ChangeWorkingHours(new TimeOnly(8, 0), new TimeOnly(16, 0), 8, 8);
        }
        await context.SaveChangesAsync();

        var attendancesWithCheckOut = await context.Attendances
            .Include(a => a.Shift)
            .Include(a => a.AttendancePolicy)
            .Where(a => a.CheckOutTime != null && !a.IsDeleted)
            .ToListAsync();

        if (!attendancesWithCheckOut.Any()) return;

        foreach (var attendance in attendancesWithCheckOut)
        {
            // CheckOutTime is guaranteed non-null by the Where filter above (a.CheckOutTime != null)
            if (attendance.CheckOutTime is not { } checkOutTime) continue;

            int lateMinutes = 0;
            int earlyLeaveMinutes = 0;
            int overtimeMinutes = 0;

            if (attendance.Shift != null)
            {
                var shiftStart = attendance.CheckInTime.Date.Add(attendance.Shift.StartTime.ToTimeSpan());
                var shiftEnd = attendance.CheckInTime.Date.Add(attendance.Shift.EndTime.ToTimeSpan());

                var gracePeriod = attendance.AttendancePolicy?.GracePeriodMinutes ?? 0;
                lateMinutes = Math.Max(0, (int)(attendance.CheckInTime - shiftStart).TotalMinutes - gracePeriod);

                var earlyLeaveTolerance = attendance.AttendancePolicy?.EarlyLeaveToleranceMinutes ?? 0;
                earlyLeaveMinutes = Math.Max(0, (int)(shiftEnd - checkOutTime).TotalMinutes - earlyLeaveTolerance);

                overtimeMinutes = Math.Max(0, (int)(checkOutTime - shiftEnd).TotalMinutes);
            }

            attendance.CalculateWorkingHours(lateMinutes, earlyLeaveMinutes, breakMinutes: 0, overtimeMinutes: overtimeMinutes);
        }

        await context.SaveChangesAsync();
    }
}