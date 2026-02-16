namespace ProductApi.Models;

public class Order
{
    public long Id { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public required string Status { get; set; }
    // Relación con Customer (Según el Diagrama)
    public long CustomerId { get; set; }
    public Customer? Customer { get; set; }
    // Relación con Detalles
    public ICollection<OrderDetail>? Items { get; set; }
}