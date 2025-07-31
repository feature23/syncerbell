namespace ExampleInMemoryQueue;

public record Customer(string Name, string Email)
{
    public int Id { get; init; }

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public DateTime LastSyncAt { get; set; } = DateTime.UtcNow;

}
