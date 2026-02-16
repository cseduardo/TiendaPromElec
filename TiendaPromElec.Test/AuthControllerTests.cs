using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using ProductApi.Data;
using ProductApi.Models;
using TiendaPromElec.Controllers;
using TiendaPromElec.DTOs;

namespace TiendaPromElec.Test;

public class AuthControllerTests
{
    private readonly Mock<IConfiguration> _mockConfig;

    public AuthControllerTests()
    {
        // 1. Simulamos el Configuration para el JWT Secret
        _mockConfig = new Mock<IConfiguration>();
        // El secreto debe ser largo para que el algoritmo HMAC no falle
        _mockConfig.Setup(c => c["JWT_SECRET"]).Returns("SuperSecretKeyParaTests_1234567890_MasLargaMejor");
    }

    // --- HELPER: Crear DB en memoria limpia para cada test ---
    private AppDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Nombre único para no mezclar datos entre tests
            .Options;

        var context = new AppDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    // --- TEST 1: REGISTRO EXITOSO ---
    [Fact]
    public async Task Register_DatosValidos_DeberiaCrearUsuario()
    {
        // ARRANGE
        using var context = GetInMemoryDbContext();
        var controller = new AuthController(context, _mockConfig.Object);

        var dto = new RegisterDto(
            "Juan Test",
            "juan@test.com",
            "Password123",
            "5551234",
            "Calle 1"
        );

        // ACT
        var result = await controller.Register(dto);

        // ASSERT
        // 1. Debe retornar Ok (200)
        var okResult = Assert.IsType<OkObjectResult>(result);

        // 2. Verificar que el usuario se guardó en la BD falsa
        var userEnDb = await context.Customers.FirstOrDefaultAsync(u => u.Email == "juan@test.com");
        Assert.NotNull(userEnDb);
        Assert.Equal("Juan Test", userEnDb.FullName);

        // 3. ¡SEGURIDAD! Verificar que la contraseña NO se guardó en texto plano
        Assert.NotEqual("Password123", userEnDb.Password);
        Assert.True(userEnDb.Password.StartsWith("$2a$"), "La contraseña debería ser un hash de BCrypt");
    }

    // --- TEST 2: REGISTRO FALLIDO (EMAIL DUPLICADO) ---
    [Fact]
    public async Task Register_EmailDuplicado_DeberiaRetornarBadRequest()
    {
        // ARRANGE
        using var context = GetInMemoryDbContext();
        // Pre-semilla: Insertamos un usuario existente
        context.Customers.Add(new Customer
        {
            FullName = "Ya Existe",
            Email = "duplicado@test.com",
            Password = "hash",
            Phone = "1",
            Address = "1"
        });
        await context.SaveChangesAsync();

        var controller = new AuthController(context, _mockConfig.Object);
        var dto = new RegisterDto("Nuevo", "duplicado@test.com", "123", "1", "1");

        // ACT
        var result = await controller.Register(dto);

        // ASSERT
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("El correo ya está registrado.", badRequest.Value);
    }

    // --- TEST 3: LOGIN EXITOSO ---
    [Fact]
    public async Task Login_CredencialesCorrectas_DeberiaRetornarToken()
    {
        // ARRANGE
        using var context = GetInMemoryDbContext();
        // Guardamos un usuario con contraseña hasheada REAL
        string hashPassword = BCrypt.Net.BCrypt.HashPassword("MiPasswordSeguro");

        context.Customers.Add(new Customer
        {
            FullName = "Login User",
            Email = "login@test.com",
            Password = hashPassword, // Guardamos el hash, no el texto plano
            Phone = "1",
            Address = "1",
            Role = "Client"
        });
        await context.SaveChangesAsync();

        var controller = new AuthController(context, _mockConfig.Object);
        var loginDto = new LoginDto("login@test.com", "MiPasswordSeguro");

        // ACT
        var result = await controller.Login(loginDto);

        // ASSERT
        // Login devuelve ActionResult<LoginResponseDto>, así que extraemos el Result
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var responseDto = Assert.IsType<LoginResponseDto>(okResult.Value);

        // Verificamos que venga un token
        Assert.False(string.IsNullOrEmpty(responseDto.Token));
        Assert.Equal("login@test.com", responseDto.Email);
    }

    // --- TEST 4: LOGIN FALLIDO (PASSWORD INCORRECTO) ---
    [Fact]
    public async Task Login_PasswordIncorrecto_DeberiaRetornarUnauthorized()
    {
        // ARRANGE
        using var context = GetInMemoryDbContext();
        string hashPassword = BCrypt.Net.BCrypt.HashPassword("Correcto");

        context.Customers.Add(new Customer
        {
            FullName = "User",
            Email = "user@test.com",
            Password = hashPassword,
            Phone = "1",
            Address = "1"
        });
        await context.SaveChangesAsync();

        var controller = new AuthController(context, _mockConfig.Object);
        // Intentamos entrar con password erróneo
        var loginDto = new LoginDto("user@test.com", "INCORRECTO");

        // ACT
        var result = await controller.Login(loginDto);

        // ASSERT
        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }
}
