using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

using Todozra.Db.Todo;

namespace Todozra.Api.Features.Todo;

file sealed record Request(string Title, string? Description)
{
    public TodoModel ToModel()
    {
        return new TodoModel
        {
            Title = Title,
            Description = Description,
            CompletedAt = null
        };
    }
}

file sealed class EndPoint : IEndPoint
{
    public void MapEndPoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/api/todos", Handler);
    }

    private static async Task<Results<Ok<TodoDto>, ValidationProblem>> Handler(TodoDbContext db, [FromBody] Request request)
    {
        var todo = request.ToModel();
        todo.CreatedAt = DateTime.UtcNow;

        var errors = todo.Validate();
        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors.ToValidationErrors());
        }

        await db.Todos.AddAsync(todo);
        await db.SaveChangesAsync();

        return TypedResults.Ok(todo.ToDto());
    }
}
