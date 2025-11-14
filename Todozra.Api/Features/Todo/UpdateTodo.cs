using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

using Todozra.Db.Todo;

namespace Todozra.Api.Features.Todo;

public sealed class UpdateTodo
{
    public sealed record Request(Guid Id, string Title, string? Description);

    public class EndPoint : IEndPoint
    {
        public void MapEndPoint(IEndpointRouteBuilder builder)
        {
            builder.MapPost("/api/todos/{id:guid}", Handler);
        }

        private static async Task<Results<Ok<TodoDto>, NotFound>> Handler(TodoDbContext context, Guid id)
        {
            var todo = await context.Todos.FirstOrDefaultAsync(x => x.Id == id);

            if (todo == null)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(todo);
        }
    }
}