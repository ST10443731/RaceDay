using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace RaceDay.Web.Services
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            // USE THE DOCKER SERVICE NAME
            _httpClient.BaseAddress = new Uri("http://raceday-api:8080/");
        }

        private void AddAuthCookie(HttpRequestMessage request)
        {
            var cookie = _httpContextAccessor.HttpContext?.Session.GetString("ApiCookie");
            if (!string.IsNullOrEmpty(cookie))
            {
                request.Headers.Add("Cookie", cookie);
            }
        }

        public async Task<ApiResponse> Get(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            AddAuthCookie(request);
            var response = await _httpClient.SendAsync(request);
            return await ApiResponse.Create(response);
        }

        public async Task<ApiResponse> Post(string url, object data)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            AddAuthCookie(request);
            
            var json = JsonSerializer.Serialize(data);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            return await ApiResponse.Create(response);
        }

        public async Task<ApiResponse> PostForm(string url, Dictionary<string, string> formData, IFormFile? file)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            AddAuthCookie(request);

            var content = new MultipartFormDataContent();

            foreach (var key in formData.Keys)
            {
                content.Add(new StringContent(formData[key] ?? ""), key);
            }

            if (file != null && file.Length > 0)
            {
                var fileContent = new StreamContent(file.OpenReadStream());
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                content.Add(fileContent, "BannerImage", file.FileName);
            }

            request.Content = content;
            var response = await _httpClient.SendAsync(request);
            return await ApiResponse.Create(response);
        }
    }

    public class ApiResponse
    {
        public bool IsSuccess { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? SetCookie { get; set; }
        public System.Net.HttpStatusCode StatusCode { get; set; }
        public string? ErrorMessage() => IsSuccess ? null : Content;

        public T? As<T>() => string.IsNullOrEmpty(Content) ? default : JsonSerializer.Deserialize<T>(Content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        public static async Task<ApiResponse> Create(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            var setCookie = response.Headers.TryGetValues("Set-Cookie", out var cookies) ? cookies.FirstOrDefault() : null;
            
            return new ApiResponse
            {
                IsSuccess = response.IsSuccessStatusCode,
                StatusCode = response.StatusCode,
                Content = content,
                SetCookie = setCookie
            };
        }
    }
}