using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace FleetManagement.Core.Models
{
    public class AppUser : IdentityUser
    {
        public string? FullName { get; set; }
        public int? VehicleId { get; set; }
        public Vehicle? Vehicle { get; set; }
    }
}

