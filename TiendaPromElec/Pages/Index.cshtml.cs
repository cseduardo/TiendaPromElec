using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.Models;

namespace TiendaPromElec.Pages
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Product> Product { get; set; }

        public async Task OnGetAsync()
        {
            Product = await _context.Products.Include(p => p.Category).ToListAsync();
        }
    }
}