using System.Net;
using System.Text.Json;

namespace RaceDay.Web.Services
{
    public class ApiResponse
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        public HttpStatusCode StatusCode { get; init; }
        public string Body { get; init; } = "";
        public string? SetCookie { get; init; }
        public bool ApiUnavailable { get; init; }

        public bool IsSuccess => (int)StatusCode >= 200 && (int)StatusCode < 300;

        public T? As<T>()
        {
            if (string.IsNullOrWhiteSpace(Body)) return default;
            return JsonSerializer.Deserialize<T>(Body, JsonOptions);
        }

        public string ErrorMessage()
        {
            if (ApiUnavailable)
                return "The RaceDay service is not available right now. Please try again in a moment.";
            if (StatusCode == HttpStatusCode.Forbidden)
                return "You do not have permission to do that.";
            if (StatusCode == HttpStatusCode.NotFound)
                return "We could not find what you were looking for.";

            var text = Body.Trim();

            if (text.Length == 0 && StatusCode == HttpStatusCode.Unauthorized)
                return "Please log in again.";

            if (text.StartsWith("{"))
            {
                try
                {
                    using var doc = JsonDocument.Parse(text);
                    if (doc.RootElement.TryGetProperty("errors", out var errors))
                    {
                        var messages = errors.EnumerateObject()
                            .SelectMany(p => p.Value.EnumerateArray().Select(m => m.GetString()));
                        return string.Join(" ", messages);
                    }
                }
                catch (JsonException) { }

                return "Some of the details you entered are not valid.";
            }

            if (text.Length > 0 && text.Length < 200) return text.Trim('"');

            return "Something went wrong. Please try again.";
        }
    }
}