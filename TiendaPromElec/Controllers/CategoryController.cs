using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.Models;

namespace TiendaPromElec.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoryController : ControllerBase
{
    private readonly AppDbContext _context;

    public CategoryController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Category
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
    {
        return await _context.Categories.ToListAsync();
    }

    // POST: api/Category
    [HttpPost]
    [Authorize(Roles = "Admin")] // Solo Admin
    public async Task<ActionResult<Category>> PostCategory(Category category)
    {
        // Validar que no exista el nombre duplicado
        bool existe = await _context.Categories
        .AnyAsync(c => c.Name.ToLower() == category.Name.ToLower());
        if (existe)
        {
            return BadRequest("Ya existe una categoría con ese nombre.");
        }

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetCategories", new { id = category.Id }, category);
    }

    // DELETE: api/Category/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")] // Solo Admin
    public async Task<IActionResult> DeleteCategory(long id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return NotFound("Categoría no encontrada");

        // VALIDACIÓN IMPORTANTE: No borrar si se tiene productos
        var hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id);
        if (hasProducts)
        {
            return BadRequest("No se puede eliminar: Hay productos asociados a esta categoría.");
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}