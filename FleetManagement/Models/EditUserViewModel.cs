using FleetManagement.Core.Models;


namespace FleetManagement.Models
{
    public class EditUserViewModel
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public int? VehicleId { get; set; }
        public List<Vehicle> Vehicles { get; set; } = new();
    }
}
