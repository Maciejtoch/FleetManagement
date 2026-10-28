namespace FleetManagement.Models
{
    public class EditUserRoleViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
        public string? ReturnUrl { get; set; }
    }
}
