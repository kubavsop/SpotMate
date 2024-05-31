using SpotMate.Application;
using SpotMate.Infrastructure;
using SpotMate.Persistence;
using SpotMate.Web;
using SpotMate.Web.Middlewares;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

builder.Services
    .AddInfrastructureLayer()
    .AddApplicationLayer(configuration)
    .AddPersistenceLayer(configuration)
    .AddPresentationLayer();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

await app.Services.AddAutoMigrationAsync();

app.UseExceptionHandlingMiddleware();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();