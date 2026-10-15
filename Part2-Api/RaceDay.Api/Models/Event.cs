using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models
{
    [Table("Event")]
    public class Event
    {
        public int EventId { get; set; }

        public int OrganizerId { get; set; }
        public User Organizer { get; set; } = null!;

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }   

        public DateTime EventDate { get; set; }

        [Required]
        [MaxLength(200)]
        public string Location { get; set; } = string.Empty;   

        [Required]
        [MaxLength(20)]
        public string EventType { get; set; } = string.Empty;  

        [Required]
        [MaxLength(600)]
        public string RouteInfo { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public List<Category> Categories { get; set; } = new();
    }
}
