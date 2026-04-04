using WorkJournalApi.Contracts;
using WorkJournalApi.Services;
using WorkJournalApi.Validation;

namespace WorkJournalApi.Endpoints;

public static class WorkItemEndpoints
{
    public static IEndpointRouteBuilder MapWorkItemEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/work-items");

        group.MapGet("/", async (IWorkItemService service, CancellationToken ct) =>
        {
            var items = await service.GetAllAsync(ct);
            var response = items.Select(WorkItemResponse.FromDomain);
            return Results.Ok(response);
        });

        group.MapGet("/{id:guid}", async (Guid id, IWorkItemService service, CancellationToken ct) =>
        {
            var item = await service.GetByIdAsync(id, ct);

            return item is null
                ? Results.NotFound()
                : Results.Ok(WorkItemResponse.FromDomain(item));
        });

        group.MapPost("/", async (
            CreateWorkItemRequest request,
            CreateWorkItemRequestValidator validator,
            IWorkItemService service,
            CancellationToken ct) =>
        {
            var validation = validator.Validate(request);

            if (!validation.IsValid)
            {
                return Results.ValidationProblem(validation.Errors);
            }

            var item = await service.CreateAsync(request.Title, request.Notes, ct);

            return Results.Created($"/work-items/{item.Id}", WorkItemResponse.FromDomain(item));
        });

        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateWorkItemRequest request,
            UpdateWorkItemRequestValidator validator,
            IWorkItemService service,
            CancellationToken ct) =>
        {
            var validation = validator.Validate(request);

            if (!validation.IsValid)
            {
                return Results.ValidationProblem(validation.Errors);
            }

            var item = await service.UpdateAsync(id, request.Title, request.Notes, ct);

            return item is null
                ? Results.NotFound()
                : Results.Ok(WorkItemResponse.FromDomain(item));
        });

        group.MapPost("/{id:guid}/complete", async (Guid id, IWorkItemService service, CancellationToken ct) =>
        {
            var completed = await service.CompleteAsync(id, ct);
            return completed ? Results.NoContent() : Results.NotFound();
        });

        group.MapDelete("/{id:guid}", async (Guid id, IWorkItemService service, CancellationToken ct) =>
        {
            var deleted = await service.DeleteAsync(id, ct);
            return deleted ? Results.NoContent() : Results.NotFound();
        });

        return app;
    }
}
