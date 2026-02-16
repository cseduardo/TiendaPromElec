using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using ProductApi.Data;
using ProductApi.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace TiendaPromElec.Test.Integration;

public class FlujosCompletosTests : IntegrationTestBase
{
    public FlujosCompletosTests(WebApplicationFactory<Program> factory) : base(factory) { }

    // ==========================================
    // GRUPO 1: PRUEBAS DE AUTENTICACIÓN
    // ==========================================

    [Fact]
    public async Task Registro_Y_Login_FuncionanCorrectamente()
    {
        // ARRANGE
        var email = GetUniqueEmail();
        var password = "SuperP@$$w0rd1.10";
        var registerData = new { FullName = "User Test", Email = email, Password = password, Role = "Client", Phone = "5555555", Address = "Calle Test" };

        // ACT 1: Registro
        var regResponse = await _client.PostAsync("/api/Auth/register", GetJsonContent(registerData));
        regResponse.EnsureSuccessStatusCode();

        // ACT 2: Login
        var loginResponse = await _client.PostAsync("/api/Auth/login", GetJsonContent(new { Email = email, Password = password }));

        // ASSERT
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var doc = JsonDocument.Parse(await loginResponse.Content.ReadAsStringAsync());
        Assert.True(doc.RootElement.GetProperty("token").GetString().Length > 0);
    }

