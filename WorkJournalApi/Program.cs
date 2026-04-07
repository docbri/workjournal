using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WorkJournalApi.Data;
using WorkJournalApi.Endpoints;
using WorkJournalApi.Middleware;
using WorkJournalApi.Options;
using WorkJournalApi.Repositories;
using WorkJournalApi.Services;
using WorkJournalApi.Validation;
using WorkJournalApi.Infrastructure.Observability;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddObservability(builder.Configuration, builder.Environment);

builder.Services.Configure<DiagnosticsOptions>(
    builder.Configuration.GetSection(DiagnosticsOptions.SectionName));

builder.Services.AddDbContext<WorkJournalDbContext>((serviceProvider, options) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("WorkJournal");

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException(
            "Connection string 'WorkJournal' was not found for this environment.");
    }

    options.UseSqlServer(connectionString, sqlServerOptions =>
    {
        sqlServerOptions.EnableRetryOnFailure();
    });
});

builder.Services.AddHealthChecks()
    .AddDbContextCheck<WorkJournalDbContext>(
        name: "database",
        tags: new[] { "ready" });

builder.Services.AddScoped<IWorkItemRepository, EfCoreWorkItemRepository>();
builder.Services.AddScoped<IWorkItemService, WorkItemService>();
builder.Services.AddSingleton<CreateWorkItemRequestValidator>();
builder.Services.AddSingleton<UpdateWorkItemRequestValidator>();

var app = builder.Build();

var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();
var startupConfiguration = app.Services.GetRequiredService<IConfiguration>();

startupLogger.LogInformation(
    "Application starting in environment '{EnvironmentName}'.",
    app.Environment.EnvironmentName);

startupLogger.LogInformation(
    "Diagnostics config endpoint enabled: {EnableConfigEndpoint}",
    startupConfiguration.GetValue<bool>("Diagnostics:EnableConfigEndpoint"));

var applicationInsightsConnectionString =
    startupConfiguration["APPLICATIONINSIGHTS_CONNECTION_STRING"];

if (string.IsNullOrWhiteSpace(applicationInsightsConnectionString))
{
    if (app.Environment.IsDevelopment())
    {
        startupLogger.LogInformation(
            "Application Insights is not configured for environment '{EnvironmentName}'. Observability export is disabled.",
            app.Environment.EnvironmentName);
    }
    else
    {
        startupLogger.LogWarning(
            "Application Insights is not configured for environment '{EnvironmentName}'. Observability export is disabled.",
            app.Environment.EnvironmentName);
    }
}
else
{
    startupLogger.LogInformation(
        "Application Insights is configured for environment '{EnvironmentName}'.",
        app.Environment.EnvironmentName);
}

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
