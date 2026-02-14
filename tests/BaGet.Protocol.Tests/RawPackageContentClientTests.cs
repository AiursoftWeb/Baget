using Aiursoft.BaGet.Protocol.PackageContent;
using Aiursoft.BaGet.Protocol.Tests.Support;
using Xunit;
using Assert = Xunit.Assert;

namespace Aiursoft.BaGet.Protocol.Tests
{
    public class RawPackageContentTests : IClassFixture<ProtocolFixture>
    {
        private readonly RawPackageContentClient _target;

        public RawPackageContentTests(ProtocolFixture fixture)
        {
            _target = fixture.ContentClient;
        }

        [Fact]
        public async Task GetsPackageVersions()
        {
            var result = await _target.GetPackageVersionsOrNullAsync("Test.Package", TestContext.Current.CancellationToken);

            Assert.NotNull(result);
            Assert.Equal(2, result.Versions.Count);
            Assert.Equal("1.0.0", result.Versions[0]);
            Assert.Equal("2.0.0", result.Versions[1]);
        }

        [Fact]
        public async Task ReturnsNullIfPackageDoesNotExist()
        {
            var result = await _target.GetPackageVersionsOrNullAsync(Guid.NewGuid().ToString(), TestContext.Current.CancellationToken);

            Assert.Null(result);
        }

        // TODO: Test package download
        // TODO: Test package manifest download
    }
}
