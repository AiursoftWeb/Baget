using Aiursoft.BaGet.Protocol.Catalog;
using Aiursoft.BaGet.Protocol.Extensions;
using Aiursoft.BaGet.Protocol.Models;
using Aiursoft.BaGet.Protocol.PackageContent;
using Aiursoft.BaGet.Protocol.PackageMetadata;
using Aiursoft.BaGet.Protocol.Search;
using Aiursoft.BaGet.Protocol.ServiceIndex;
using NuGet.Versioning;

namespace Aiursoft.BaGet.Protocol.ClientFactories
{
    public class NuGetClientFactory
    {
        private class CatalogClient : ICatalogClient
        {
            private readonly NuGetClientFactory _clientFactory;

            public CatalogClient(NuGetClientFactory clientFactory)
            {
                _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
            }

            public async Task<CatalogIndex> GetIndexAsync(CancellationToken cancellationToken = default)
            {
                var client = await _clientFactory.GetCatalogClientAsync(cancellationToken);

                return await client.GetIndexAsync(cancellationToken);
            }

            public async Task<CatalogPage> GetPageAsync(string pageUrl, CancellationToken cancellationToken = default)
            {
                var client = await _clientFactory.GetCatalogClientAsync(cancellationToken);

                return await client.GetPageAsync(pageUrl, cancellationToken);
            }

            public async Task<PackageDetailsCatalogLeaf> GetPackageDetailsLeafAsync(string leafUrl, CancellationToken cancellationToken = default)
            {
                var client = await _clientFactory.GetCatalogClientAsync(cancellationToken);

                return await client.GetPackageDetailsLeafAsync(leafUrl, cancellationToken);
            }

            public async Task<PackageDeleteCatalogLeaf> GetPackageDeleteLeafAsync(string leafUrl, CancellationToken cancellationToken = default)
            {
                var client = await _clientFactory.GetCatalogClientAsync(cancellationToken);

                return await client.GetPackageDeleteLeafAsync(leafUrl, cancellationToken);
            }
        }

        private class SearchClient : ISearchClient
        {
            private readonly NuGetClientFactory _clientfactory;

            public SearchClient(NuGetClientFactory clientFactory)
            {
                _clientfactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
            }

            public async Task<SearchResponse> SearchAsync(
                string query = null,
                int skip = 0,
                int take = 20,
                bool includePrerelease = true,
                bool includeSemVer2 = true,
                CancellationToken cancellationToken = default)
            {
                // TODO: Support search failover.
                // See: https://github.com/loic-sharma/BaGet/issues/314
                var client = await _clientfactory.GetSearchClientAsync(cancellationToken);

                return await client.SearchAsync(query, skip, take, includePrerelease, includeSemVer2);
            }
        }

        private class AutocompleteClient : IAutocompleteClient
        {
            private readonly NuGetClientFactory _clientfactory;

            public AutocompleteClient(NuGetClientFactory clientFactory)
            {
                _clientfactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
            }

            public async Task<AutocompleteResponse> AutocompleteAsync(
                string query = null,
                int skip = 0,
                int take = 20,
                bool includePrerelease = true,
                bool includeSemVer2 = true,
                CancellationToken cancellationToken = default)
            {
                // TODO: Support search failover.
                // See: https://github.com/loic-sharma/BaGet/issues/314
                var client = await _clientfactory.GetAutocompleteClientAsync(cancellationToken);

                return await client.AutocompleteAsync(query, skip, take, includePrerelease, includeSemVer2, cancellationToken);
            }

            public async Task<AutocompleteResponse> ListPackageVersionsAsync(
                string packageId,
                bool includePrerelease = true,
                bool includeSemVer2 = true,
                CancellationToken cancellationToken = default)
            {
                // TODO: Support search failover.
                // See: https://github.com/loic-sharma/BaGet/issues/314
                var client = await _clientfactory.GetAutocompleteClientAsync(cancellationToken);

                return await client.ListPackageVersionsAsync(packageId, includePrerelease, includeSemVer2, cancellationToken);
            }
        }

        private class PackageContentClient : IPackageContentClient
        {
            private readonly NuGetClientFactory _clientfactory;

            public PackageContentClient(NuGetClientFactory clientFactory)
            {
                _clientfactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
            }

            public async Task<Stream> DownloadPackageOrNullAsync(
                string packageId,
                NuGetVersion packageVersion,
                CancellationToken cancellationToken = default)
            {
                var client = await _clientfactory.GetPackageContentClientAsync(cancellationToken);

                return await client.DownloadPackageOrNullAsync(packageId, packageVersion, cancellationToken);
            }

