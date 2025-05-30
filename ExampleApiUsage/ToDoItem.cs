namespace ExampleApiUsage;

public record ToDoItem(string Description, bool IsComplete)
{
    public int Id { get; init; }

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    public bool IsComplete { get; private set; } = IsComplete;

    public void MarkAsComplete()
    {
        IsComplete = true;
        ModifiedAt = DateTime.UtcNow;
    }
}
