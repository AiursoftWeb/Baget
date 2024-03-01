using Aiursoft.BaGet.Database.Sqlite;
using Aiursoft.DbTools;
using static Aiursoft.WebTools.Extends;

namespace Aiursoft.BaGet.Web;

/// <summary>
/// The Program class represents the entry point for the BaGet application.
/// </summary>
public class Program
{
    /// <summary>
    /// The entry point for the BaGet application.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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