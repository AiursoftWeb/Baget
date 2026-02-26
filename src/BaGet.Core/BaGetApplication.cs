using Microsoft.Extensions.DependencyInjection;

namespace Aiursoft.BaGet.Core
{
    public class BaGetApplication(IServiceCollection services)
    {
        public IServiceCollection Services { get; } = services ?? throw new ArgumentNullException(nameof(services));
    }
}
