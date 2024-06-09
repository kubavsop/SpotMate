using Microsoft.AspNetCore.Authentication.JwtBearer;
using SpotMate.Application.Services;
using SpotMate.Infrastructure.Options;
using SpotMate.Infrastructure.Options.OptionsSetup;
using SpotMate.Infrastructure.Services;
using StaticFileOptions = SpotMate.Infrastructure.Options.StaticFileOptions;

namespace SpotMate.Infrastructure;

public static class DependencyInjection
{
    
    public static IServiceCollection AddInfrastructureLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IFileProvider, FileProvider>();
        services.AddScoped<IJwtProvider, JwtProvider>();
        services.ConfigureOptions<JwtOptionsSetup>();
        services.ConfigureOptions<JwtBearerOptionsSetup>();
        services.ConfigureOptions<CustomJwtBearerOptionsSetup>();
        services.ConfigureOptions<CustomAuthorizationOptionsSetup>();
        services.Configure<StaticFileOptions>(configuration.GetSection("StaticFilePath"));

        
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer()
            .AddJwtBearer(CustomJwtBearerDefaults.CheckOnlySignature); 
        
        return services;
    }
}