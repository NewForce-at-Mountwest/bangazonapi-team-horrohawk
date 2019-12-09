using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonAPI.Models
{
    public class Customer
    {
        public int id { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }

        public DateTime AccountCreated { get; }
        public DateTime lastActive { get; }

        public List<PaymentType> PaymentType { get; set; } = new List<PaymentType>
            public List<Product> ProductsForSale { get; set; } = new List<Product>
    }
}
