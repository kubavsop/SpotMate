using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SpotMate.Application.Context;
using SpotMate.Domain.Entities;
using SpotMate.Domain.Enums;
using SpotMate.Persistence.Interceptors;

namespace SpotMate.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistenceLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services
            .AddIdentityCore<SpotMateUser>()
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationDbContext>();
        
        services.AddSingleton<AuditableEntityInterceptor>();
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<ApplicationDbContext>(
            (sp, options) =>
            {
                options.AddInterceptors(sp.GetRequiredService<AuditableEntityInterceptor>());
                options.UseNpgsql(connectionString);
            });
        
        return services;
    }
    
    public static async Task AddAutoMigrationAsync(this IServiceProvider services)
    {
        using (var scope = services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await context.Database.MigrateAsync();
        }
    }

    public static async Task EnsureInterestTypesCreatedAsync(this IServiceProvider services)
    {
        using (var scope = services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            foreach (var type in GetInterestTypes())
            {
                if (!await context.Interests.AnyAsync(i => i.Type == type))
                {
                    await context.Interests.AddAsync(new Interest
                    {
                        Type = type
                    });
                }
            }

            await context.SaveChangesAsync();
        }
    }
    
    private static IEnumerable<InterestType> GetInterestTypes() =>
        Enum.GetValues<InterestType>();
}