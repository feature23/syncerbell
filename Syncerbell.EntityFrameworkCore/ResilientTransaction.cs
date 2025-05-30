// From: https://learn.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/implement-resilient-entity-framework-core-sql-connections#execution-strategies-and-explicit-transactions-using-begintransaction-and-multiple-dbcontexts

using Microsoft.EntityFrameworkCore;

namespace Syncerbell.EntityFrameworkCore;

internal class ResilientTransaction
{
    private readonly DbContext _context;
    private ResilientTransaction(DbContext context) =>
        _context = context ?? throw new ArgumentNullException(nameof(context));

    public static ResilientTransaction New(DbContext context) =>
        new ResilientTransaction(context);

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default)
    {
        // Use of an EF Core resiliency strategy when using multiple DbContexts
        // within an explicit BeginTransaction():
        // https://learn.microsoft.com/ef/core/miscellaneous/connection-resiliency
        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            var result = await action();
            await transaction.CommitAsync(cancellationToken);
            return result;
        });
    }
}
