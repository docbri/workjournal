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
    var environment = serviceProvider.GetRequiredService<IHostEnvironment>();
    var connectionString = builder.Configuration.GetConnectionString("WorkJournal");

    options.UseSqlite(connectionString);

    if (environment.IsDevelopment())
    {
        options
            .LogTo(Console.WriteLine, LogLevel.Information)
            .EnableSensitiveDataLogging();
    }
});

builder.Services.AddScoped<IWorkItemRepository, EfCoreWorkItemRepository>();
builder.Services.AddScoped<IWorkItemService, WorkItemService>();
builder.Services.AddSingleton<CreateWorkItemRequestValidator>();
builder.Services.AddSingleton<UpdateWorkItemRequestValidator>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<WorkJournalDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestTimingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Work Journal API v1");
    });
}

app.MapDiagnosticEndpoints();
app.MapWorkItemEndpoints();

app.Run();

public partial class Program { }
