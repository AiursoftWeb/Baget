using Aiursoft.BaGet.Core.Entities;
using Aiursoft.BaGet.Core.Upstream.Clients;
using Aiursoft.BaGet.Protocol;
using Aiursoft.BaGet.Protocol.Models;
using Microsoft.Extensions.Logging.Abstractions;
using NuGet.Versioning;
using Xunit;
using Assert = Xunit.Assert;

namespace Aiursoft.BaGet.Core.Tests.Upstream
{
    public class V3UpstreamClientTests
    {
        public class ListPackageVersionsAsync : FactsBase
        {
            [Fact]
            public async Task ReturnsEmpty()
            {
                Client.Versions = () => Task.FromResult<IReadOnlyList<NuGetVersion>>(new List<NuGetVersion>());

                var result = await Target.ListPackageVersionsAsync(Id, Cancellation);

                Assert.Empty(result);
            }

            [Fact]
            public async Task IgnoresExceptions()
            {
                Client.Versions = () => Task.FromException<IReadOnlyList<NuGetVersion>>(new InvalidDataException("Hello"));

                var result = await Target.ListPackageVersionsAsync(Id, Cancellation);

                Assert.Empty(result);
            }

            [Fact]
            public async Task ReturnsPackages()
            {
                Client.Versions = () => Task.FromResult<IReadOnlyList<NuGetVersion>>(new List<NuGetVersion> { Version });

                var result = await Target.ListPackageVersionsAsync(Id, Cancellation);

                var version = Assert.Single(result);
                Assert.Equal(Version, version);
            }
        }

        public class ListPackagesAsync : FactsBase
        {
            [Fact]
            public async Task ReturnsEmpty()
            {
                Client.Metadata = () => Task.FromResult<IReadOnlyList<PackageMetadata>>(new List<PackageMetadata>());

                var result = await Target.ListPackagesAsync(Id, Cancellation);

                Assert.Empty(result);
            }

            [Fact]
            public async Task IgnoresExceptions()
            {
                Client.Metadata = () => Task.FromException<IReadOnlyList<PackageMetadata>>(new InvalidDataException("Hello world"));

                var result = await Target.ListPackagesAsync(Id, Cancellation);

                Assert.Empty(result);
            }

            [Fact]
            public async Task ReturnsPackages()
            {
                var published = DateTimeOffset.Now;

                Client.Metadata = () => Task.FromResult<IReadOnlyList<PackageMetadata>>(new List<PackageMetadata>
                    {
                        new()
                        {
                            PackageId = "Foo",
                            Version = "1.2.3-prerelease+semver2",
                            Authors = "Author1, Author2",
                            Description = "Description",
                            IconUrl = "https://icon.test/",
                            Language = "Language",
                            LicenseUrl = "https://license.test/",
                            Listed = true,
                            MinClientVersion = "1.0.0",
                            PackageContentUrl = "https://content.test/",
                            Published = published,
                            RequireLicenseAcceptance = true,
                            Summary = "Summary",
                            Title = "Title",

                            Tags = new List<string> { "Tag1", "Tag2" },

                            Deprecation = new PackageDeprecation
                            {
                                Reasons = new List<string> { "Reason1", "Reason2" },
                                Message = "Message",
                                AlternatePackage = new AlternatePackage
                                {
                                    Id = "Alternate",
                                    Range = "*",
                                },
                            },
                            DependencyGroups = new List<DependencyGroupItem>
                            {
                                new()
                                {
                                    TargetFramework = "Target Framework",
                                    Dependencies = new List<DependencyItem>
                                    {
                                        new()
                                        {
                                            Id = "Dependency",
                                            Range = "1.0.0",
                                        }
                                    }
                                }
                            }
                        }
                    });

                var result = await Target.ListPackagesAsync(Id, Cancellation);

                var package = Assert.Single(result);

                Assert.Equal("Foo", package.Id);
                Assert.Equal(new[] { "Author1", "Author2"}, package.Authors);
                Assert.Equal("Description", package.Description);
                Assert.False(package.HasReadme);
                Assert.False(package.HasEmbeddedIcon);
                Assert.True(package.IsPrerelease);
                Assert.Null(package.ReleaseNotes);
                Assert.Equal("Language", package.Language);
                Assert.True(package.Listed);
                Assert.Equal("1.0.0", package.MinClientVersion);
                Assert.Equal(published.UtcDateTime, package.Published);
                Assert.True(package.RequireLicenseAcceptance);
                Assert.Equal(SemVerLevel.SemVer2, package.SemVerLevel);
                Assert.Equal("Summary", package.Summary);
                Assert.Equal("Title", package.Title);
                Assert.Equal("https://icon.test/", package.IconUrlString);
                Assert.Equal("https://license.test/", package.LicenseUrlString);
                Assert.Equal("", package.ProjectUrlString);
                Assert.Equal("", package.RepositoryUrlString);
                Assert.Null(package.RepositoryType);
                Assert.Equal(new[] { "Tag1", "Tag2" }, package.Tags);
                Assert.Equal("1.2.3-prerelease", package.NormalizedVersionString);
                Assert.Equal("1.2.3-prerelease+semver2", package.OriginalVersionString);
            }
        }

