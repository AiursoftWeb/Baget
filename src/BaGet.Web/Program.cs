using Aiursoft.BaGet.Core.Entities;
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
        var app = await AppAsync<Startup>(args);
        await app.UpdateDbAsync<AbstractContext>();
        await app.RunAsync();
    }
}
