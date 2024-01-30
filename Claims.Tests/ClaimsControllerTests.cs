using Application.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using Xunit;

namespace Claims.Tests
{
    public class ClaimsControllerTests
    {
        [Fact]
        public async Task Get_Claims()
        {
            var application = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(_ =>
                { });

            var client = application.CreateClient();

            var response = await client.GetAsync("/Claims");

            response.EnsureSuccessStatusCode();

            //TODO: Apart from ensuring 200 OK being returned, what else can be asserted?

            Assert.Equal("application/json", response?.Content?.Headers?.ContentType?.MediaType);

            var responseContent = await response.Content.ReadAsStringAsync();
            var claims = JsonConvert.DeserializeObject<List<GetClaimModel>>(responseContent);

            Assert.NotEmpty(claims);

            foreach (var claim in claims)
            {
                Assert.NotNull(claim.Name);
            }

        }

    }
}
