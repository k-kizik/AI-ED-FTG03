using LegalDocumentComparator.Application.Common.Interfaces;
using LegalDocumentComparator.Application.Common.Interfaces.Repositories;
using LegalDocumentComparator.Application.Common.Interfaces.Services;
using LegalDocumentComparator.Infrastructure.Persistence;
using LegalDocumentComparator.Infrastructure.Persistence.Repositories;
using LegalDocumentComparator.Infrastructure.Services;
using LegalDocumentComparator.Infrastructure.Services.AiProviders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LegalDocumentComparator.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var databasePath = configuration["Database:Path"] ?? 
            Path.Combine(Directory.GetCurrentDirectory(), "data", "legal-comparator.db");
        
        var databaseDirectory = Path.GetDirectoryName(databasePath);
        if (!string.IsNullOrEmpty(databaseDirectory) && !Directory.Exists(databaseDirectory))
        {
            Directory.CreateDirectory(databaseDirectory);
        }

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite($"Data Source={databasePath}"));

        services.AddScoped<IApplicationDbContext>(provider => 
            provider.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<ApplicationDbContextInitializer>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IComparisonRepository, ComparisonRepository>();

        services.AddScoped<IPdfService, PdfService>();
        services.AddScoped<IAiService, AiService>();
        services.AddScoped<IStorageService, StorageService>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddHttpClient();

        services.AddScoped<IAiProvider, GroqProvider>();

        return services;
    }
}
