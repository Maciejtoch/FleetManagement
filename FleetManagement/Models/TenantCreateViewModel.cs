using System.ComponentModel.DataAnnotations;

namespace FleetManagement.Models
{
    public class TenantCreateViewModel
    {
        [Required]
        [Display(Name = "Nazwa firmy")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Slug")]
        public string Slug { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Domena / adres bazowy")]
        public string Domain { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "E-mail administratora")]
        public string AdminEmail { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Hasło administratora")]
        public string AdminPassword { get; set; } = string.Empty;
    }
}
