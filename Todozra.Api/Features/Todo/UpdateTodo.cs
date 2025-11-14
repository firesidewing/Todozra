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
    public void MapEndPoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/api/todos/{id:guid}", Handler);
    }

    private static async Task<Results<Ok<TodoDto>, NotFound>> Handler(TodoDbContext db, Guid id, [FromBody] Request request)
    {
        var todo = await db.Todos.FirstOrDefaultAsync(x => x.Id == id);

        if (todo == null)
        {
            return TypedResults.NotFound();
        }

        db.Todos.Update(request.Apply(todo));
        await db.SaveChangesAsync();

        return TypedResults.Ok(todo.ToDto());
    }
}