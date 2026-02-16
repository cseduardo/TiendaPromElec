using System.ComponentModel.DataAnnotations;

namespace TiendaPromElec.DTOs;

public record LoginDto
(
    [Required]
    [EmailAddress]
    string Email,
    [Required]
    string Password
);
