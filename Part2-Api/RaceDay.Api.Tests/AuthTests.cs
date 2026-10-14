using System.Net;
using System.Net.Http.Json;

namespace RaceDay.Api.Tests
{
    public class AuthTests
    {
        [Fact]
        public async Task Register_ValidOrganizer_Returns201()
        {
            // Arrange: start the API and get a client that acts like a browser
            using var factory = new ApiFactory();
            var client = factory.CreateClient();

            // Act: send a register request
            var response = await client.PostAsJsonAsync("/api/auth/register", new
            {
                fullName = "Test Organizer",
                email = "org@test.com",
                password = "Pass1234",
                role = "Organizer"
            });

            // Assert: check the status code
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }
    }
}