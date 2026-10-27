using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.Models
{
    public class CategoryViewModel
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public double DistanceKm { get; set; }
        public int MaxParticipants { get; set; }
    }

    public class CreateCategoryViewModel
    {
        [Required(ErrorMessage = "Category name is required.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Distance is required.")]
        [Range(0.1, 1000, ErrorMessage = "Distance must be greater than 0.")]
        public double DistanceKm { get; set; }

        [Required(ErrorMessage = "Max participants is required.")]
        [Range(1, 100000, ErrorMessage = "Max participants must be at least 1.")]
        public int MaxParticipants { get; set; }
    }
}