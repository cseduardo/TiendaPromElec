using System.ComponentModel.DataAnnotations;

namespace TiendaPromElec.DTOs;

public record CreateOrderDto(
        [Required] long CustomerId,
        [Required] List<CreateOrderDetailDto> Items
    );
