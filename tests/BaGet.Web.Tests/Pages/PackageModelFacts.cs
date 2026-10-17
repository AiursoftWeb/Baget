using Aiursoft.BaGet.Core;
using Aiursoft.BaGet.Core.Entities;
using Aiursoft.BaGet.Core.Search;
using Aiursoft.BaGet.Web.Pages;
using NuGet.Versioning;
using Xunit;
using Assert = Xunit.Assert;

namespace Aiursoft.BaGet.Web.Tests.Pages
{
    public class PackageModelFacts
    {
        private readonly StubContentService _content;
        private readonly StubPackageService _packages;
        private readonly StubSearchService _search;
        private readonly PackageModel _target;

        public PackageModelFacts()
        {
            _content = new StubContentService();
            _packages = new StubPackageService();
            _search = new StubSearchService();
            var url = new StubUrlGenerator();
            _target = new PackageModel(
                _packages,
                _content,
                _search,
                url);

            _search.Dependents = new DependentsResponse();
        }

        [Fact]
        public async Task ReturnsNotFound()
        {
            _packages.Packages = new List<Package>();

            await _target.OnGetAsync("testpackage", "1.0.0", TestContext.Current.CancellationToken);

            Assert.False(_target.Found);
            Assert.Equal("testpackage", _target.Package.Id);
            Assert.Null(_target.DependencyGroups);
            Assert.Null(_target.Versions);
        }

        [Fact]
        public async Task ReturnsNotFoundIfAllUnlisted()
        {
            _packages.Packages = new List<Package>
                {
                    CreatePackage("1.0.0", listed: false),
                };

            await _target.OnGetAsync("testpackage", version: null, TestContext.Current.CancellationToken);

            Assert.False(_target.Found);
            Assert.Equal("testpackage", _target.Package.Id);
            Assert.Null(_target.DependencyGroups);
            Assert.Null(_target.Versions);
        }

        [Fact]
        public async Task ReturnsRequestedVersion()
        {
            _packages.Packages = new List<Package>
                {
                    CreatePackage("1.0.0"),
                    CreatePackage("2.0.0"),
                    CreatePackage("3.0.0"),
                };

            await _target.OnGetAsync("testpackage", "2.0.0", TestContext.Current.CancellationToken);

            Assert.True(_target.Found);
            Assert.Equal("testpackage", _target.Package.Id);
            Assert.Equal("2.0.0", _target.Package.NormalizedVersionString);

            Assert.Equal(3, _target.Versions.Count);
            Assert.Equal("3.0.0", _target.Versions[0].Version.OriginalVersion);
            Assert.False(_target.Versions[0].Selected);
            Assert.Equal("2.0.0", _target.Versions[1].Version.OriginalVersion);
            Assert.True(_target.Versions[1].Selected);
            Assert.Equal("1.0.0", _target.Versions[2].Version.OriginalVersion);
            Assert.False(_target.Versions[2].Selected);
        }

        [Fact]
        public async Task ReturnsRequestedUnlistedVersion()
        {
            _packages.Packages = new List<Package>
                {
                    CreatePackage("1.0.0"),
                    CreatePackage("2.0.0", listed: false),
                    CreatePackage("3.0.0"),
                };

            await _target.OnGetAsync("testpackage", "2.0.0", TestContext.Current.CancellationToken);

            Assert.True(_target.Found);
            Assert.Equal("testpackage", _target.Package.Id);
            Assert.Equal("2.0.0", _target.Package.NormalizedVersionString);

            Assert.Equal(2, _target.Versions.Count);
            Assert.Equal("3.0.0", _target.Versions[0].Version.OriginalVersion);
            Assert.False(_target.Versions[0].Selected);
            Assert.Equal("1.0.0", _target.Versions[1].Version.OriginalVersion);
            Assert.False(_target.Versions[1].Selected);
        }

