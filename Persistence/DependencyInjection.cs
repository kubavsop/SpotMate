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
        new SpotMateUser
        {
            Id = Guid.Parse("1222ac8f-3e1b-43ab-bfc2-9d55fe174743"),
            Email = "john.doe@example.com",
            UserName = "john.doe",
            FullName = "John Doe",
        },
        new SpotMateUser
        {
            Id = Guid.Parse("1111ac8f-3e1b-43ab-bfc2-9d55fe174743"),
            Email = "jane.smith@example.com",
            UserName = "jane.smith",
            FullName = "Jane Smith",
        },
        new SpotMateUser
        {
            Id = Guid.Parse("1213ac8f-3e1b-43ab-bfc2-9d55fe174743"),
            Email = "alex.jones@example.com",
            UserName = "alex.jones",
            FullName = "Alex Jones",
        },
        new SpotMateUser
        {
            Id = Guid.Parse("1313ac8f-3e1b-43ab-bfc2-9d55fe174743"),
            Email = "emily.brown@example.com",
            UserName = "emily.brown",
            FullName = "Emily Brown",
        },
        new SpotMateUser
        {
            Id = Guid.Parse("1414ac8f-3e1b-43ab-bfc2-9d55fe174743"),
            Email = "michael.davis@example.com",
            UserName = "michael.davis",
            FullName = "Michael Davis",
        },
        new SpotMateUser
        {
            Id = Guid.Parse("1515ac8f-3e1b-43ab-bfc2-9d55fe174743"),
            Email = "sarah.white@example.com",
            UserName = "sarah.white",
            FullName = "Sarah White",
        },
        new SpotMateUser
        {
            Id = Guid.Parse("1616ac8f-3e1b-43ab-bfc2-9d55fe174743"),
            Email = "robert.johnson@example.com",
            UserName = "robert.johnson",
            FullName = "Robert Johnson",
        },
        new SpotMateUser
        {
            Id = Guid.Parse("1717ac8f-3e1b-43ab-bfc2-9d55fe174743"),
            Email = "amanda.miller@example.com",
            UserName = "amanda.miller",
            FullName = "Amanda Miller",
        },
        new SpotMateUser
        {
            Id = Guid.Parse("1818ac8f-3e1b-43ab-bfc2-9d55fe174743"),
            Email = "william.wilson@example.com",
            UserName = "william.wilson",
            FullName = "William Wilson",
        },
        new SpotMateUser
        {
            Id = Guid.Parse("1919ac8f-3e1b-43ab-bfc2-9d55fe174743"),
            Email = "laura.thomas@example.com",
            UserName = "laura.thomas",
            FullName = "Laura Thomas",
        },
        new SpotMateUser
        {
            Id = Guid.Parse("2020ac8f-3e1b-43ab-bfc2-9d55fe174743"),
            Email = "daniel.clark@example.com",
            UserName = "daniel.clark",
            FullName = "Daniel Clark",
        },
        new SpotMateUser
        {
            Id = Guid.Parse("2121ac8f-3e1b-43ab-bfc2-9d55fe134743"),
            Email = "natalie.baker@example.com",
            UserName = "natalie.baker",
            FullName = "Natalie Baker",
        },
        new SpotMateUser
        {
            Id = Guid.Parse("2222ac8f-3e1b-43ab-bfc2-9d55fe154743"),
            Email = "kevin.thompson@example.com",
            UserName = "kevin.thompson",
            FullName = "Kevin Thompson",
        },
        new SpotMateUser
        {
            Id = Guid.Parse("2323ac8f-3e1b-43ab-bfc2-9d55fe154743"),
            Email = "olivia.hall@example.com",
            UserName = "olivia.hall",
            FullName = "Olivia Hall",
        },
        new SpotMateUser
        {
            Id = Guid.Parse("2424ac8f-3e1b-43ab-bfc2-9d55fe171743"),
            Email = "chris.garcia@example.com",
            UserName = "chris.garcia",
            FullName = "Chris Garcia",
        },
        new SpotMateUser
        {
            Id = Guid.Parse("2525ac8f-3e1b-43ab-bfc2-9d55fe574743"),
            Email = "hannah.wright@example.com",
            UserName = "hannah.wright",
            FullName = "Hannah Wright",
        },
        new SpotMateUser
        {
            Id = Guid.Parse("2626ac8f-3e1b-43ab-bfc2-9d55fe174743"),
            Email = "joshua.scott@example.com",
            UserName = "joshua.scott",
            FullName = "Joshua Scott",
        },
        new SpotMateUser
        {
            Id = Guid.Parse("2727ac8f-3e1b-43ab-bfc2-9d55fe174743"),
            Email = "samantha.perez@example.com",
            UserName = "samantha.perez",
            FullName = "Samantha Perez",
        },
        new SpotMateUser
        {
            Id = Guid.Parse("2828ac8f-3e1b-43ab-bfc2-9d55fe174743"),
            Email = "matthew.rodriguez@example.com",
            UserName = "matthew.rodriguez",
            FullName = "Matthew Rodriguez",
        },
        new SpotMateUser
        {
            Id = Guid.Parse("2929ac8f-3e1b-43ab-bfc2-9d55fe174743"),
            Email = "elizabeth.young@example.com",
            UserName = "elizabeth.young",
            FullName = "Elizabeth Young",
        },
    ];
}