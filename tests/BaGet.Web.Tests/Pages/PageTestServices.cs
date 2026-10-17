using Aiursoft.BaGet.Core;
using Aiursoft.BaGet.Core.Content;
using Aiursoft.BaGet.Core.Entities;
using Aiursoft.BaGet.Core.Search;
using Aiursoft.BaGet.Protocol.Models;
using NuGet.Versioning;
using Xunit;
using Assert = Xunit.Assert;

namespace Aiursoft.BaGet.Web.Tests.Pages;

internal sealed class StubSearchService : ISearchService
{
    public Func<SearchRequest, CancellationToken, Task<SearchResponse>> Search { get; set; }
    public DependentsResponse Dependents { get; set; } = new();
    public Task<SearchResponse> SearchAsync(SearchRequest request, CancellationToken cancellationToken) => Search(request, cancellationToken);
    public Task<DependentsResponse> FindDependentsAsync(string packageId, CancellationToken cancellationToken)
    {
        Assert.Equal("testpackage", packageId);
        Assert.Equal(TestContext.Current.CancellationToken, cancellationToken);
        return Task.FromResult(Dependents);
    }
    public Task<AutocompleteResponse> AutocompleteAsync(AutocompleteRequest request, CancellationToken cancellationToken) => throw new NotSupportedException();
    public Task<AutocompleteResponse> ListPackageVersionsAsync(VersionsRequest request, CancellationToken cancellationToken) => throw new NotSupportedException();
}

internal sealed class StubPackageService : IPackageService
{
    public IReadOnlyList<Package> Packages { get; set; } = Array.Empty<Package>();
    public Task<IReadOnlyList<Package>> FindPackagesAsync(string id, CancellationToken cancellationToken)
    {
        Assert.Equal("testpackage", id);
        Assert.Equal(TestContext.Current.CancellationToken, cancellationToken);
        return Task.FromResult(Packages);
    }
    public Task<IReadOnlyList<NuGetVersion>> FindPackageVersionsAsync(string id, CancellationToken cancellationToken) => throw new NotSupportedException();
    public Task<Package> FindPackageOrNullAsync(string id, NuGetVersion version, CancellationToken cancellationToken) => throw new NotSupportedException();
    public Task<bool> ExistsAsync(string id, NuGetVersion version, CancellationToken cancellationToken) => throw new NotSupportedException();
}

internal sealed class StubContentService : IPackageContentService
{
    public Stream Readme { get; set; }
    public Task<Stream> GetPackageReadmeStreamOrNullAsync(string id, NuGetVersion version, CancellationToken cancellationToken)
    {
        Assert.Equal("testpackage", id);
        Assert.Equal("1.0.0", version.OriginalVersion);
        Assert.Equal(TestContext.Current.CancellationToken, cancellationToken);
        return Task.FromResult(Readme);
    }
    public Task<PackageVersionsResponse> GetPackageVersionsOrNullAsync(string packageId, CancellationToken cancellationToken) => throw new NotSupportedException();
    public Task<Stream> GetPackageContentStreamOrNullAsync(string packageId, NuGetVersion packageVersion, CancellationToken cancellationToken) => throw new NotSupportedException();
    public Task<Stream> GetPackageManifestStreamOrNullAsync(string id, NuGetVersion version, CancellationToken cancellationToken) => throw new NotSupportedException();
    public Task<Stream> GetPackageIconStreamOrNullAsync(string id, NuGetVersion version, CancellationToken cancellationToken) => throw new NotSupportedException();
}
