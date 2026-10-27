using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RaceDay.Web.Models
{
    public class EventViewModel
    {
        public int EventId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        
        // The API sends "EventDate"
        public DateTime EventDate { get; set; }
        
        public string Location { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public string RouteInfo { get; set; } = string.Empty;
        public string? BannerUrl { get; set; }
        public string Organizer { get; set; } = string.Empty;
    }

    public class CreateEventViewModel
    {
        [Required(ErrorMessage = "Event name is required.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date is required.")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Now.AddDays(7);

        [Required(ErrorMessage = "Location is required.")]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Event Type is required.")]
        public string EventType { get; set; } = string.Empty; 

        [Required(ErrorMessage = "Route Info is required.")]
        public string RouteInfo { get; set; } = string.Empty;

        public IFormFile? BannerImage { get; set; }
    }
}