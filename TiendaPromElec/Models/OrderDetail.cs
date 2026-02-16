using Newtonsoft.Json;

namespace ProductApi.Models;

public class OrderDetail
{
    public long Id { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    // Relación con Product
    public long ProductId { get; set; }
    public Product? Product { get; set; }
    // Relación con Order
    public long OrderId { get; set; }
    [JsonIgnore]
    public Order? Order { get; set; }
}