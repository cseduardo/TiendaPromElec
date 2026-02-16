using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ProductApi.Models;
using TiendaPromElec.Controllers;
using TiendaPromElec.DTOs;
using TiendaPromElec.Repositories;

namespace TiendaPromElec.Tests;

public class ProductControllerTests
{
    // 1. Definimos los Mocks (objetos falsos)
    private readonly Mock<IProductRepository> _mockRepo;
    private readonly Mock<ILogger<ProductController>> _mockLogger;

    // 2. Definimos el controlador real que vamos a probar
    private readonly ProductController _controller;

    public ProductControllerTests()
    {
        // Inicializamos los mocks
        _mockRepo = new Mock<IProductRepository>();
        _mockLogger = new Mock<ILogger<ProductController>>();

        // Inyectamos los mocks en el controlador real
        _controller = new ProductController(_mockRepo.Object, _mockLogger.Object);
    }

    // --- TEST 1: OBTENER TODOS LOS PRODUCTOS ---
    [Fact]
    public async Task GetProducts_DeberiaRetornarListaDeProductos()
    {
        // ARRANGE (Preparar el escenario)
        // Le decimos al mock: "Cuando te pidan GetAllAsync, devuelve esta lista falsa"
        var fakeProducts = new List<Product>
        {
            new Product { Id = 1, Name = "Laptop Falsa", Price = 100,Description="Laptop Falsa de Prueba", Brand="Pruebas" },
            new Product {Id = 2, Name = "Mouse Falso", Price = 20, Description = "Laptop Falsa de Prueba", Brand="Pruebitas"}
        };

        _mockRepo.Setup(repo => repo.GetAllAsync())
                 .ReturnsAsync(fakeProducts);

        // ACT (Ejecutar la acción)
        var result = await _controller.GetProducts();

        // ASSERT (Verificar)
        // 1. Verificamos que sea un OkObjectResult (Status 200)
        var okResult = Assert.IsType<OkObjectResult>(result.Result);

        // 2. Verificamos que dentro venga la lista
        var returnProducts = Assert.IsType<List<Product>>(okResult.Value);

        // 3. Verificamos que sean 2 productos
        Assert.Equal(2, returnProducts.Count);
    }

    // --- TEST 2: OBTENER PRODUCTO POR ID (EXISTE) ---
    [Fact]
    public async Task GetProduct_SiExiste_DeberiaRetornarProducto()
    {
        // ARRANGE
        var fakeProduct = new Product { Id = 1, Name = "Producto 1", Description = "Producto prueba", Brand = "Pruebitas" };

        _mockRepo.Setup(repo => repo.GetByIdAsync(1))
                 .ReturnsAsync(fakeProduct);

        // ACT
        var result = await _controller.GetProduct(1);

        // ASSERT
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var product = Assert.IsType<Product>(okResult.Value);
        Assert.Equal("Producto 1", product.Name);
    }

    // --- TEST 3: OBTENER PRODUCTO POR ID (NO EXISTE) ---
    [Fact]
    public async Task GetProduct_SiNoExiste_DeberiaRetornarNotFound()
    {
        // ARRANGE
        // Simulamos que devuelve null
        _mockRepo.Setup(repo => repo.GetByIdAsync(99))
                 .ReturnsAsync((Product)null);

        // ACT
        var result = await _controller.GetProduct(99);

        // ASSERT
        // Esperamos un NotFoundResult (404)
        Assert.IsType<NotFoundResult>(result.Result);
    }

    // --- TEST 4: CREAR PRODUCTO (POST) ---
    [Fact]
    public async Task PostProduct_DeberiaCrearProductoYRetornarCreated()
    {
        // ARRANGE
        var newProductDto = new CreateProductDto(
    "Nuevo Producto",                 // Name
    "Descripción del nuevo producto", // Description
    "Marca Nueva",                    // Brand
    150m,                             // Price (nota la 'm' para decimal)
    30,                               // Stock
    "http://imagen.com/nuevo.jpg",    // ImageUrl
    2                                 // CategoryId
);

        // ACT
        var result = await _controller.PostProduct(newProductDto);

        // ASSERT
        // Verificamos que retorne CreatedAtAction (201)
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);

        // Verificamos que se haya llamado al método AddAsync del repositorio exactamente 1 vez
        _mockRepo.Verify(repo => repo.AddAsync(It.IsAny<Product>()), Times.Once);
    }

    // --- TEST 5: BORRAR PRODUCTO (SI EXISTE) ---
    [Fact]
    public async Task DeleteProduct_SiExiste_DeberiaRetornarNoContent()
    {
        // ARRANGE
        // 1. Decimos que SI existe
        _mockRepo.Setup(repo => repo.ExistsAsync(1)).ReturnsAsync(true);

        // ACT
        var result = await _controller.DeleteProduct(1);

        // ASSERT
        Assert.IsType<NoContentResult>(result); // 204

        // Verificamos que se llamó a DeleteAsync
        _mockRepo.Verify(repo => repo.DeleteAsync(1), Times.Once);
    }

    // --- TEST 6: BORRAR PRODUCTO (SI NO EXISTE) ---
    [Fact]
    public async Task DeleteProduct_SiNoExiste_DeberiaRetornarNotFound()
    {
        // ARRANGE
        // 1. Decimos que NO existe
        _mockRepo.Setup(repo => repo.ExistsAsync(99)).ReturnsAsync(false);

        // ACT
        var result = await _controller.DeleteProduct(99);

        // ASSERT
        Assert.IsType<NotFoundResult>(result); // 404

        // Verificamos que NUNCA se llamó a DeleteAsync real
        _mockRepo.Verify(repo => repo.DeleteAsync(It.IsAny<long>()), Times.Never);
    }
}