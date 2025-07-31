namespace ExampleInMemoryQueue;

public record Order(int CustomerId, DateTime OrderDate)
{
    public int Id { get; init; }

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public DateTime LastSyncAt { get; set; } = DateTime.UtcNow;

    public decimal TotalAmount { get; set; } = 0m;

    public string Status { get; set; } = "Pending";

}
