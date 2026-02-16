using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApi.Models;
using TiendaPromElec.DTOs;
using TiendaPromElec.Repositories;

namespace TiendaPromElec.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductController : ControllerBase
{
    private readonly IProductRepository _repository;
    private readonly ILogger<ProductController> _logger;

    public ProductController(IProductRepository repository, ILogger<ProductController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    // GET: api/Product
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
    {
        return Ok(await _repository.GetAllAsync());
    }

    // GET: api/Product/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProduct(long id)
    {
        // Usamos LogInformation para registrar un evento de rutina
        _logger.LogInformation("Iniciando búsqueda del producto con ID: {ProductoId}", id);

        var product = await _repository.GetByIdAsync(id);

        if (product == null)
        {
            // Usamos LogWarning para registrar un evento de advertencia
            _logger.LogWarning("Producto con ID {ProductoId} no encontrado", id);
            return NotFound();
        }
        _logger.LogInformation("Producto con ID: {ProductoId} encontrado exitosamente.", id);
        return Ok(product);
    }

    // PUT: api/Product/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PutProduct(long id, UpdateProductDto dto)
    {
        // Verificamos si existe antes de intentar actualizar
        var exists = await _repository.ExistsAsync(id);
        if (!exists) return NotFound();

        var product = await _repository.GetByIdAsync(id);
        product.Name = dto.Name;
        product.Description = dto.Description;
        product.Brand = dto.Brand;
        product.Price = dto.Price;
        product.Stock = dto.Stock;
        product.ImageUrl = dto.ImageUrl;
        product.CategoryId = dto.CategoryId;

        try
        {
            await _repository.UpdateAsync(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error actualizando producto {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }

        return NoContent();
    }

    // POST: api/Product
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Product>> PostProduct(CreateProductDto dto)
    {
        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Brand = dto.Brand,
            Price = dto.Price,
            Stock = dto.Stock,
            ImageUrl = dto.ImageUrl,
            CategoryId = dto.CategoryId
        };
        await _repository.AddAsync(product);

        return CreatedAtAction("GetProduct", new { id = product.Id }, product);
    }

    // DELETE: api/Product/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteProduct(long id)
    {
        var exists = await _repository.ExistsAsync(id);
        if (!exists) return NotFound();

        await _repository.DeleteAsync(id);
        return NoContent();
    }
}