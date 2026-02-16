using System.ComponentModel.DataAnnotations;

namespace TiendaPromElec.DTOs
{
    public record CreateProductDto(
        [Required] string Name,
        [Required] string Description,
        [Required] string Brand,
        [Range(0.01, double.MaxValue)] decimal Price,
        [Range(0, int.MaxValue)] int Stock,
        string? ImageUrl,
        [Required] long CategoryId);
}
