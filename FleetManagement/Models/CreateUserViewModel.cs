using System.ComponentModel.DataAnnotations;

namespace FleetManagement.Models
{
    public class CreateUserViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [RegularExpression("^(User|TenantAdmin)$", ErrorMessage = "Dozwolone role to: Kierowca lub Admin (dla tenanta).")]
        public string Role { get; set; } = "User";
    }
}
