using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models
{
    [Table("Role")]
    public class Role
    {
        public int RoleId { get; set; }

        [Required]
        [MaxLength(30)]
        public string RoleName { get; set; } = string.Empty;

        public List<User> Users { get; set; } = new();
    }
}
