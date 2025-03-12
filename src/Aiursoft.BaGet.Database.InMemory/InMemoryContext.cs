using Aiursoft.BaGet.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.BaGet.Database.InMemory;

public class InMemoryContext(DbContextOptions<InMemoryContext> options) : AbstractContext(options)
{
    public override async Task MigrateAsync(CancellationToken cancellationToken)
    {
        await Database.EnsureDeletedAsync(cancellationToken);
        await Database.EnsureCreatedAsync(cancellationToken);
    }

    public override Task<bool> CanConnectAsync()
    {
        return Task.FromResult(true);
    }
}
