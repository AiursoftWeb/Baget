using System.Text.Json.Serialization;
using Aiursoft.BaGet.Core;
using Aiursoft.BaGet.Web;
using Microsoft.Extensions.DependencyInjection;

namespace Aiursoft.BaGet
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddBaGetWebApplication(
            this IServiceCollection services,
            Action<BaGetApplication, IConfiguration> configureAction)
        {
            

            return services;
        }
    }
}
