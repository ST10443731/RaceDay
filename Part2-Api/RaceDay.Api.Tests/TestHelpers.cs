using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDay.Api.Tests
{
    public static class TestHelpers
    {
        public static Task<HttpResponseMessage> Register(
            HttpClient client, string email, string role, string password = "Pass1234")
            => client.PostAsJsonAsync("/api/auth/register",
                new { fullName = "Test User", email, password, role });

        public static Task<HttpResponseMessage> Login(
            HttpClient client, string email, string password = "Pass1234")
            => client.PostAsJsonAsync("/api/auth/login", new { email, password });

        // Registers a user, logs in, and returns a client that holds that user's session cookie.
        // Each client has its own cookie jar, like your normal and incognito windows.
        public static async Task<HttpClient> LoggedInClient(ApiFactory factory, string email, string role)
        {
            var client = factory.CreateClient();
            await Register(client, email, role);
            await Login(client, email);
            return client;
        }

        public static object EventBody(string name = "Test Fun Run", string eventType = "run") => new
        {
            name,
            description = "A test event",
            eventDate = DateTime.Now.AddDays(90),   // always in the future
            location = "Johannesburg",
            eventType,
            routeInfo = "Starts and ends at the park"
        };

        // Reads a number such as "eventId" out of a JSON response
        public static async Task<int> ReadId(HttpResponseMessage response, string property)
        {
            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            return json.GetProperty(property).GetInt32();
        }

        public static async Task<int> CreateEvent(HttpClient organizer)
        {
            var response = await organizer.PostAsJsonAsync("/api/events", EventBody());
            return await ReadId(response, "eventId");
        }
    }
}