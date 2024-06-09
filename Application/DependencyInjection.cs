using SpotMate.Application.Mapping;
using SpotMate.Application.Options;
using SpotMate.Application.Services;
using SpotMate.Application.Services.Impl;

namespace SpotMate.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddAutoMapper(typeof(ConventionalMappingProfile).Assembly);
        services.Configure<RefreshTokenOptions>(configuration.GetSection("RefreshToken"));
        services.Configure<StaticBaseUrlOptions>(configuration.GetSection("StaticFilesBaseUrl"));
        return services;
    }
}