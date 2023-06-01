using System;
using Microsoft.Extensions.DependencyInjection;

namespace Aiursoft.BaGet.Core
{
    public class BaGetApplication
    {
        public BaGetApplication(IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }

        public IServiceCollection Services { get; }
    }
}
