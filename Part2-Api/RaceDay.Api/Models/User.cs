using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace RaceDay.Api.Models
{
    [Table("User")]
    public class User
    {
        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        public int RoleId { get; set; }          // the foreign key column
        public Role Role { get; set; } = null!;  // the Role object it points to

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public List<Event> OrganisedEvents { get; set; } = new();
        public List<Enrolment> Enrolments { get; set; } = new();
    }
}