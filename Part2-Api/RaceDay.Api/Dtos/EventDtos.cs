using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RaceDay.Api.Dtos
{
    public class EventDto
    {
        [Required, MaxLength(150)] public string Name { get; set; } = string.Empty;
        [MaxLength(500)] public string? Description { get; set; }
        [Required] public DateTime EventDate { get; set; }
        [Required, MaxLength(200)] public string Location { get; set; } = string.Empty;

        [Required]
        [RegularExpression("^(run|walk|cycle)$", ErrorMessage = "EventType must be run, walk or cycle.")]
        public string EventType { get; set; } = string.Empty;

        [Required, MaxLength(600)] public string RouteInfo { get; set; } = string.Empty;

        // NEW: This allows the API to receive the uploaded image
        public IFormFile? BannerImage { get; set; }
    }
}