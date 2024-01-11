using Aiursoft.BaGet.Database.Sqlite;
using Aiursoft.BaGet.Web;
using Aiursoft.DbTools;
using static Aiursoft.WebTools.Extends;

namespace Aiursoft.Baget.Web;

public class Program
{
    public static async Task Main(string[] args)
    {
        var app = App<Startup>(args, configureBuilder: builder =>
        {
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Limits.MaxRequestBodySize = null;
            });
        });
        await app.UpdateDbAsync<SqliteContext>(UpdateMode.MigrateThenUse);
        await app.RunAsync();
    }
}