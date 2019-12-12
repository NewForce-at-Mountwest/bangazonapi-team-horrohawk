using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonAPI.Models
{
    public class Employee
    {
        public int id { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public bool isSupervisor { get; set; }
        public int departmentId { get; set; }
        public Department department { get; set; }
        public Computer computer { get; set; }
    }
}
