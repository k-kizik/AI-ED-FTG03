using LegalDocumentComparator.Domain.Entities;
using LegalDocumentComparator.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LegalDocumentComparator.Infrastructure.Persistence;

public class ApplicationDbContextInitializer
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ApplicationDbContextInitializer> _logger;

    public ApplicationDbContextInitializer(
        ApplicationDbContext context,
        ILogger<ApplicationDbContextInitializer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        try
        {
            await _context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initializing the database");
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
            _logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    private async Task TrySeedAsync()
    {
        if (!_context.Users.Any())
        {
            var adminPasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!");
            var admin = User.Create("admin@legal.com", adminPasswordHash, UserRole.Manager);

            var userPasswordHash = BCrypt.Net.BCrypt.HashPassword("User123!");
            var user = User.Create("user@legal.com", userPasswordHash, UserRole.User);

            await _context.Users.AddRangeAsync(admin, user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Seeded default users:");
            _logger.LogInformation("  Manager: admin@legal.com / Admin123!");
            _logger.LogInformation("  User: user@legal.com / User123!");
        }
    }
}
