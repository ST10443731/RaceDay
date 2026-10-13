using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models
{
    [Table("Category")]
    public class Category
    {
        public int CategoryId { get; set; }

        public int EventId { get; set; }
        public Event Event { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [Column(TypeName = "decimal(5,2)")]   // DECIMAL(5,2)
        public decimal DistanceKm { get; set; }

        public int MaxParticipants { get; set; } = 100;   // SQL default is 100

        public List<Enrolment> Enrolments { get; set; } = new();
    }
}
