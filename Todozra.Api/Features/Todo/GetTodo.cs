using Microsoft.AspNetCore.Http.HttpResults;

using Todozra.Db.Todo;

namespace Todozra.Api.Features.Todo;

public static class GetTodo
{
    public class EndPoint : IEndPoint
    {
        public void MapEndPoint(IEndpointRouteBuilder builder)
        {
            builder.MapGet("/api/todos/{id:guid}", Handler);
        }

        private async Task<TypedResults<Ok<TodoModel>>> Handler(TodoDbContext context, Guid id)
        {
            var todo = await context.Todos.FindAsync(id);

            if (todo == null)
            {
                return Results.NotFound();
            }

            return Results.Ok(todo);
        }
    }
}