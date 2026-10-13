using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models
{
    [Table("Result")]
    public class Result
    {
        public int ResultId { get; set; }

        public int EnrolmentId { get; set; }
        public Enrolment Enrolment { get; set; } = null!;

        public TimeSpan FinishTime { get; set; }   // SQL TIME becomes TimeSpan in C#
        public int Position { get; set; }

        public DateTime RecordedAt { get; set; } = DateTime.Now;
    }
}