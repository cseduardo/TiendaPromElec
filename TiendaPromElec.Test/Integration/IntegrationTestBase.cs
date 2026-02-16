using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProductApi.Data;
using System.Text;
using System.Text.Json;

namespace TiendaPromElec.Test.Integration;

public class IntegrationTestBase : IClassFixture<WebApplicationFactory<Program>>
{
    protected readonly HttpClient _client;
    protected readonly WebApplicationFactory<Program> _factory;

    public IntegrationTestBase(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                // 1. Buscamos la configuración de base de datos existente (SQL Server)
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

                // 2. Si existe, la eliminamos
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // 3. Agregamos la nueva base de datos En Memoria
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase("MemoriaTestDB");
                });

                // 4. (Opcional pero recomendado) Construimos la BD para asegurarnos que esté creada
                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<AppDbContext>();

                    db.Database.EnsureCreated();
                    // Esto ejecutará tu DbSeeder automáticamente si lo tienes en Program.cs
                    // O creará la estructura de tablas vacía.
                }
            });
        });

        _client = _factory.CreateClient();
    }

    protected StringContent GetJsonContent(object data)
    {
        var json = JsonSerializer.Serialize(data);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    protected string GetUniqueEmail()
    {
        return $"test_{Guid.NewGuid()}@promelec.com";
    }
}