        [Fact]
        public async Task FallsBackToLatestListedVersion()
        {
            _packages.Packages = new List<Package>
                {
                    CreatePackage("1.0.0"),
                    CreatePackage("2.0.0"),
                    CreatePackage("3.0.0", listed: false),
                };

            await _target.OnGetAsync("testpackage", "4.0.0", TestContext.Current.CancellationToken);

            Assert.True(_target.Found);
            Assert.Equal("testpackage", _target.Package.Id);
            Assert.Equal("2.0.0", _target.Package.NormalizedVersionString);

            Assert.Equal(2, _target.Versions.Count);
            Assert.Equal("2.0.0", _target.Versions[0].Version.OriginalVersion);
            Assert.True(_target.Versions[0].Selected);
            Assert.Equal("1.0.0", _target.Versions[1].Version.OriginalVersion);
            Assert.False(_target.Versions[1].Selected);
        }

        [Theory]
        [InlineData(new[] { "test" }, /*expectDotnetTemplate: */ false, /*expectDotnetTool: */ false)]
        [InlineData(new[] { "template" }, /*expectDotnetTemplate: */ true, /*expectDotnetTool: */ false)]
        [InlineData(new[] { "dOtNeTtOoL" }, /*expectDotnetTemplate: */ false, /*expectDotnetTool: */ true)]

        [InlineData(new[] { "tEmPlAte", "dOtNeTtOoL" }, /*expectDotnetTemplate: */ true, /*expectDotnetTool: */ true)]
        public async Task HandlesPackageTypes(IEnumerable<string> packageTypes, bool expectDotnetTemplate, bool expectDotnetTool)
        {
            _packages.Packages = new List<Package>
                {
                    CreatePackage("1.0.0", packageTypes: packageTypes)
                };

            await _target.OnGetAsync("testpackage", "1.0.0", TestContext.Current.CancellationToken);

            Assert.True(_target.Found);
            Assert.Equal(expectDotnetTemplate, _target.IsDotnetTemplate);
            Assert.Equal(expectDotnetTool, _target.IsDotnetTool);
        }

        [Fact]
        public async Task FindsDependentPackages()
        {
            _packages.Packages = new List<Package>
                {
                    CreatePackage("1.0.0")
                };

            _search.Dependents = new DependentsResponse
                {
                    Data = new List<PackageDependent>
                    {
                        new() { Id = "Used by 1" },
                        new() { Id = "Used by 2" },
                    }
                };

            await _target.OnGetAsync("testpackage", "1.0.0", TestContext.Current.CancellationToken);

            Assert.Equal(2, _target.UsedBy.Count);
            Assert.Equal("Used by 1", _target.UsedBy[0].Id);
            Assert.Equal("Used by 2", _target.UsedBy[1].Id);
        }

        [Fact]
        public async Task GroupsVersions()
        {
            _packages.Packages = new List<Package>
                {
                    CreatePackage("1.0.0", dependencies: new[]
                    {
                        new PackageDependency
                        {
                            TargetFramework = "net5.0",
                            Id = "Dependency1",
                            VersionRange = "[1.0.0, )",
                        },
                        new PackageDependency
                        {
                            TargetFramework = "net4.8",
                            Id = "Dependency2",
                            VersionRange = "[2.0.0, )",
                        },
                        new PackageDependency
                        {
                            TargetFramework = "net5.0",
                            Id = "Dependency3",
                            VersionRange = "[3.0.0, )",
                        },
                    })
                };

            await _target.OnGetAsync("testpackage", "1.0.0", TestContext.Current.CancellationToken);

            Assert.True(_target.Found);
            Assert.Equal(2, _target.DependencyGroups.Count);
            Assert.Equal(".NET 5.0", _target.DependencyGroups[0].Name);
            Assert.Equal(".NET Framework 4.8", _target.DependencyGroups[1].Name);

            Assert.Equal(2, _target.DependencyGroups[0].Dependencies.Count);
            Assert.Single(_target.DependencyGroups[1].Dependencies);

            Assert.Equal("Dependency1", _target.DependencyGroups[0].Dependencies[0].PackageId);
            Assert.Equal("(>= 1.0.0)", _target.DependencyGroups[0].Dependencies[0].VersionSpec);

            Assert.Equal("Dependency3", _target.DependencyGroups[0].Dependencies[1].PackageId);
            Assert.Equal("(>= 3.0.0)", _target.DependencyGroups[0].Dependencies[1].VersionSpec);

            Assert.Equal("Dependency2", _target.DependencyGroups[1].Dependencies[0].PackageId);
            Assert.Equal("(>= 2.0.0)", _target.DependencyGroups[1].Dependencies[0].VersionSpec);
        }