            public async Task<Stream> DownloadPackageManifestOrNullAsync(
                string packageId,
                NuGetVersion packageVersion,
                CancellationToken cancellationToken = default)
            {
                var client = await _clientfactory.GetPackageContentClientAsync(cancellationToken);

                return await client.DownloadPackageManifestOrNullAsync(packageId, packageVersion, cancellationToken);
            }

            public async Task<PackageVersionsResponse> GetPackageVersionsOrNullAsync(
                string packageId,
                CancellationToken cancellationToken = default)
            {
                var client = await _clientfactory.GetPackageContentClientAsync(cancellationToken);

                return await client.GetPackageVersionsOrNullAsync(packageId, cancellationToken);
            }
        }

        private class ServiceIndexClient : IServiceIndexClient
        {
            private readonly NuGetClientFactory _clientFactory;

            public ServiceIndexClient(NuGetClientFactory clientFactory)
            {
                _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
            }

            public async Task<ServiceIndexResponse> GetAsync(CancellationToken cancellationToken = default)
            {
                return await _clientFactory.GetServiceIndexAsync(cancellationToken);
            }
        }

        private class PackageMetadataClient : IPackageMetadataClient
        {
            private readonly NuGetClientFactory _clientfactory;

            public PackageMetadataClient(NuGetClientFactory clientFactory)
            {
                _clientfactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
            }

            public async Task<RegistrationIndexResponse> GetRegistrationIndexOrNullAsync(
                string packageId,
                CancellationToken cancellationToken = default)
            {
                var client = await _clientfactory.GetPackageMetadataClientAsync(cancellationToken);

                return await client.GetRegistrationIndexOrNullAsync(packageId, cancellationToken);
            }

            public async Task<RegistrationPageResponse> GetRegistrationPageAsync(
                string pageUrl,
                CancellationToken cancellationToken = default)
            {
                var client = await _clientfactory.GetPackageMetadataClientAsync(cancellationToken);

                return await client.GetRegistrationPageAsync(pageUrl, cancellationToken);
            }

            public async Task<RegistrationLeafResponse> GetRegistrationLeafAsync(
                string leafUrl,
                CancellationToken cancellationToken = default)
            {
                var client = await _clientfactory.GetPackageMetadataClientAsync(cancellationToken);

                return await client.GetRegistrationLeafAsync(leafUrl, cancellationToken);
            }
        }

        private readonly HttpClient _httpClient;
        private readonly string _serviceIndexUrl;

        private readonly SemaphoreSlim _mutex;
        private NuGetClients _clients;

        /// <summary>
        /// Initializes a new instance of the <see cref="NuGetClientFactory"/> class
        /// for mocking.
        /// </summary>
        protected NuGetClientFactory()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NuGetClientFactory"/> class.
        /// </summary>
        /// <param name="httpClient">The client used for HTTP requests.</param>
        /// <param name="serviceIndexUrl">
        /// The NuGet Service Index resource URL.
        ///
        /// For NuGet.org, use https://api.nuget.org/v3/index.json
        /// </param>
        public NuGetClientFactory(HttpClient httpClient, string serviceIndexUrl)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _serviceIndexUrl = serviceIndexUrl ?? throw new ArgumentNullException(nameof(serviceIndexUrl));

            _mutex = new SemaphoreSlim(1, 1);
            _clients = null;
        }

        /// <summary>
        /// Create a client to interact with the NuGet Service Index resource.
        /// 
        /// See https://docs.microsoft.com/en-us/nuget/api/service-index
        /// </summary>
        /// <returns>A client to interact with the NuGet Service Index resource.</returns>
        public virtual IServiceIndexClient CreateServiceIndexClient()
        {
            return new ServiceIndexClient(this);
        }

        /// <summary>
        /// Create a client to interact with the NuGet Package Content resource.
        ///
        /// See https://docs.microsoft.com/en-us/nuget/api/package-base-address-resource
        /// </summary>
        /// <returns>A client to interact with the NuGet Package Content resource.</returns>
        public virtual IPackageContentClient CreatePackageContentClient()
        {
            return new PackageContentClient(this);
        }

        /// <summary>
        /// Create a client to interact with the NuGet Package Metadata resource.
        /// 
        /// See https://docs.microsoft.com/en-us/nuget/api/registration-base-url-resource
        /// </summary>
        /// <returns>A client to interact with the NuGet Package Metadata resource.</returns>
        public virtual IPackageMetadataClient CreatePackageMetadataClient()
        {
            return new PackageMetadataClient(this);
        }

