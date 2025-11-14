using Microsoft.AspNetCore.Mvc;

namespace Todozra.Api.Features.Todo;

public class AddTodo
{
    public class EndPoint : IEndPoint
    {
        public void MapEndPoint(IEndpointRouteBuilder builder)
        {
            builder.MapPost("/api/todos/{id}", Handler);
        }

        private async Task Handler(HttpContext context, [FromBody] TodoDto todoDto)
        {
            throw new NotImplementedException();
        }
    }
}