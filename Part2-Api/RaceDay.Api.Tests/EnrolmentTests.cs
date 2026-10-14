using System.Net;

namespace RaceDay.Api.Tests
{
    public class EnrolmentTests
    {
        [Fact]
        public async Task Participant_Enrol_Returns201_AndEnrolmentIsRecorded()
        {
            using var factory = new ApiFactory();
            var organizer = await TestHelpers.LoggedInClient(factory, "org@test.com", "Organizer");
            var participant = await TestHelpers.LoggedInClient(factory, "p@test.com", "Participant");
            var eventId = await TestHelpers.CreateEvent(organizer);
            var categoryId = await TestHelpers.CreateCategory(organizer, eventId);

            var enrol = await participant.PostAsync($"/api/categories/{categoryId}/enrolments", null);
            var mine = await participant.GetAsync("/api/users/me/enrolments");

            Assert.Equal(HttpStatusCode.Created, enrol.StatusCode);
            Assert.Equal(1, await TestHelpers.CountItems(mine));   // the enrolment really was saved
        }

        [Fact]
        public async Task Participant_EnrolTwice_Returns409()
        {
            using var factory = new ApiFactory();
            var organizer = await TestHelpers.LoggedInClient(factory, "org@test.com", "Organizer");
            var participant = await TestHelpers.LoggedInClient(factory, "p@test.com", "Participant");
            var eventId = await TestHelpers.CreateEvent(organizer);
            var categoryId = await TestHelpers.CreateCategory(organizer, eventId);

            await participant.PostAsync($"/api/categories/{categoryId}/enrolments", null);
            var second = await participant.PostAsync($"/api/categories/{categoryId}/enrolments", null);

            Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
        }

        [Fact]
        public async Task Enrol_CategoryIsFull_Returns409()
        {
            using var factory = new ApiFactory();
            var organizer = await TestHelpers.LoggedInClient(factory, "org@test.com", "Organizer");
            var first = await TestHelpers.LoggedInClient(factory, "p1@test.com", "Participant");
            var second = await TestHelpers.LoggedInClient(factory, "p2@test.com", "Participant");
            var eventId = await TestHelpers.CreateEvent(organizer);
            var categoryId = await TestHelpers.CreateCategory(organizer, eventId, max: 1);   // room for ONE person

            await first.PostAsync($"/api/categories/{categoryId}/enrolments", null);
            var response = await second.PostAsync($"/api/categories/{categoryId}/enrolments", null);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task Enrol_CategoryDoesNotExist_Returns404()
        {
            using var factory = new ApiFactory();
            var participant = await TestHelpers.LoggedInClient(factory, "p@test.com", "Participant");

            var response = await participant.PostAsync("/api/categories/999/enrolments", null);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Organizer_ViewsEnrolmentsForOwnEvent_Returns200()
        {
            using var factory = new ApiFactory();
            var organizer = await TestHelpers.LoggedInClient(factory, "org@test.com", "Organizer");
            var participant = await TestHelpers.LoggedInClient(factory, "p@test.com", "Participant");
            var eventId = await TestHelpers.CreateEvent(organizer);
            var categoryId = await TestHelpers.CreateCategory(organizer, eventId);
            await participant.PostAsync($"/api/categories/{categoryId}/enrolments", null);

            var response = await organizer.GetAsync($"/api/events/{eventId}/enrolments");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(1, await TestHelpers.CountItems(response));
        }

        [Fact]
        public async Task Participant_WithdrawAnotherParticipantsEnrolment_Returns403()
        {
            using var factory = new ApiFactory();
            var organizer = await TestHelpers.LoggedInClient(factory, "org@test.com", "Organizer");
            var owner = await TestHelpers.LoggedInClient(factory, "p1@test.com", "Participant");
            var other = await TestHelpers.LoggedInClient(factory, "p2@test.com", "Participant");
            var eventId = await TestHelpers.CreateEvent(organizer);
            var categoryId = await TestHelpers.CreateCategory(organizer, eventId);
            var enrol = await owner.PostAsync($"/api/categories/{categoryId}/enrolments", null);
            var enrolmentId = await TestHelpers.ReadId(enrol, "enrolmentId");

            var response = await other.DeleteAsync($"/api/enrolments/{enrolmentId}");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}
