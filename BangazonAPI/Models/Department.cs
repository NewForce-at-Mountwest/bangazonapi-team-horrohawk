using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonAPI.Models
{
    public class Department
    {
        public int id { get; set; }
        public string name { get; set; }
        public int budget { get; set; }
        List<Employee> employees { get; set; } = new List<Employee> { };
}
}
