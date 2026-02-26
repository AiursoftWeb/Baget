using Aiursoft.BaGet.Core.ServiceIndex;
using Aiursoft.BaGet.Protocol.Models;
using Microsoft.AspNetCore.Mvc;

namespace Aiursoft.BaGet.Web.Controllers
{
    /// <summary>
    /// The NuGet Service Index. This aids NuGet client to discover this server's services.
    /// </summary>
    public class ServiceIndexController(IServiceIndexService serviceIndex) : Controller
    {
        private readonly IServiceIndexService _serviceIndex = serviceIndex ?? throw new ArgumentNullException(nameof(serviceIndex));

        // GET v3/index
        [HttpGet]
        public async Task<ServiceIndexResponse> GetAsync(CancellationToken cancellationToken)
        {
            return await _serviceIndex.GetAsync(cancellationToken);
        }
    }
}