        public class DownloadPackageOrNullAsync : FactsBase
        {
            [Fact]
            public async Task ReturnsNull()
            {
                Client.Download = () => Task.FromException<Stream>(new PackageNotFoundException(Id, Version));

                var result = await Target.DownloadPackageOrNullAsync(Id, Version, Cancellation);

                Assert.Null(result);
            }

            [Fact]
            public async Task IgnoresExceptions()
            {
                Client.Download = () => Task.FromException<Stream>(new InvalidDataException("Hello world"));

                var result = await Target.DownloadPackageOrNullAsync(Id, Version, Cancellation);

                Assert.Null(result);
            }

            [Fact]
            public async Task ReturnsPackage()
            {
                Client.Download = () => Task.FromResult<Stream>(new MemoryStream());

                var result = await Target.DownloadPackageOrNullAsync(Id, Version, Cancellation);

                Assert.NotNull(result);
                Assert.True(result.CanSeek);
            }
        }

        public class FactsBase
        {
            protected readonly StubNuGetClient Client;
            protected readonly V3UpstreamClient Target;

            protected readonly string Id = "Foo";
            protected readonly NuGetVersion Version = new("1.2.3-prerelease+semver2");
            protected readonly CancellationToken Cancellation = CancellationToken.None;

            protected FactsBase()
            {
                Client = new StubNuGetClient();
                Target = new V3UpstreamClient(
                    Client,
                    NullLogger<V3UpstreamClient>.Instance);
            }
        }
    }
    public sealed class StubNuGetClient : NuGetClient
    {
        public Func<Task<IReadOnlyList<NuGetVersion>>> Versions { get; set; }
        public Func<Task<IReadOnlyList<PackageMetadata>>> Metadata { get; set; }
        public Func<Task<Stream>> Download { get; set; }

        public override Task<IReadOnlyList<NuGetVersion>> ListPackageVersionsAsync(string packageId, bool includeUnlisted, CancellationToken cancellationToken = default)
        {
            Assert.Equal("Foo", packageId);
            Assert.True(includeUnlisted);
            Assert.Equal(CancellationToken.None, cancellationToken);
            return Versions();
        }

        public override Task<IReadOnlyList<PackageMetadata>> GetPackageMetadataAsync(string packageId, CancellationToken cancellationToken = default)
        {
            Assert.Equal("Foo", packageId);
            Assert.Equal(CancellationToken.None, cancellationToken);
            return Metadata();
        }

        public override Task<Stream> DownloadPackageAsync(string packageId, NuGetVersion packageVersion, CancellationToken cancellationToken = default)
        {
            Assert.Equal("Foo", packageId);
            Assert.Equal("1.2.3-prerelease+semver2", packageVersion.ToFullString());
            Assert.Equal(CancellationToken.None, cancellationToken);
            return Download();
        }
    }
}
