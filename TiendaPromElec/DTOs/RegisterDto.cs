using System.ComponentModel.DataAnnotations;

namespace TiendaPromElec.DTOs;

public record RegisterDto(
        [Required] string FullName,
        [Required][EmailAddress] string Email,
        [Required] string Password,
        string? Phone,
        string? Address
    );
