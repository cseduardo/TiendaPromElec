using Newtonsoft.Json;

namespace ProductApi.Models;

public class Customer
{
    public long Id { get; set; }
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string Phone { get; set; }
    public required string Address { get; set; }
    // --- CAMPOS AGREGADOS PARA CUMPLIR REQUERIMIENTO DE SEGURIDAD ---
    // (No están en el diagrama visual, pero son necesarios para el Login)
    [JsonIgnore] // Para no devolver el password en las consultas
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = "Client"; // "Admin" o "Client"

    // Relación 1:N con Orders
    [JsonIgnore] // Evitar ciclos infinitos
    public ICollection<Order>? Orders { get; set; }
}