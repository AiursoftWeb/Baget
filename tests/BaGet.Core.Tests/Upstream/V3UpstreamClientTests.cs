using Aiursoft.BaGet.Core.Entities;
using Aiursoft.BaGet.Core.Upstream.Clients;
using Aiursoft.BaGet.Protocol;
using Aiursoft.BaGet.Protocol.Models;
using Microsoft.Extensions.Logging;
using Moq;
using NuGet.Versioning;
using Xunit;

namespace Aiursoft.BaGet.Core.Tests.Upstream
{
    public class V3UpstreamClientTests
    {
        public class ListPackageVersionsAsync : FactsBase
        {
            [Fact]
            public async Task ReturnsEmpty()
            {
                Client
                    .Setup(c => c.ListPackageVersionsAsync(
                        Id,
                        /*includeUnlisted: */ true,
                        Cancellation))
                    .ReturnsAsync(new List<NuGetVersion>());

                var result = await Target.ListPackageVersionsAsync(Id, Cancellation);

                Assert.Empty(result);
            }

            [Fact]
            public async Task IgnoresExceptions()
            {
                Client
                    .Setup(c => c.ListPackageVersionsAsync(
                        Id,
                        /*includeUnlisted: */ true,
                        Cancellation))
                    .ThrowsAsync(new InvalidDataException("Hello"));

                var result = await Target.ListPackageVersionsAsync(Id, Cancellation);

                Assert.Empty(result);
            }

            [Fact]
            public async Task ReturnsPackages()
            {
                Client
                    .Setup(c => c.ListPackageVersionsAsync(
                        Id,
                        /*includeUnlisted: */ true,
                        Cancellation))
                    .ReturnsAsync(new List<NuGetVersion> { Version });

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
                Client.Setup(c => c.GetPackageMetadataAsync(Id, Cancellation))
                    .ReturnsAsync(new List<PackageMetadata>());

                var result = await Target.ListPackagesAsync(Id, Cancellation);

                Assert.Empty(result);
            }

            [Fact]
            public async Task IgnoresExceptions()
            {
                Client.Setup(c => c.GetPackageMetadataAsync(Id, Cancellation))
                    .ThrowsAsync(new InvalidDataException("Hello world"));

                var result = await Target.ListPackagesAsync(Id, Cancellation);

                Assert.Empty(result);
            }

            [Fact]
            public async Task ReturnsPackages()
            {
                var published = DateTimeOffset.Now;

                Client.Setup(c => c.GetPackageMetadataAsync(Id, Cancellation))
                    .ReturnsAsync(new List<PackageMetadata>
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
                Client
                    .Setup(c => c.DownloadPackageAsync(Id, Version, Cancellation))
                    .ThrowsAsync(new PackageNotFoundException(Id, Version));

                var result = await Target.DownloadPackageOrNullAsync(Id, Version, Cancellation);

                Assert.Null(result);
            }

            [Fact]
            public async Task IgnoresExceptions()
            {
                Client
                    .Setup(c => c.DownloadPackageAsync(Id, Version, Cancellation))
                    .ThrowsAsync(new InvalidDataException("Hello world"));

                var result = await Target.DownloadPackageOrNullAsync(Id, Version, Cancellation);

                Assert.Null(result);
            }

            [Fact]
            public async Task ReturnsPackage()
            {
                Client
                    .Setup(c => c.DownloadPackageAsync(Id, Version, Cancellation))
                    .ReturnsAsync(new MemoryStream());

                var result = await Target.DownloadPackageOrNullAsync(Id, Version, Cancellation);

                Assert.NotNull(result);
                Assert.True(result.CanSeek);
            }
        }

        public class FactsBase
        {
            protected readonly Mock<NuGetClient> Client;
            protected readonly V3UpstreamClient Target;

            protected readonly string Id = "Foo";
            protected readonly NuGetVersion Version = new("1.2.3-prerelease+semver2");
            protected readonly CancellationToken Cancellation = CancellationToken.None;

            protected FactsBase()
            {
                Client = new Mock<NuGetClient>();
                Target = new V3UpstreamClient(
                    Client.Object,
                    Mock.Of<ILogger<V3UpstreamClient>>());
            }
        }
    }
}
