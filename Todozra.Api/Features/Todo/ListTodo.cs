namespace Todozra.Api.Features.Todo;

public class ListTodo
{
    public class EndPoint : IEndPoint
    {
        public void MapEndPoint(IEndpointRouteBuilder builder)
        {
            builder.MapGet("/api/todos", Handler);
        }

        private async Task Handler(HttpContext context)
        {
            throw new NotImplementedException();
        }
    }
}