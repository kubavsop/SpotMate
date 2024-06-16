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
            .AddIdentityCore<SpotMateUser>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 0;
            })
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

    public static async Task EnsureDefaultUsersCreatedAsync(this IServiceProvider services)
    {
        using (var scope = services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<SpotMateUser>>();
            foreach (var user in GetUsers())
            {
                if (await userManager.FindByIdAsync(user.Id.ToString()) != null)
                    continue;
                
                await userManager.CreateAsync(user, "111111");
            }
        }
    }
    
    private static IEnumerable<InterestType> GetInterestTypes() =>
        Enum.GetValues<InterestType>();

    private static IEnumerable<SpotMateUser> GetUsers() =>
    [
        new SpotMateUser
        {
            Id = Guid.Parse("1111ac8f-3e1b-43ab-bfc2-9d55fe174743"),
            Email = "Maksim@gmail.com",
            UserName = "MaksimUserName",
            FullName = "Maksim",
        },
        new SpotMateUser
        {
            Id = Guid.Parse("2222ac8f-3e1b-43ab-bfc2-9d55fe174743"),
            Email = "Gordey@gmail.com",
            UserName = "GordeyUserName",
            FullName = "Gordey",
        },
        new SpotMateUser
        {
            Id = Guid.Parse("3333ac8f-3e1b-43ab-bfc2-9d55fe174743"),
            Email = "Yura@gmail.com",
            UserName = "YuraUserName",
            FullName = "Yura",
        },
        new SpotMateUser
        {
            Id = Guid.Parse("4444ac8f-3e1b-43ab-bfc2-9d55fe174743"),
            Email = "Nikita@gmail.com",
            UserName = "NikitaUserName",
            FullName = "Nikita",
        },
        new SpotMateUser
        {
            Id = Guid.Parse("5555ac8f-3e1b-43ab-bfc2-9d55fe174743"),
            Email = "Danil@gmail.com",
            UserName = "DanilUserName",
            FullName = "Danil",
        },
        new SpotMateUser
        {
            Id = Guid.Parse("6666ac8f-3e1b-43ab-bfc2-9d55fe174743"),
            Email = "Ruslan@gmail.com",
            UserName = "RuslanUserName",
            FullName = "Ruslan",
        },
        new SpotMateUser
        {
            Id = Guid.Parse("7777ac8f-3e1b-43ab-bfc2-9d55fe174743"),
            Email = "Ivan@gmail.com",
            UserName = "IvanUserName",
            FullName = "Ivan",
        },
        new SpotMateUser
        {
            Id = Guid.Parse("8888ac8f-3e1b-43ab-bfc2-9d55fe174743"),
            Email = "Alex@gmail.com",
            UserName = "AlexUserName",
            FullName = "Alex",
        },
        new SpotMateUser
        {
            Id = Guid.Parse("9999ac8f-3e1b-43ab-bfc2-9d55fe174743"),
            Email = "Artem@gmail.com",
            UserName = "ArtemUserName",
            FullName = "Artem",
        },
        new SpotMateUser
        {
            Id = Guid.Parse("1212ac8f-3e1b-43ab-bfc2-9d55fe174743"),
            Email = "Kubavsop@gmail.com",
            UserName = "KubavsopUserName",
            FullName = "Kubavsop",
        },
    ];
}