using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

using Todozra.Db.Todo;

namespace Todozra.Api.Features.Todo;

file sealed class EndPoint : IEndPoint
{
    public void MapEndPoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/api/todos/{id:guid}", Handler);
    }

    private static async Task<Results<Ok<TodoDto>, NotFound>> Handler(TodoDbContext context, Guid id)
    {
        var todo = await context.Todos
            .Select(x => new TodoDto(x.Id, x.Title, x.Description, x.CompletedAt, x.CreatedAt))
            .FirstOrDefaultAsync(x => x.Id == id);

        if (todo == null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(todo);
    }
}