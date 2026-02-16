using System;
using System.Collections.Generic;

namespace ProductApi.Models
{
    public class Order
    {
        public long Id { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public required string Status { get; set; }
        public long CustomerId { get; set; }
        public Customer Customer { get; set; }
        public ICollection<OrderDetail> Items { get; set; }
    }
}