using Aiursoft.BaGet.Core.Search;
using Aiursoft.BaGet.Protocol.Models;
using Aiursoft.BaGet.Web.Pages;
using Xunit;
using Assert = Xunit.Assert;

namespace Aiursoft.BaGet.Web.Tests.Pages
{
    public class IndexModelFacts
    {
        private readonly IndexModel _target;

        private SearchRequest _capturedRequest;
        private CancellationToken _capturedCancellation;
        private readonly SearchResponse _response = new();
        private readonly CancellationToken _cancellation = CancellationToken.None;

        public IndexModelFacts()
        {
            var search = new StubSearchService
            {
                Search = (request, token) =>
                {
                    _capturedCancellation = token;
                    _capturedRequest = request;
                    return Task.FromResult(_response);
                }
            };
            _target = new IndexModel(search);
        }

        [Fact]
        public async Task DefaultSearch()
        {
            await _target.OnGetAsync(_cancellation);

            Assert.Equal(_cancellation, _capturedCancellation);
            Assert.Equal(0, _capturedRequest.Skip);
            Assert.Equal(20, _capturedRequest.Take);
            Assert.True(_capturedRequest.IncludePrerelease);
            Assert.True(_capturedRequest.IncludeSemVer2);
            Assert.Null(_capturedRequest.PackageType);
            Assert.Null(_capturedRequest.Framework);
            Assert.Null(_capturedRequest.Query);
        }

        [Fact]
        public async Task AcceptsParameters()
        {
            _target.Prerelease = false;
            _target.PageIndex = 5;
            _target.PackageType = "foo";
            _target.Framework = "bar";
            _target.Query = "Hello world";

            await _target.OnGetAsync(_cancellation);

            Assert.Equal(_cancellation, _capturedCancellation);
            Assert.Equal(80, _capturedRequest.Skip);
            Assert.Equal(20, _capturedRequest.Take);
            Assert.False(_capturedRequest.IncludePrerelease);
            Assert.True(_capturedRequest.IncludeSemVer2);
            Assert.Equal("foo", _capturedRequest.PackageType);
            Assert.Equal("bar", _capturedRequest.Framework);
            Assert.Equal("Hello world", _capturedRequest.Query);
        }
    }
}
