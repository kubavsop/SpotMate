using SpotMate.Application.Mapping;
using SpotMate.Application.Options;
using SpotMate.Application.Services;
using SpotMate.Application.Services.Impl;

namespace SpotMate.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IStepService, StepService>();
        services.AddScoped<IFriendService, FriendService>();
        services.AddScoped<IRequestService, RequestService>();
        services.AddScoped<IInterestService, InterestService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddAutoMapper(typeof(ConventionalMappingProfile).Assembly);
        services.Configure<RefreshTokenOptions>(configuration.GetSection("RefreshToken"));
        services.Configure<BaseUrlOptions>(configuration.GetSection("BaseUrl"));
        services.AddStackExchangeRedisCache(options =>
        {
            var connection = configuration.GetConnectionString("Redis");
            options.Configuration = connection;
        });
        return services;
    }
}