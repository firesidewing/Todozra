namespace Todozra.Db.Todo;

public sealed class TodoModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public bool IsPriority { get; set; }
    public DateTime? CompletedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }
}

public sealed record TodoValidationError(string Code, string Message, string? Field = null);

public static class TodoValidator
{
    private static readonly IReadOnlyList<Func<TodoModel, IEnumerable<TodoValidationError>>> Rules =
        new List<Func<TodoModel, IEnumerable<TodoValidationError>>>
        {
            ValidateTitleRequired,
            ValidateTitleMaxLength,
            ValidateDescriptionMaxLength
        };

    public static IReadOnlyList<TodoValidationError> Validate(this TodoModel todo)
    {
        return Rules.SelectMany(x => x(todo)).ToList().AsReadOnly();
    }

    public static IEnumerable<KeyValuePair<string, string[]>> ToValidationErrors(
        this IEnumerable<TodoValidationError> errors)
    {
        return errors.GroupBy(e => e.Field ?? string.Empty)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.Message).ToArray());
    }

    private static IEnumerable<TodoValidationError> ValidateTitleRequired(TodoModel todo)
    {
        if (string.IsNullOrWhiteSpace(todo.Title))
        {
            yield return new TodoValidationError(
                Code: "TitleRequired",
                Message: "Title is required.",
                Field: nameof(TodoModel.Title));
        }
    }

    public const int TitleMaxLength = 200;
    private static IEnumerable<TodoValidationError> ValidateTitleMaxLength(TodoModel todo)
    {
        if (!string.IsNullOrEmpty(todo.Title) && todo.Title.Length > TitleMaxLength)
        {
            yield return new TodoValidationError(
                Code: "TitleTooLong",
                Message: $"Title must be at most {TitleMaxLength} characters.",
                Field: nameof(TodoModel.Title));
        }
    }

    public const int DescriptionMaxLength = 2000;
    private static IEnumerable<TodoValidationError> ValidateDescriptionMaxLength(TodoModel todo)
    {
        if (!string.IsNullOrEmpty(todo.Description) && todo.Description.Length > DescriptionMaxLength)
        {
            yield return new TodoValidationError(
                Code: "DescriptionTooLong",
                Message: $"Description must be at most {DescriptionMaxLength} characters.",
                Field: nameof(TodoModel.Description));
        }
    }
}