using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Todozra.Db.Todo;

namespace Todozra.Api.Features.Todo;

file sealed record Request(string Title, string? Description, DateTime? CompletedAt)
{
    public TodoModel Apply(TodoModel model)
    {
        model.Title = Title;
        model.Description = Description;
        model.CompletedAt = CompletedAt;
        model.UpdatedAt = DateTime.UtcNow;

        return model;
    }
}

file sealed class EndPoint : IEndPoint
{
    public void MapEndPoint(IEndpointRouteBuilder builder) { builder.MapPost("/api/todos/{id:guid}", Handler); }

    private static async Task<Results<Ok<TodoDto>, NotFound, ValidationProblem>> Handler(TodoDbContext db,
        ILogger<EndPoint> logger,
        Guid id,
        [FromBody] Request request)
    {
        logger.LogInformation("Handling request");

        var todo = await db.Todos.FirstOrDefaultAsync(x => x.Id == id);
        if (todo == null)
        {
            logger.LogWarning("Todo not found {Id}", id);
            return TypedResults.NotFound();
        }

        var errors = request
            .Apply(todo)
            .Validate();
        if (errors.Count > 0)
        {
            logger.LogWarning("Validation errors: {@Errors}", errors.Select(x => x.Message));
            return TypedResults.ValidationProblem(errors.ToValidationErrors());
        }

        todo.UpdatedAt = DateTime.UtcNow;
        db.Todos.Update(todo);
        await db.SaveChangesAsync();

        return TypedResults.Ok(todo.ToDto());
    }
}