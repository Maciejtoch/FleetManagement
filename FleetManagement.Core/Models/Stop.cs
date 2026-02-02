using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetManagement.Core.Models
{
    public class Stop
    {
        public int Id { get; set; }

        [Required]
        public string CompanyName { get; set; }
        [Required]
        public string Address { get; set; }
    }
}
