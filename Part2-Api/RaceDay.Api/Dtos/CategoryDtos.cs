using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos
{
    public class CategoryDto
    {
        [Required, MaxLength(50)] public string Name { get; set; } = string.Empty;

        [Range(0.01, 1000, ErrorMessage = "DistanceKm must be between 0.01 and 1000.")]
        public decimal DistanceKm { get; set; }

        [Range(1, 100000, ErrorMessage = "MaxParticipants must be at least 1.")]
        public int MaxParticipants { get; set; } = 100;
    }
}
