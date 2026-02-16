using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProductApi.Data;
using ProductApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TiendaPromElec.DTOs;

namespace TiendaPromElec.Controllers;


[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (await _context.Customers.AnyAsync(u => u.Email == dto.Email))
            return BadRequest("El correo ya está registrado.");
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        // CREAMOS UN CLIENTE (Cumpliendo el ERD)
        var customer = new Customer
        {
            FullName = dto.FullName,
            Email = dto.Email,
            Password = passwordHash,
            Phone = dto.Phone,
            Address = dto.Address,
            Role = "Client" // Por defecto
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Cliente registrado exitosamente" });
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login(LoginDto dto)
    {
        // 1. Buscamos en CUSTOMERS
        var customer = await _context.Customers
            .SingleOrDefaultAsync(u => u.Email == dto.Email);

        // 2. VERIFICAR SI EXISTE Y SI LA CONTRASEÑA COINCIDE
        // BCrypt.Verify hace la magia de comparar "123456" con "$2a$11$..."
        if (customer == null || !BCrypt.Net.BCrypt.Verify(dto.Password, customer.Password))
        {
            return Unauthorized("Credenciales inválidas");
        }

        // Generamos token con el ID del Cliente
        var token = GenerateJwtToken(customer);
        return Ok(new LoginResponseDto(token, customer.Email, customer.Role));
    }

    [HttpPut("update-profile")]
    [Authorize] // <--- Solo usuarios logueados
    public async Task<IActionResult> UpdateProfile(RegisterDto dto)
    {
        // Obtener ID del usuario desde el token
        var idClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        var userId = long.Parse(idClaim);

        var customer = await _context.Customers.FindAsync(userId);
        if (customer == null) return NotFound();

        // Actualizar datos
        customer.FullName = dto.FullName;
        customer.Phone = dto.Phone;
        customer.Address = dto.Address;

        // Solo actualizar contraseña si viene una nueva
        if (!string.IsNullOrEmpty(dto.Password))
        {
            customer.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        }

        await _context.SaveChangesAsync();
        return Ok(new { message = "Perfil actualizado" });
    }

    // Endpoint para LEER mis datos actuales (necesario para llenar el formulario)
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult> GetMyProfile()
    {
        var idClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        var userId = long.Parse(idClaim);
        var customer = await _context.Customers.FindAsync(userId);
        return Ok(customer);
    }

    private string GenerateJwtToken(Customer customer)
    {
        var secretKey = _configuration["JWT_SECRET"];
        var key = Encoding.ASCII.GetBytes(secretKey);
        var claims = new[]
        {
                // EL ID DEL TOKEN ES EL ID DEL CLIENTE. ¡PERFECTO PARA LAS ORDENES!
                new Claim(ClaimTypes.NameIdentifier, customer.Id.ToString()),
                new Claim(ClaimTypes.Name, customer.Email),
                new Claim(ClaimTypes.Role, customer.Role)
            };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(2),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
    }
}