namespace RaceDay.Web.Models
{
    public class OrganiserEnrolmentViewModel
    {
        public int EnrolmentId { get; set; }
        public DateTime EnrolmentDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Participant { get; set; } = string.Empty;
        public string ParticipantEmail { get; set; } = string.Empty;
    }

    public class CaptureResultViewModel
    {
        public string FinishTime { get; set; } = string.Empty;
        public int Position { get; set; }
    }
}