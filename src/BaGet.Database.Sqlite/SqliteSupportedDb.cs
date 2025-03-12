using Aiursoft.BaGet.Core.Entities;
using Aiursoft.DbTools;
using Aiursoft.DbTools.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace Aiursoft.BaGet.Database.Sqlite;

public class SqliteSupportedDb(bool allowCache, bool splitQuery) : SupportedDatabaseType<AbstractContext>
{
    public override string DbType => "Sqlite";

    public override IServiceCollection RegisterFunction(IServiceCollection services, string connectionString)
    {
        return services.AddAiurSqliteWithCache<SqliteContext>(
            connectionString,
            splitQuery: splitQuery,
            allowCache: allowCache);
    }

    public override AbstractContext ContextResolver(IServiceProvider serviceProvider)
    {
        return serviceProvider.GetRequiredService<SqliteContext>();
    }
}
