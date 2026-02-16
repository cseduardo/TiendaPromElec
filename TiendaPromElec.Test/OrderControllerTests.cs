using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ProductApi.Models;
using System.Security.Claims;
using TiendaPromElec.Controllers;
using TiendaPromElec.DTOs;
using TiendaPromElec.Repositories;

namespace TiendaPromElec.Test;

public class OrderControllerTests
{
    private readonly Mock<IOrderRepository> _mockOrderRepo;
    private readonly Mock<IProductRepository> _mockProductRepo;
    private readonly Mock<ILogger<OrderController>> _mockLogger;
    private readonly OrderController _controller;

    public OrderControllerTests()
    {
        _mockOrderRepo = new Mock<IOrderRepository>();
        _mockProductRepo = new Mock<IProductRepository>();
        _mockLogger = new Mock<ILogger<OrderController>>();

        _controller = new OrderController(_mockOrderRepo.Object, _mockLogger.Object, _mockProductRepo.Object);
    }

    // --- HELPER: Simular Usuario Logueado ---
    private void SetupUser(long userId, string role)
    {
        var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role),
                new Claim(ClaimTypes.Name, "test@user.com")
            };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    // --- TEST 1: ADMIN VE TODO ---
    [Fact]
    public async Task GetOrders_ComoAdmin_DeberiaRetornarTodas()
    {
        // ARRANGE
        SetupUser(1, "Admin");
        var fakeOrders = new List<Order> { new Order { Id = 1, Status = "Pending" }, new Order { Id = 2, Status = "Sent" } };

        _mockOrderRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(fakeOrders);

        // ACT
        var result = await _controller.GetOrders();

        // ASSERT
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var orders = Assert.IsType<List<Order>>(okResult.Value);
        Assert.Equal(2, orders.Count);
    }

    // --- TEST 2: CLIENTE VE SOLO LO SUYO ---
    [Fact]
    public async Task GetOrders_ComoCliente_DeberiaRetornarSoloSuyas()
    {
        // ARRANGE
        long myId = 10;
        SetupUser(myId, "Client");
        var myOrders = new List<Order> { new Order { Id = 5, CustomerId = myId, Status = "Pending" } };

        _mockOrderRepo.Setup(repo => repo.GetOrdersByCustomerAsync(myId)).ReturnsAsync(myOrders);

        // ACT
        var result = await _controller.GetOrders();

        // ASSERT
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var orders = Assert.IsType<List<Order>>(okResult.Value);
        Assert.Single(orders);
        Assert.Equal(5, orders[0].Id);
    }

    // --- TEST 3: CREAR ORDEN EXITOSA (STOCK SUFICIENTE) ---
    [Fact]
    public async Task PostOrder_StockSuficiente_CreaOrdenYDescuentaStock()
    {
        // ARRANGE
        SetupUser(50, "Client"); // Usuario ID 50 logueado

        // 1. Preparamos el producto (Tiene 10 en stock)
        // IMPORTANTE: Ponemos properties requeridas como Name/Brand para que no falle al instanciar si el modelo lo pide
        var product = new Product
        {
            Id = 1,
            Name = "Laptop",
            Description = "Desc",
            Brand = "Dell",
            Stock = 10,
            Price = 1000
        };

        _mockProductRepo.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(product);

        // 2. Preparamos el DTO usando tus Records Posicionales
        // CreateOrderDetailDto(Quantity, ProductId)
        var detailDto = new CreateOrderDetailDto(2, 1);

        // CreateOrderDto(CustomerId, Items) -> El CustomerId aquí (0) el controller lo ignora y usa el del token (50)
        var orderDto = new CreateOrderDto(0, new List<CreateOrderDetailDto> { detailDto });

        // ACT
        var result = await _controller.PostOrder(orderDto);

        // ASSERT
        // 1. Debe retornar 201 Created
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);

        // 2. Lo que devuelve debe ser un OrderResultDto (tu clase envoltorio)
        var resultDto = Assert.IsType<OrderResultDto>(createdResult.Value);
        Assert.NotNull(resultDto.Order);

        // 3. Verificamos lógica de negocio: Stock bajó de 10 a 8
        Assert.Equal(8, product.Stock);

        // 4. Verificamos que se llamó a Update (para stock) y Add (para orden)
        _mockProductRepo.Verify(repo => repo.UpdateAsync(product), Times.Once);
        _mockOrderRepo.Verify(repo => repo.AddAsync(It.IsAny<Order>()), Times.Once);
    }

    // --- TEST 4: ERROR DE STOCK (STOCK INSUFICIENTE) ---
    [Fact]
    public async Task PostOrder_StockInsuficiente_RetornaBadRequest()
    {
        // ARRANGE
        SetupUser(50, "Client");

        // Producto con SOLO 1 en stock
        var product = new Product
        {
            Id = 2,
            Name = "Mouse",
            Description = "D",
            Brand = "B",
            Stock = 1,
            Price = 50
        };
        _mockProductRepo.Setup(repo => repo.GetByIdAsync(2)).ReturnsAsync(product);

        // Intentamos comprar 5 unidades
        var detailDto = new CreateOrderDetailDto(5, 2);
        var orderDto = new CreateOrderDto(0, new List<CreateOrderDetailDto> { detailDto });

        // ACT
        var result = await _controller.PostOrder(orderDto);

        // ASSERT
        // Tu controller devuelve BadRequest si no pudo procesar ningún ítem
        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);

        // El stock NO cambia
        Assert.Equal(1, product.Stock);

        // NO se guarda la orden
        _mockOrderRepo.Verify(repo => repo.AddAsync(It.IsAny<Order>()), Times.Never);
    }
}