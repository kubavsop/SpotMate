using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Formatters;
using SpotMate.Web.Configurations;
using SpotMate.Web.Converters;

namespace SpotMate.Web;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentationLayer(this IServiceCollection services)
    {
        services.AddSignalR();
        services.AddControllers(options =>
            {
                options.OutputFormatters.RemoveType<HttpNoContentOutputFormatter>();
            })
            .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    options.JsonSerializerOptions.Converters.Add(new CustomDateTimeConverter());
                });
        
        services.Configure<RouteOptions>(options =>
        {
            options.LowercaseUrls = true;
            options.LowercaseQueryStrings = true;
        });
        services.ConfigureOptions<SwaggerGenOptionsConfigure>();

        services.AddCors(options =>
            options.AddDefaultPolicy(policy => 
                policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin())
            );
        
        return services;
    }
}