    [Fact]
    public async Task Login_ConPasswordIncorrecto_DevuelveUnauthorized()
    {
        var loginData = new { Email = "no_existe@test.com", Password = "BadPassword!" };
        var response = await _client.PostAsync("/api/Auth/login", GetJsonContent(loginData));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Registro_EmailDuplicado_DevuelveError()
    {
        // ARRANGE: Registramos uno primero
        var email = GetUniqueEmail();
        await RegistrarUsuarioAsync(email, "Client");

        // ACT: Intentamos registrarlo OTRA VEZ
        var response = await _client.PostAsync("/api/Auth/register", GetJsonContent(new
        {
            FullName = "Clone",
            Email = email,
            Password = "Pass!",
            Role = "Client",
            Phone = "123",
            Address = "ABC"
        }));

        // ASSERT
        Assert.True(response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Registro_EmailFormatoMal_DevuelveBadRequest()
    {
        var userMal = new { FullName = "Test", Email = "esto-no-es-un-correo", Password = "Pass!", Role = "Client" };
        var response = await _client.PostAsync("/api/Auth/register", GetJsonContent(userMal));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ==========================================
    // GRUPO 2: PRUEBAS DE PRODUCTOS Y CATEGORÍAS
    // ==========================================

    [Fact]
    public async Task Obtener_Productos_DevuelveLista()
    {
        var response = await _client.GetAsync("/api/Product");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.StartsWith("[", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task GetProducto_IdExistente_DevuelveOK()
    {
        // Nos aseguramos que el producto 1 exista antes de consultar
        await AsegurarProductoExistenteAsync(1);

        var response = await _client.GetAsync("/api/Product/1");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetProducto_IdInexistente_DevuelveNotFound()
    {
        var response = await _client.GetAsync("/api/Product/999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetCategorias_DevuelveLista()
    {
        var response = await _client.GetAsync("/api/Category");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ==========================================
    // GRUPO 3: FLUJOS COMPLEJOS (ORDENES)
    // ==========================================

    [Fact]
    public async Task Usuario_Autenticado_Puede_Crear_Orden()
    {
        // ARRANGE
        await AsegurarProductoExistenteAsync(1); // Helper para crear producto en BD

        var token = await ObtenerTokenAsync("Client"); // Helper para autenticar
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var ordenData = new
        {
            Items = new[] { new { ProductId = 1, Quantity = 2 } }
        };

        // ACT
        var ordenRes = await _client.PostAsync("/api/Order", GetJsonContent(ordenData));

        // ASSERT
        var content = await ordenRes.Content.ReadAsStringAsync();
        if (ordenRes.StatusCode != HttpStatusCode.Created && ordenRes.StatusCode != HttpStatusCode.OK)
            throw new Exception($"FALLÓ ORDEN: {content}");

        Assert.True(ordenRes.StatusCode == HttpStatusCode.Created || ordenRes.StatusCode == HttpStatusCode.OK);
    }

    [Fact]
    public async Task Cliente_PuedeVer_MisPedidos()
    {
        var token = await ObtenerTokenAsync("Client");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/Order");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CrearOrden_SinProductos_DevuelveBadRequest()
    {
        var token = await ObtenerTokenAsync("Client");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var ordenVacia = new { Details = new object[] { } }; // O Items, segun tu API
        var response = await _client.PostAsync("/api/Order", GetJsonContent(ordenVacia));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ==========================================
    // GRUPO 4: SEGURIDAD Y ROLES
    // ==========================================

    [Fact]
    public async Task UsuarioLogueado_PuedeVer_SuPerfil()
    {
        var token = await ObtenerTokenAsync("Client");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/Auth/me");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Anonimo_NoPuede_CrearOrden()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var ordenData = new { Items = new[] { new { ProductId = 1, Quantity = 1 } } };

        var response = await _client.PostAsync("/api/Order", GetJsonContent(ordenData));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UsuarioCliente_NoPuede_BorrarCategoria()
    {
        var token = await ObtenerTokenAsync("Client");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.DeleteAsync("/api/Category/1");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Admin_PuedeCrear_Categoria()
    {
        // Usamos el Helper que crea un usuario y fuerza el rol en BD
        var token = await ObtenerTokenAsync("Admin");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var nuevaCat = new { Name = "Cat Admin " + Guid.NewGuid(), Products = new object[] { } };

        var response = await _client.PostAsync("/api/Category", GetJsonContent(nuevaCat));
        Assert.True(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created);
    }

    [Fact]
    public async Task CrearCategoria_NombreVacio_DevuelveBadRequest()
    {
        // Necesitamos ser Admin para llegar a la validación del nombre
        var token = await ObtenerTokenAsync("Admin");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var catInvalida = new { Name = "" };
        var response = await _client.PostAsync("/api/Category", GetJsonContent(catInvalida));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CrearProducto_PrecioNegativo_DevuelveBadRequest()
    {
        var token = await ObtenerTokenAsync("Admin");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var prodMalo = new { Name = "Malo", Price = -50, CategoryId = 1 };
        var response = await _client.PostAsync("/api/Product", GetJsonContent(prodMalo));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Cliente_NoPuede_CrearProducto()
    {
        var token = await ObtenerTokenAsync("Client");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var prod = new { Name = "Hack", Price = 100, CategoryId = 1 };
        var response = await _client.PostAsync("/api/Product", GetJsonContent(prod));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Anonimo_NoPuedeVer_MisPedidos()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/Order/my-orders"); // O la ruta correcta
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task TokenInvalido_DevuelveUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "token_falso_123");
        var response = await _client.GetAsync("/api/Auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ==========================================
    // HELPERS (MÉTODOS AUXILIARES REUTILIZABLES)
    // ==========================================

    /// <summary>
    /// Crea un usuario único, lo guarda en BD, (si es Admin fuerza el rol) y devuelve el Token.
    /// </summary>
    private async Task<string> ObtenerTokenAsync(string rol)
    {
        var email = GetUniqueEmail();
        var pass = "SuperP@$$w0rd1.10";

        // 1. Registro via API
        await _client.PostAsync("/api/Auth/register", GetJsonContent(new
        {
            FullName = $"User {rol}",
            Email = email,
            Password = pass,
            Role = rol,
            Phone = "55555555",
            Address = "Test Address"
        }));

        // 2. Si el rol es Admin, necesitamos forzarlo en la BD (bypass de seguridad API)
        if (rol == "Admin")
        {
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                // Ajusta 'Customers' o 'Users' según tu DbSet real
                var user = context.Customers.FirstOrDefault(u => u.Email == email);
                if (user != null)
                {
                    user.Role = "Admin";
                    await context.SaveChangesAsync();
                }
            }
        }

        // 3. Login
        var res = await _client.PostAsync("/api/Auth/login", GetJsonContent(new { Email = email, Password = pass }));
        var json = await res.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("token").GetString();
    }

    /// <summary>
    /// Helper simple para solo registrar sin loguear
    /// </summary>
    private async Task RegistrarUsuarioAsync(string email, string role)
    {
        await _client.PostAsync("/api/Auth/register", GetJsonContent(new
        {
            FullName = "Test User",
            Email = email,
            Password = "Password123!",
            Role = role,
            Phone = "12345678",
            Address = "Address 1"
        }));
    }

    /// <summary>
    /// Se asegura que el producto con ID especifico exista en la BD en memoria.
    /// </summary>
    private async Task AsegurarProductoExistenteAsync(int productId)
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            if (context.Products.FirstOrDefault(x=>x.Id==productId) == null)
            {
                if (context.Categories.FirstOrDefault(x => x.Id == 1) == null)
                    context.Categories.Add(new Category { Id = 1, Name = "General", Products = new List<Product>() });

                context.Products.Add(new Product
                {
                    Id = productId,
                    Name = "Producto Test Auto",
                    Price = 500,
                    CategoryId = 1,
                    Stock = 100,
                    Description = "Creado por Helper",
                    Brand = "Generic"
                });
                await context.SaveChangesAsync();
            }
        }
    }
}