using System.Collections.Generic;

namespace ProductApi.Models
{
    public class Customer
    {
        public long Id { get; set; }
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public required string Phone { get; set; }
        public required string Address { get; set; }
        public ICollection<Order> Orders { get; set; }
    }
}