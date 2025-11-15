using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

using Todozra.Db.Todo;

namespace Todozra.Api.Features.Todo;

file sealed class EndPoint : IEndPoint
{
    public  void MapEndPoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/api/todos", Handler);
    }

    private static async Task<Ok<List<TodoDto>>> Handler(TodoDbContext db)
    {
        var ret = await db.Todos
            .Where(x => x.DeletedAt == null)
            .Select(x => x.ToDto())
            .ToListAsync();

        return TypedResults.Ok(ret);
    }
}