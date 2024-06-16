using Microsoft.Extensions.FileProviders;
using SpotMate.Application;
using SpotMate.Infrastructure;
using SpotMate.Persistence;
using SpotMate.Web;
using SpotMate.Web.Hubs.Impl;
using SpotMate.Web.Middlewares;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

builder.Services
    .AddInfrastructureLayer(configuration)
    .AddApplicationLayer(configuration)
    .AddPersistenceLayer(configuration)
    .AddPresentationLayer();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

await app.Services.AddAutoMigrationAsync();
await app.Services.EnsureInterestTypesCreatedAsync();

app.UseExceptionHandlingMiddleware();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var staticFilesPath = Path.Combine(builder.Environment.ContentRootPath, "StaticFiles");

if (!Directory.Exists(staticFilesPath))
{
    Directory.CreateDirectory(staticFilesPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "StaticFiles")),
    RequestPath = "/Static"
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHub<LocationHub>("location-hub");

app.Run();