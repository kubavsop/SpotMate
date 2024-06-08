using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Formatters;
using SpotMate.Web.Configurations;

namespace SpotMate.Web;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentationLayer(this IServiceCollection services)
    {
        services.AddControllers(options =>
            {
                options.OutputFormatters.RemoveType<HttpNoContentOutputFormatter>();
            })
            .AddJsonOptions(config => config.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        
        services.Configure<RouteOptions>(options =>
        {
            options.LowercaseUrls = true;
            options.LowercaseQueryStrings = true;
        });
        services.ConfigureOptions<SwaggerGenOptionsConfigure>();
        
        
        return services;
    }
}