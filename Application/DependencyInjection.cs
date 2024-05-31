using SpotMate.Application.Options;
using SpotMate.Application.Services;
using SpotMate.Application.Services.Impl;

namespace SpotMate.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.Configure<RefreshTokenOptions>(configuration.GetSection("RefreshToken"));
        return services;
    }
}