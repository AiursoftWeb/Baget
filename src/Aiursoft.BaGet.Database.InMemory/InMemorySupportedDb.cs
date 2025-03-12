using Aiursoft.BaGet.Core.Entities;
using Aiursoft.DbTools;
using Aiursoft.DbTools.InMemory;
using Microsoft.Extensions.DependencyInjection;

namespace Aiursoft.BaGet.Database.InMemory;

public class InMemorySupportedDb : SupportedDatabaseType<AbstractContext>
{
    public override string DbType => "InMemory";

    public override IServiceCollection RegisterFunction(IServiceCollection services, string connectionString)
    {
        return services.AddAiurInMemoryDb<InMemoryContext>();
    }

    public override AbstractContext ContextResolver(IServiceProvider serviceProvider)
    {
        return serviceProvider.GetRequiredService<InMemoryContext>();
    }
}