        [Theory]
        [InlineData(null, "All Frameworks")]
        [InlineData("net5.0", ".NET 5.0")]
        [InlineData("netstandard2.1", ".NET Standard 2.1")]
        [InlineData("netcoreapp3.1", ".NET Core 3.1")]
        [InlineData("net4.8", ".NET Framework 4.8")]
        public async Task PrettifiesTargetFramework(string targetFramework, string expectedResult)
        {
            _packages.Packages = new List<Package>
                {
                    CreatePackage("1.0.0", dependencies: new[]
                    {
                       new PackageDependency
                       {
                           TargetFramework = targetFramework,
                           Id = "DependencyPackage",
                           VersionRange = "[1.0.0, )",
                       }
                    })
                };

            await _target.OnGetAsync("testpackage", "1.0.0", TestContext.Current.CancellationToken);

            Assert.True(_target.Found);
            var group = Assert.Single(_target.DependencyGroups);
            Assert.Equal(expectedResult, group.Name);
        }

        [Fact]
        public async Task StatisticsIncludeUnlistedPackages()
        {
            var now = DateTime.Now;

            _packages.Packages = new List<Package>
                {
                    CreatePackage("1.0.0", published: DateTime.Now.AddDays(-2)),
                    CreatePackage("2.0.0", listed: false, published: now),
                };

            await _target.OnGetAsync("testpackage", "1.0.0", TestContext.Current.CancellationToken);

            Assert.True(_target.Found);
            Assert.Equal(now, _target.LastUpdated);
        }

        [Fact]
        public async Task RendersReadme()
        {
            using (var readmeStream = new MemoryStream())
            {
                using (var streamWriter = new StreamWriter(readmeStream, leaveOpen: true))
                {
                    await streamWriter.WriteLineAsync("# My readme".AsMemory(), TestContext.Current.CancellationToken);
                    await streamWriter.WriteLineAsync("Hello world!".AsMemory(), TestContext.Current.CancellationToken);
                    await streamWriter.FlushAsync(TestContext.Current.CancellationToken);
                }

                readmeStream.Position = 0;

                _packages.Packages = new List<Package>
                    {
                        CreatePackage("1.0.0", hasReadme: true),
                    };

                _content.Readme = readmeStream;

                await _target.OnGetAsync("testpackage", "1.0.0", TestContext.Current.CancellationToken);

                Assert.Equal(
                    "<h1 id=\"my-readme\">My readme</h1>\n<p>Hello world!</p>\n",
                    _target.Readme.Value);
            }
        }

        private Package CreatePackage(
            string version,
            bool hasReadme = false,
            bool listed = true,
            DateTime? published = null,
            IEnumerable<PackageDependency> dependencies = null,
            IEnumerable<string> packageTypes = null)
        {
            published = published ?? DateTime.Now;
            dependencies = dependencies ?? Array.Empty<PackageDependency>();
            packageTypes = packageTypes ?? Array.Empty<string>();

            return new Package
            {
                Id = "testpackage",
                HasReadme = hasReadme,
                Listed = listed,
                NormalizedVersionString = version,
                Published = published.Value,

                Dependencies = dependencies.ToList(),
                PackageTypes = packageTypes
                    .Select(name => new PackageType { Name = name })
                    .ToList(),
            };
        }
    }

    internal sealed class StubUrlGenerator : IUrlGenerator
    {
        public string GetServiceIndexUrl() => null;
        public string GetPackageContentResourceUrl() => null;
        public string GetPackageMetadataResourceUrl() => null;
        public string GetPackagePublishResourceUrl() => null;
        public string GetSymbolPublishResourceUrl() => null;
        public string GetSearchResourceUrl() => null;
        public string GetAutocompleteResourceUrl() => null;
        public string GetRegistrationIndexUrl(string id) => null;
        public string GetRegistrationPageUrl(string id, NuGetVersion lower, NuGetVersion upper) => null;
        public string GetRegistrationLeafUrl(string id, NuGetVersion version) => null;
        public string GetPackageVersionsUrl(string id) => null;
        public string GetPackageDownloadUrl(string id, NuGetVersion version) => null;
        public string GetPackageManifestDownloadUrl(string id, NuGetVersion version) => null;
        public string GetPackageIconDownloadUrl(string id, NuGetVersion version) => null;
    }
}
