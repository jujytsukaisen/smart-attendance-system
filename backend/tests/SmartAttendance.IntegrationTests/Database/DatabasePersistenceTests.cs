using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SmartAttendance.IntegrationTests.Common;
using Xunit;

namespace SmartAttendance.IntegrationTests.Database;

public class DatabasePersistenceTests : BaseIntegrationTest
{
    public DatabasePersistenceTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task SeedData_ShouldExistInDatabase()
    {
        await ExecuteDbContextAsync(async context =>
        {
            var company = await context.Companies.FirstOrDefaultAsync();
            company.Should().NotBeNull();
            company!.Name.Should().Be("Smart Attendance Demo");

            var admin = await context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Email.Value == "admin@smartattendance.local");
            admin.Should().NotBeNull();
            
            var roles = await context.Set<SmartAttendance.Infrastructure.Identity.Models.ApplicationRole>().ToListAsync();
            roles.Should().NotBeEmpty();
            roles.Should().Contain(r => r.Name == "SuperAdmin");
        });
    }

    [Fact]
    public async Task SoftDelete_ShouldFilterDeletedEntities()
    {
        Guid companyId = Guid.Empty;

        // 1. Arrange & Act: Soft delete a company
        await ExecuteDbContextAsync(async context =>
        {
            var company = await context.Companies.FirstAsync();
            companyId = company.Id;
            
            company.Deactivate(); // Suppose Deactivate sets IsDeleted or similar state.
            
            // To properly test soft delete, let's create a dummy company and delete it via EF core.
            var dummyCompany = SmartAttendance.Domain.Entities.Company.Create(
                "Delete Me", "Delete Me LLC", "REG-DEL", "TAX-DEL", 
                new SmartAttendance.Domain.ValueObjects.Email("del@test.com"), 
                new SmartAttendance.Domain.ValueObjects.PhoneNumber("+111"), 
                new SmartAttendance.Domain.ValueObjects.Address("A", "B", "C", "D", "E"), 
                DateTime.UtcNow);
            
            await context.Companies.AddAsync(dummyCompany);
            await context.SaveChangesAsync();
            
            context.Companies.Remove(dummyCompany); // This should trigger Soft Delete interceptor
            await context.SaveChangesAsync();
        });

        // 2. Assert: Verify it's filtered out
        await ExecuteDbContextAsync(async context =>
        {
            var company = await context.Companies.FirstOrDefaultAsync(c => c.Name == "Delete Me");
            company.Should().BeNull(); // Filtered by global query filter
            
            var allIncluded = await context.Companies.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Name == "Delete Me");
            allIncluded.Should().NotBeNull(); // Exists in DB
            allIncluded!.IsDeleted.Should().BeTrue();
        });
    }
}
