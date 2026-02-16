using System.ComponentModel.DataAnnotations;

namespace TiendaPromElec.DTOs;

public record CreateOrderDetailDto(
        [Required][Range(1, int.MaxValue)] int Quantity,
        [Required] long ProductId
    );