using Aiursoft.BaGet.Web;
using static Aiursoft.WebTools.Extends;

namespace Aiursoft.Baget.Web;

public class Program
{
    public static async Task Main(string[] args)
    {
        var app = App<Startup>(args);
        await app.RunMigrationsAsync();
        await app.RunAsync();
    }
}