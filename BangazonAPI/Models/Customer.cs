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

        public DateTime accountCreated { get; }
        public DateTime lastActive { get; }

        public List<PaymentType> paymentTypes { get; set; } = new List<PaymentType> { };
        public List<Product> productsForSale { get; set; } = new List<Product> { };
    }
}
