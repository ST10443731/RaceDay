namespace RaceDay.Web.Models
{
    public class ResultViewModel
    {
        public int ResultId { get; set; }
        public string Event { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Category { get; set; } = string.Empty;
        public string FinishTime { get; set; } = string.Empty; // TimeSpan in API comes as string
        public int Position { get; set; }
        public DateTime RecordedAt { get; set; }
    }
}