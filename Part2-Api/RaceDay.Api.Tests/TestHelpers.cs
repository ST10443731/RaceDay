using System.Net.Http.Json;

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
    }
}