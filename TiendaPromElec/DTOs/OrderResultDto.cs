using ProductApi.Models;

namespace TiendaPromElec.DTOs;

public class OrderResultDto
{
    public Order Order { get; set; }
    public List<string> Warnings { get; set; }
}
