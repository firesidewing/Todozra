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

file class EndPoint : IEndPoint
{
    public void MapEndPoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/api/todos", Handler);
    }

    private static async Task<Ok<TodoDto>> Handler(TodoDbContext db, [FromBody] Request request)
    {
        var model = request.ToModel();
        model.CreatedAt = DateTime.UtcNow;

        await db.Todos.AddAsync(model);
        await db.SaveChangesAsync();

        return TypedResults.Ok(model.ToDto());
    }
}
