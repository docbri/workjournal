using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WorkJournalApi.Data;
using WorkJournalApi.Endpoints;
using WorkJournalApi.Middleware;
using WorkJournalApi.Options;
using WorkJournalApi.Repositories;
using WorkJournalApi.Services;
using WorkJournalApi.Validation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.Configure<DiagnosticsOptions>(
    builder.Configuration.GetSection(DiagnosticsOptions.SectionName));

builder.Services.AddDbContext<WorkJournalDbContext>((serviceProvider, options) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();

    var provider = configuration["Database:Provider"];
    var connectionString = configuration.GetConnectionString("WorkJournal");

    if (string.IsNullOrWhiteSpace(provider))
    {
        throw new InvalidOperationException(
            "Database provider is not configured for this environment.");
    }

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException(
            "Connection string 'WorkJournal' was not found for this environment.");
    }

    switch (provider)
    {
        case "Sqlite":
            options.UseSqlite(connectionString);
            break;

        case "SqlServer":
            options.UseSqlServer(connectionString, sqlServerOptions =>
            {
                sqlServerOptions.EnableRetryOnFailure();
            });
            break;

        default:
            throw new InvalidOperationException(
                $"Unsupported database provider: '{provider}'.");
    }
});

builder.Services.AddScoped<IWorkItemRepository, EfCoreWorkItemRepository>();
builder.Services.AddScoped<IWorkItemService, WorkItemService>();
builder.Services.AddSingleton<CreateWorkItemRequestValidator>();
builder.Services.AddSingleton<UpdateWorkItemRequestValidator>();

var app = builder.Build();

var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();
var startupConfiguration = app.Services.GetRequiredService<IConfiguration>();

startupLogger.LogInformation(
    "Application starting in environment '{EnvironmentName}' with database provider '{DatabaseProvider}'.",
    app.Environment.EnvironmentName,
    startupConfiguration["Database:Provider"]);

startupLogger.LogInformation(
    "Diagnostics config endpoint enabled: {EnableConfigEndpoint}",
    startupConfiguration.GetValue<bool>("Diagnostics:EnableConfigEndpoint"));

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestTimingMiddleware>();

if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Dev")
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Work Journal API v1");
    });
}

app.MapDiagnosticEndpoints();
app.MapWorkItemEndpoints();
app.MapSystemEndpoints();

app.Run();

public partial class Program { }
