using System.Net;
using System.Net.Http.Json;

namespace RaceDay.Web.Services
{
    public class ApiClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _accessor;

        public ApiClient(HttpClient http, IHttpContextAccessor accessor)
        {
            _http = http;
            _accessor = accessor;
        }

        private ISession Session => _accessor.HttpContext!.Session;

        public Task<ApiResponse> Get(string url) => Send(HttpMethod.Get, url, null);
        public Task<ApiResponse> Post(string url, object? body = null) => Send(HttpMethod.Post, url, body);
        public Task<ApiResponse> Put(string url, object? body = null) => Send(HttpMethod.Put, url, body);
        public Task<ApiResponse> Delete(string url) => Send(HttpMethod.Delete, url, null);

        private async Task<ApiResponse> Send(HttpMethod method, string url, object? body)
        {
            var request = new HttpRequestMessage(method, url);

            if (body != null)
                request.Content = JsonContent.Create(body);

            var cookie = Session.GetString("ApiCookie");
            if (!string.IsNullOrEmpty(cookie))
                request.Headers.TryAddWithoutValidation("Cookie", cookie);

            try
            {
                var response = await _http.SendAsync(request);
                var text = await response.Content.ReadAsStringAsync();

                string? setCookie = null;
                if (response.Headers.TryGetValues("Set-Cookie", out var values))
                    setCookie = values.FirstOrDefault();

                return new ApiResponse
                {
                    StatusCode = response.StatusCode,
                    Body = text,
                    SetCookie = setCookie
                };
            }
            catch (HttpRequestException)
            {
                return new ApiResponse { StatusCode = HttpStatusCode.ServiceUnavailable, ApiUnavailable = true };
            }
        }
    }
}