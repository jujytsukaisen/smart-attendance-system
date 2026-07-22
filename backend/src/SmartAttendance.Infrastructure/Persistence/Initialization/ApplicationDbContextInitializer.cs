using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartAttendance.Infrastructure.Identity.Models;

namespace SmartAttendance.Infrastructure.Persistence.Initialization;

public interface IApplicationDbContextInitializer
{
    Task InitializeAsync();
    Task SeedAsync();
}

public class ApplicationDbContextInitializer : IApplicationDbContextInitializer
{
    private readonly ILogger<ApplicationDbContextInitializer> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;

    public ApplicationDbContextInitializer(
        ILogger<ApplicationDbContextInitializer> logger,
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
    }

    public async Task InitializeAsync()
    {
        try
        {
            if (_context.Database.IsSqlServer() && _configuration.GetValue<bool>("EnableAutoMigration"))
            {
                await _context.Database.MigrateAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private async Task TrySeedAsync()
    {
        _logger.LogInformation("Started database seeding process.");

        var executionStrategy = _context.Database.CreateExecutionStrategy();

        await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                await SmartAttendance.Infrastructure.Persistence.Seed.ApplicationDbContextSeed.SeedAsync(_context, _roleManager, _userManager);
                await transaction.CommitAsync();
                
                _logger.LogInformation("Database seeding completed successfully.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Database seeding failed. Transaction rolled back.");
                throw;
            }
        });
    }
}
