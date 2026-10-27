namespace RaceDay.Web.Models
{
    public class EnrolmentViewModel
    {
        public int EnrolmentId { get; set; }
        public DateTime EnrolmentDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Event { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Category { get; set; } = string.Empty;
        public double DistanceKm { get; set; }
    }
}