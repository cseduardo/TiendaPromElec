using System.Collections.Generic;

namespace ProductApi.Models
{
    public class Category
    {
        public long Id { get; set; }
        public required string Name { get; set; }
        public ICollection<Product> Products { get; set; }
    }
}