        /// <summary>
        /// Create a client to interact with the NuGet Search resource.
        /// 
        /// See https://docs.microsoft.com/en-us/nuget/api/search-query-service-resource
        /// </summary>
        /// <returns>A client to interact with the NuGet Search resource.</returns>
        public virtual ISearchClient CreateSearchClient()
        {
            return new SearchClient(this);
        }

        /// <summary>
        /// Create a client to interact with the NuGet Autocomplete resource.
        /// 
        /// See https://docs.microsoft.com/en-us/nuget/api/search-autocomplete-service-resource
        /// </summary>
        /// <returns>A client to interact with the NuGet Autocomplete resource.</returns>
        public virtual IAutocompleteClient CreateAutocompleteClient()
        {
            return new AutocompleteClient(this);
        }

        /// <summary>
        /// Create a client to interact with the NuGet catalog resource.
        /// 
        /// See https://docs.microsoft.com/en-us/nuget/api/catalog-resource
        /// </summary>
        /// <returns>A client to interact with the Catalog resource.</returns>
        public virtual ICatalogClient CreateCatalogClient()
        {
            return new CatalogClient(this);
        }


        private Task<ServiceIndexResponse> GetServiceIndexAsync(CancellationToken cancellationToken = default)
        {
            return GetAsync(c => c.ServiceIndex, cancellationToken);
        }

        private Task<IPackageContentClient> GetPackageContentClientAsync(CancellationToken cancellationToken = default)
        {
            return GetAsync(c => c.PackageContentClient, cancellationToken);
        }

        private Task<IPackageMetadataClient> GetPackageMetadataClientAsync(CancellationToken cancellationToken = default)
        {
            return GetAsync(c => c.PackageMetadataClient, cancellationToken);
        }

        private Task<ISearchClient> GetSearchClientAsync(CancellationToken cancellationToken = default)
        {
            return GetAsync(c => c.SearchClient, cancellationToken);
        }

        private Task<IAutocompleteClient> GetAutocompleteClientAsync(CancellationToken cancellationToken = default)
        {
            return GetAsync(c => c.AutocompleteClient, cancellationToken);
        }

        private Task<ICatalogClient> GetCatalogClientAsync(CancellationToken cancellationToken = default)
        {
            return GetAsync(c => c.CatalogClient, cancellationToken);
        }

        private async Task<T> GetAsync<T>(Func<NuGetClients, T> clientFactory, CancellationToken cancellationToken)
        {
            if (_clients == null)
            {
                await _mutex.WaitAsync(cancellationToken);

                try
                {
                    if (_clients == null)
                    {
                        var serviceIndexClient = new RawServiceIndexClient(_httpClient, _serviceIndexUrl);

                        var serviceIndex = await serviceIndexClient.GetAsync(cancellationToken);

                        var contentResourceUrl = serviceIndex.GetPackageContentResourceUrl();
                        var metadataResourceUrl = serviceIndex.GetPackageMetadataResourceUrl();
                        var catalogResourceUrl = serviceIndex.GetCatalogResourceUrl();
                        var searchResourceUrl = serviceIndex.GetSearchQueryResourceUrl();
                        var autocompleteResourceUrl = serviceIndex.GetSearchAutocompleteResourceUrl();

                        // Create clients for required resources.
                        var contentClient = new RawPackageContentClient(_httpClient, contentResourceUrl);
                        var metadataClient = new RawPackageMetadataClient(_httpClient, metadataResourceUrl);
                        var searchClient = new RawSearchClient(_httpClient, searchResourceUrl);

                        // Create clients for optional resources.
                        var catalogClient = catalogResourceUrl == null
                            ? new NullCatalogClient() as ICatalogClient
                            : new RawCatalogClient(_httpClient, catalogResourceUrl);
                        var autocompleteClient = autocompleteResourceUrl == null
                            ? new NullAutocompleteClient() as IAutocompleteClient
                            : new RawAutocompleteClient(_httpClient, autocompleteResourceUrl);

                        _clients = new NuGetClients
                        {
                            ServiceIndex = serviceIndex,

                            PackageContentClient = contentClient,
                            PackageMetadataClient = metadataClient,
                            SearchClient = searchClient,
                            AutocompleteClient = autocompleteClient,
                            CatalogClient = catalogClient,
                        };
                    }
                }
                finally
                {
                    _mutex.Release();
                }
            }

            // TODO: This should periodically refresh the service index response.
            return clientFactory(_clients);
        }
    }
}
