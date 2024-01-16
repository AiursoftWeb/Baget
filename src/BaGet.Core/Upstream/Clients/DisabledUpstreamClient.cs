using Aiursoft.BaGet.Core.Entities;
using NuGet.Versioning;

namespace Aiursoft.BaGet.Core.Upstream.Clients
{
    /// <summary>
    /// The client used when there are no upstream package sources.
    /// </summary>
    public class DisabledUpstreamClient : IUpstreamClient
    {
        private readonly IReadOnlyList<NuGetVersion> _emptyVersionList = new List<NuGetVersion>();
        private readonly IReadOnlyList<Package> _emptyPackageList = new List<Package>();

        public Task<IReadOnlyList<NuGetVersion>> ListPackageVersionsAsync(string id, CancellationToken cancellationToken)
        {
            return Task.FromResult(_emptyVersionList);
        }

        public Task<IReadOnlyList<Package>> ListPackagesAsync(string id, CancellationToken cancellationToken)
        {
            return Task.FromResult(_emptyPackageList);
        }

        public Task<Stream> DownloadPackageOrNullAsync(
            string id,
            NuGetVersion version,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<Stream>(null);
        }
    }
}
