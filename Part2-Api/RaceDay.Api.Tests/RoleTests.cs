using System.Net;
using System.Net.Http.Json;

namespace RaceDay.Api.Tests
{
    public class RoleTests
    {
        [Fact]
        public async Task Participant_CreateEvent_Returns403()
        {
            using var factory = new ApiFactory();
            var participant = await TestHelpers.LoggedInClient(factory, "p@test.com", "Participant");

            var response = await participant.PostAsJsonAsync("/api/events", TestHelpers.EventBody());

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Participant_CreateCategory_Returns403()
        {
            using var factory = new ApiFactory();
            var organizer = await TestHelpers.LoggedInClient(factory, "org@test.com", "Organizer");
            var participant = await TestHelpers.LoggedInClient(factory, "p@test.com", "Participant");
            var eventId = await TestHelpers.CreateEvent(organizer);

            var response = await participant.PostAsJsonAsync($"/api/events/{eventId}/categories",
                new { name = "5km", distanceKm = 5, maxParticipants = 10 });

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Organizer_Enrol_Returns403()
        {
            using var factory = new ApiFactory();
            var organizer = await TestHelpers.LoggedInClient(factory, "org@test.com", "Organizer");
            var eventId = await TestHelpers.CreateEvent(organizer);
            var categoryId = await TestHelpers.CreateCategory(organizer, eventId);

            var response = await organizer.PostAsync($"/api/categories/{categoryId}/enrolments", null);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Participant_ViewEventEnrolments_Returns403()
        {
            using var factory = new ApiFactory();
            var organizer = await TestHelpers.LoggedInClient(factory, "org@test.com", "Organizer");
            var participant = await TestHelpers.LoggedInClient(factory, "p@test.com", "Participant");
            var eventId = await TestHelpers.CreateEvent(organizer);

            var response = await participant.GetAsync($"/api/events/{eventId}/enrolments");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Participant_CaptureResult_Returns403()
        {
            using var factory = new ApiFactory();
            var participant = await TestHelpers.LoggedInClient(factory, "p@test.com", "Participant");

            var response = await participant.PostAsJsonAsync("/api/enrolments/1/result",
                new { finishTime = "00:52:30", position = 1 });

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}