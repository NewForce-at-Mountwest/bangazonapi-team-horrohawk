using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonAPI.Models
{
    public class Product
    {
        public int id { get; set; }
        public int price { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public int quantity { get; set; }
        public ProductType ProductType { get; set; }
        public Customer Customer { get; set; }
    }
}
