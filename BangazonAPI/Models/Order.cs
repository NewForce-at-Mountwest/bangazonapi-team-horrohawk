using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonAPI.Models
{
    public class Order
    {
        public int id { get; set; }
        public int paymentTypeId { get; set; }
        public int customerId { get; set; }
    }
}
