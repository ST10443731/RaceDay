using System.Net;

namespace RaceDay.Api.Tests
{
    public class AuthTests
    {
        [Fact]
        public async Task Register_ValidOrganizer_Returns201()
        {
            using var factory = new ApiFactory();
            var client = factory.CreateClient();

            var response = await TestHelpers.Register(client, "org@test.com", "Organizer");

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task Register_DuplicateEmail_Returns409()
        {
            using var factory = new ApiFactory();
            var client = factory.CreateClient();

            await TestHelpers.Register(client, "same@test.com", "Participant");
            var second = await TestHelpers.Register(client, "same@test.com", "Participant");

            Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
        }

        [Fact]
        public async Task Register_InvalidRole_Returns400()
        {
            using var factory = new ApiFactory();
            var client = factory.CreateClient();

            var response = await TestHelpers.Register(client, "admin@test.com", "Admin");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Login_CorrectPassword_Returns200()
        {
            using var factory = new ApiFactory();
            var client = factory.CreateClient();
            await TestHelpers.Register(client, "user@test.com", "Participant");

            var response = await TestHelpers.Login(client, "user@test.com");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // YOUR TURN: fill in the blank before you run the tests
        [Fact]
        public async Task Login_WrongPassword_Fails()
        {
            using var factory = new ApiFactory();
            var client = factory.CreateClient();
            await TestHelpers.Register(client, "user@test.com", "Participant");

            var response = await TestHelpers.Login(client, "user@test.com", "wrong");

            // Which status code did Swagger give you for a wrong password?
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Profile_NotLoggedIn_Returns401()
        {
            using var factory = new ApiFactory();
            var client = factory.CreateClient();   // never logged in

            var response = await client.GetAsync("/api/users/me");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}