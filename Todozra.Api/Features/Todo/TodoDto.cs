using Todozra.Db.Todo;

namespace Todozra.Api.Features.Todo;

public sealed record TodoDto(
    Guid Id,
    string Title,
    string? Description,
    bool IsPriority,
    DateTime? CompletedAt,
    DateTime CreatedAt);

public static class TodoDtoExtensions
{
    public static TodoDto ToDto(this TodoModel model)
    {
        return new TodoDto(model.Id,
            model.Title,
            model.Description,
            model.IsPriority,
            model.CompletedAt,
            model.CreatedAt);
    }
}