namespace ExampleInMemoryQueue;

public record Product(string Name, decimal Price, string Category)
{
    public int Id { get; init; }

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public DateTime LastSyncAt { get; set; } = DateTime.UtcNow;

    public int StockQuantity { get; set; } = 0;

}
