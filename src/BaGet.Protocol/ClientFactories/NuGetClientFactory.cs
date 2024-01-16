using Aiursoft.BaGet.Protocol.Catalog;
using Aiursoft.BaGet.Protocol.Models;
using Aiursoft.BaGet.Protocol.PackageContent;
using Aiursoft.BaGet.Protocol.PackageMetadata;
using Aiursoft.BaGet.Protocol.Search;

namespace Aiursoft.BaGet.Protocol.ClientFactories
{
    public class NuGetClients
    {
        public ServiceIndexResponse ServiceIndex { get; set; }
        public IPackageContentClient PackageContentClient { get; set; }
        public IPackageMetadataClient PackageMetadataClient { get; set; }
        public ISearchClient SearchClient { get; set; }
        public IAutocompleteClient AutocompleteClient { get; set; }
        public ICatalogClient CatalogClient { get; set; }
    }
}
