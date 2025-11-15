using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

using Todozra.Db.Todo;

namespace Todozra.Api.Features.Todo;

file sealed class EndPoint : IEndPoint
{
    public void MapEndPoint(IEndpointRouteBuilder builder)
    {
        builder.MapDelete("/api/todos/{id:guid}", Handler);
    }

    private static async Task<Results<NoContent, NotFound>> Handler(TodoDbContext context, Guid id)
    {
        var todo = await context.Todos.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null);

        if (todo == null)
        {
            return TypedResults.NotFound();
        }

        todo.DeletedAt = DateTime.UtcNow;
        todo.UpdatedAt = DateTime.UtcNow;

        context.Update(todo);
        await context.SaveChangesAsync();

        return TypedResults.NoContent();
    }
}
