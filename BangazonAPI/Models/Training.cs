using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonAPI.Models
{
    public class Training
    {
        public int id { get; set; }
        public string name { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public int maxAttendees { get; set; }
        public List<Employee> employeesInTraining { get; set; } = new List<Employee> { };
    }
}
