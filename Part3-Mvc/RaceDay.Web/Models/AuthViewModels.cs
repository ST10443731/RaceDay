using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Please enter your email.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your password.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Please enter your full name.")]
        [MaxLength(100)]
        [Display(Name = "Full name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your email.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please choose a password.")]
        [MinLength(6, ErrorMessage = "Your password must be at least 6 characters.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password.")]
        [Compare("Password", ErrorMessage = "The passwords do not match.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "Participant";
    }

    public class LoginResult
    {
        public int UserId { get; set; }
        public string Role { get; set; } = string.Empty;
    }

    public class ProfileResult
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}