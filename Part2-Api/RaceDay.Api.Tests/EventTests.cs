using System.Net;
using System.Net.Http.Json;

namespace RaceDay.Api.Tests
{
    public class EventTests
    {
        [Fact]
        public async Task Organizer_CreateEvent_Returns201()
        {
            using var factory = new ApiFactory();
            var organizer = await TestHelpers.LoggedInClient(factory, "org@test.com", "Organizer");

            var response = await organizer.PostAsJsonAsync("/api/events", TestHelpers.EventBody());

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task Organizer_CreateEvent_InvalidType_Returns400()
        {
            using var factory = new ApiFactory();
            var organizer = await TestHelpers.LoggedInClient(factory, "org@test.com", "Organizer");

            var response = await organizer.PostAsJsonAsync("/api/events",
                TestHelpers.EventBody(eventType: "swim"));   // only run, walk or cycle are allowed

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Organizer_UpdateOwnEvent_Returns200()
        {
            using var factory = new ApiFactory();
            var organizer = await TestHelpers.LoggedInClient(factory, "org@test.com", "Organizer");
            var eventId = await TestHelpers.CreateEvent(organizer);

            var response = await organizer.PutAsJsonAsync($"/api/events/{eventId}",
                TestHelpers.EventBody("Renamed Run"));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // YOUR TURN: fill in the blank before you run the tests
        [Fact]
        public async Task Organizer_UpdateAnotherOrganizersEvent_IsRejected()
        {
            using var factory = new ApiFactory();
            var owner = await TestHelpers.LoggedInClient(factory, "owner@test.com", "Organizer");
            var other = await TestHelpers.LoggedInClient(factory, "other@test.com", "Organizer");
            var eventId = await TestHelpers.CreateEvent(owner);

            // The second organiser tries to edit the first organiser's event
            var response = await other.PutAsJsonAsync($"/api/events/{eventId}", TestHelpers.EventBody());

            // In Swagger this gave 403. What is C#'s name for 403?
            Assert.Equal(HttpStatusCode.__________, response.StatusCode);
        }

        [Fact]
        public async Task Organizer_DeleteOwnEvent_Returns204_AndEventIsGone()
        {
            using var factory = new ApiFactory();
            var organizer = await TestHelpers.LoggedInClient(factory, "org@test.com", "Organizer");
            var eventId = await TestHelpers.CreateEvent(organizer);

            var delete = await organizer.DeleteAsync($"/api/events/{eventId}");
            var afterwards = await organizer.GetAsync($"/api/events/{eventId}");

            Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, afterwards.StatusCode);   // proves it was really deleted
        }

        [Fact]
        public async Task GetEvent_DoesNotExist_Returns404()
        {
            using var factory = new ApiFactory();
            var participant = await TestHelpers.LoggedInClient(factory, "p@test.com", "Participant");

            var response = await participant.GetAsync("/api/events/999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetEvents_NotLoggedIn_Returns401()
        {
            using var factory = new ApiFactory();
            var client = factory.CreateClient();   // never logged in

            var response = await client.GetAsync("/api/events");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}