using Aiursoft.BaGet.Core.Entities;
using Aiursoft.BaGet.Core.Metadata;
using Moq;
using NuGet.Versioning;
using Xunit;
using Assert = Xunit.Assert;

namespace Aiursoft.BaGet.Core.Tests.Metadata
{
    public class RegistrationBuilderTests
    {
        private readonly Mock<IUrlGenerator> _urlGenerator;

        public RegistrationBuilderTests()
        {
            _urlGenerator = new Mock<IUrlGenerator>();
        }

        [Fact]
        public void TheRegistrationIndexResponseIsSortedByVersion()
        {
            // Arrange
            var packageId = "BaGet.Test";

            var packages = new List<Package>
            {
                GetTestPackage(packageId, "3.1.0"),
                GetTestPackage(packageId, "10.0.5"),
                GetTestPackage(packageId, "3.2.0"),
                GetTestPackage(packageId, "3.1.0-pre"),
                GetTestPackage(packageId, "1.0.0-beta1"),
                GetTestPackage(packageId, "1.0.0"),
            };

            var registration = new PackageRegistration(packageId, packages);

            var registrationBuilder = new RegistrationBuilder(_urlGenerator.Object);

            // Act
            var response = registrationBuilder.BuildIndex(registration);

            // Assert
            Assert.Equal(packages.Count, response.Pages[0].ItemsOrNull.Count);
            var index = 0;
            foreach (var package in packages.OrderBy(p => p.Version))
            {
                Assert.Equal(package.Version.ToFullString(), response.Pages[0].ItemsOrNull[index++].PackageMetadata.Version);
            }
        }

        [Theory]
        [InlineData(0L, DateTimeKind.Unspecified)]
        [InlineData(0L, DateTimeKind.Utc)]
        [InlineData(638000000000000000L, DateTimeKind.Unspecified)]
        [InlineData(638000000000000000L, DateTimeKind.Utc)]
        public void PublicationTimestampsRemainUtc(long ticks, DateTimeKind kind)
        {
            var package = GetTestPackage("BaGet.Test", "1.0.0");
            package.Published = new DateTime(ticks, kind);
            var builder = new RegistrationBuilder(_urlGenerator.Object);
            var registration = new PackageRegistration(package.Id, new[] { package });

            var index = builder.BuildIndex(registration);
            var leaf = builder.BuildLeaf(package);
            var expected = new DateTimeOffset(new DateTime(ticks, DateTimeKind.Utc));

            Assert.Equal(expected, index.Pages[0].ItemsOrNull[0].PackageMetadata.Published);
            Assert.Equal(TimeSpan.Zero, index.Pages[0].ItemsOrNull[0].PackageMetadata.Published.Offset);
            Assert.Equal(expected, leaf.Published);
            Assert.Equal(TimeSpan.Zero, leaf.Published.Offset);
        }

        /// <summary>
        /// Create a fake <see cref="Package"></see> with the minimum metadata needed by the <see cref="RegistrationBuilder"></see>.
        /// </summary>
        private Package GetTestPackage(string packageId, string version)
        {
            return new Package
            {
                Id = packageId,
                Authors = new[] { "test" },
                PackageTypes = new List<PackageType> { new() { Name = "test" } },
                Dependencies = new List<PackageDependency>(),
                Version = new NuGetVersion(version),
            };
        }
    }
}
