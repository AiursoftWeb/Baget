using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Aiursoft.BaGet.Web.Tests
{
    public class HealthTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;

        public HealthTests(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task HealthEndpointReturnsHealthy()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/health", TestContext.Current.CancellationToken);

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            Assert.Contains("Healthy", content);
        }
    }
}
