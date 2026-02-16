using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApi.Models;
using System.Security.Claims;
using TiendaPromElec.DTOs;
using TiendaPromElec.Repositories;

namespace TiendaPromElec.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] // <--- CAPA 1: Solo usuarios autenticados pasan
public class OrderController : ControllerBase
{
    private readonly IOrderRepository _repository;
    private readonly ILogger<OrderController> _logger;
    private readonly IProductRepository _productRepository;
    // Constructor to initialize the context and logger

    public OrderController(IOrderRepository repository, ILogger<OrderController> logger, IProductRepository productRepository)
    {
        _repository = repository;
        _logger = logger;
        _productRepository = productRepository;
    }

    // --- MÉTODO HELPER DE SEGURIDAD ---
    // Extrae el ID del token de forma segura. Si el token fue manipulado, esto falla.
    private long GetCurrentUserId()
    {
        var identity = HttpContext.User.Identity as ClaimsIdentity;
        // "NameIdentifier" es el estándar para guardar el ID en el Token
        var idClaim = identity?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(idClaim) || !long.TryParse(idClaim, out long userId))
        {
            throw new UnauthorizedAccessException("El token no contiene un ID válido.");
        }
        return userId;
    }

    // GET: api/Order
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
    {
        var userId = GetCurrentUserId();

        if (User.IsInRole("Admin"))
        {
            // El Admin ve el historial completo de ventas
            var allOrders = await _repository.GetAllAsync();
            return Ok(allOrders);
        }
        else
        {
            // El Cliente solo ve sus propias compras
            var myOrders = await _repository.GetOrdersByCustomerAsync(userId);
            return Ok(myOrders);
        }
    }

    // GET: api/Order/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Order>> GetOrder(long id)
    {
        var userId = GetCurrentUserId();
        var order = await _repository.GetByIdAsync(id);

        if (order == null)
        {
            _logger.LogWarning("Orden {Id} no encontrada", id);
            return NotFound();
        }

        // SEGURIDAD:
        // Si NO eres Admin Y la orden NO es tuya -> Prohibido
        if (!User.IsInRole("Admin") && order.CustomerId != userId)
        {
            _logger.LogWarning("Alerta de seguridad: Usuario {UserId} intentó acceder orden ajena {OrderId}", userId, id);
            return Forbid();
        }

        return Ok(order);
    }

    // PUT: api/Order/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PutOrder(long id, Order order)
    {
        if (id != order.Id) return BadRequest("El ID no coincide con el cuerpo de la petición");

        var exists = await _repository.ExistsAsync(id);
        if (!exists) return NotFound();

        try
        {
            await _repository.UpdateAsync(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar la orden {Id}", id);
            return StatusCode(500, "Error interno al actualizar la orden");
        }

        return NoContent();
    }

    // POST: api/Order
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Order>> PostOrder(CreateOrderDto dto)
    {
        var userId = GetCurrentUserId();
        // Listas para reportar al usuario qué pasó
        var errores = new List<string>();
        // 1. Crear la orden base
        // ESTRATEGIA: Integridad (Evitamos que un usuario cree órdenes a nombre de otro)
        // Ignoramos cualquier CustomerId que venga en el JSON y usamos el del Token.
        var order = new Order
        {
            OrderDate = DateTime.UtcNow,
            Status = "Pendiente",
            CustomerId = userId,
            Items = new List<OrderDetail>(),
            TotalAmount = 0 
        };

        // 2. Iterar items para buscar precios REALES en base de datos
        foreach (var itemDto in dto.Items)
        {
            // 3. BUSCAMOS EL PRODUCTO REAL EN BD (Seguridad)
            var product = await _productRepository.GetByIdAsync(itemDto.ProductId);
            // CASO A: El producto no existe
            if (product == null)
            {
                errores.Add($"Producto ID {itemDto.ProductId}: No existe. Fue omitido.");
                continue; // Saltamos al siguiente ciclo sin cancelar todo
            }

            // CASO B: Validar Stock 
            if (product.Stock < itemDto.Quantity)
            {
                errores.Add($"Producto {product.Name}: Stock insuficiente (Solicitado: {itemDto.Quantity}, Disponible: {product.Stock}).");
                continue; // Saltamos este producto
            }

            // 4. Si llegamos aquí, todo es válido. DESCONTAMOS STOCK
            product.Stock -= itemDto.Quantity;

            await _productRepository.UpdateAsync(product);

            var detail = new OrderDetail
            {
                ProductId = product.Id,
                Quantity = itemDto.Quantity,
                UnitPrice = product.Price 
            };

            order.Items.Add(detail);
            order.TotalAmount += detail.Quantity * detail.UnitPrice;
        }

        // FINAL: Verificar si quedó algún ítem válido
        if (order.Items.Count == 0)
        {
            return BadRequest(new
            {
                Mensaje = "No se pudo procesar la orden porque ningún producto era válido.",
                Errores = errores
            });
        }

        await _repository.AddAsync(order);
        var resultDto = new OrderResultDto
        {
            Order = order,
            Warnings = errores
        };

        // Retornamos éxito, pero adjuntamos los errores/advertencias para que el frontend sepa
        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, resultDto);
    }

    // DELETE: api/Order/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteOrder(long id)
    {
        var exists = await _repository.ExistsAsync(id);
        if (!exists)
        {
            return NotFound();
        }

        await _repository.DeleteAsync(id);
        return NoContent();
    }
}