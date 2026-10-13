using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos
{
    public class ResultDto
    {
        [Required]
        public TimeSpan FinishTime { get; set; }   // sent as text, e.g. "00:52:30"

        [Range(1, 100000, ErrorMessage = "Position must be 1 or higher.")]
        public int Position { get; set; }
    }
}