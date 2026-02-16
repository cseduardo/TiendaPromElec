using Microsoft.EntityFrameworkCore;

namespace ProductApi.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de relaciones (opcional si se usan atributos, pero es buena práctica)

        // 1. CREACIÓN DE CATEGORÍAS
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Smartphones" },
            new Category { Id = 2, Name = "Laptops" },
            new Category { Id = 3, Name = "Accesorios" }
        );

        // 2. CREACIÓN DE PRODUCTOS
        modelBuilder.Entity<Product>().HasData(
            // Smartphones
            new Product { Id = 1, Name = "Galaxy S25", Description = "El último smartphone de Samsung", Brand = "Samsung", Price = 24999.99m, Stock = 50, ImageUrl = "https://via.placeholder.com/300", CategoryId = 1 },
            new Product { Id = 2, Name = "iPhone 16 Pro", Description = "El flagship de Apple", Brand = "Apple", Price = 28999.50m, Stock = 40, ImageUrl = "https://via.placeholder.com/300", CategoryId = 1 },
            new Product { Id = 3, Name = "Pixel 9", Description = "La experiencia pura de Android", Brand = "Google", Price = 21500.00m, Stock = 30, ImageUrl = "https://via.placeholder.com/300", CategoryId = 1 },
            // Laptops
            new Product { Id = 4, Name = "MacBook Air M4", Description = "Potencia y portabilidad", Brand = "Apple", Price = 32500.00m, Stock = 25, ImageUrl = "https://via.placeholder.com/300", CategoryId = 2 },
            new Product { Id = 5, Name = "Dell XPS 15", Description = "La mejor laptop para creadores", Brand = "Dell", Price = 38000.75m, Stock = 20, ImageUrl = "https://via.placeholder.com/300", CategoryId = 2 },
            new Product { Id = 6, Name = "ThinkPad X1 Carbon", Description = "El estándar para negocios", Brand = "Lenovo", Price = 41200.00m, Stock = 15, ImageUrl = "https://via.placeholder.com/300", CategoryId = 2 },
            // Accesorios
            new Product { Id = 7, Name = "AirPods Pro 3", Description = "Cancelación de ruido superior", Brand = "Apple", Price = 5500.00m, Stock = 100, ImageUrl = "https://via.placeholder.com/300", CategoryId = 3 },
            new Product { Id = 8, Name = "Samsung Galaxy Buds 4", Description = "Audio de alta fidelidad", Brand = "Samsung", Price = 3800.00m, Stock = 120, ImageUrl = "https://via.placeholder.com/300", CategoryId = 3 },
            new Product { Id = 9, Name = "Cargador Anker 100W", Description = "Carga rápida para todos tus dispositivos", Brand = "Anker", Price = 1200.00m, Stock = 200, ImageUrl = "https://via.placeholder.com/300", CategoryId = 3 },
            new Product { Id = 10, Name = "Mouse Logitech MX Master 4", Description = "El mouse definitivo para productividad", Brand = "Logitech", Price = 2400.00m, Stock = 80, ImageUrl = "https://via.placeholder.com/300", CategoryId = 3 }
        );

        // 3. CREACIÓN DE CLIENTES
        modelBuilder.Entity<Customer>().HasData(
            new Customer { Id = 1, FullName = "Juan Pérez", Email = "juan.perez@email.com", Phone = "5512345678", Address = "Calle Falsa 123, CDMX" },
            new Customer { Id = 2, FullName = "Ana García", Email = "ana.garcia@email.com", Phone = "8187654321", Address = "Av. Siempre Viva 742, Monterrey" }
        );

        // 4. CREACIÓN DE OrderS
        modelBuilder.Entity<Order>().HasData(
            new Order { Id = 1, OrderDate = new DateTime(2025, 6, 10), TotalAmount = 28999.50m, Status = "Entregado", CustomerId = 1 },
            new Order { Id = 2, OrderDate = new DateTime(2025, 6, 11), TotalAmount = 43700.75m, Status = "Enviado", CustomerId = 2 },
            new Order { Id = 3, OrderDate = new DateTime(2025, 6, 12), TotalAmount = 5500.00m, Status = "Enviado", CustomerId = 1 },
            new Order { Id = 4, OrderDate = new DateTime(2025, 6, 13), TotalAmount = 45999.98m, Status = "Pendiente", CustomerId = 2 },
            new Order { Id = 5, OrderDate = new DateTime(2025, 6, 14), TotalAmount = 2400.00m, Status = "Pendiente", CustomerId = 1 }
        );

        // 5. CREACIÓN DE DETALLES DE Order
        modelBuilder.Entity<OrderDetail>().HasData(
            // Order 1
            new OrderDetail { Id = 1, Quantity = 1, UnitPrice = 28999.50m, ProductId = 2, OrderId = 1 },
            // Order 2
            new OrderDetail { Id = 2, Quantity = 1, UnitPrice = 38000.75m, ProductId = 5, OrderId = 2 },
            new OrderDetail { Id = 3, Quantity = 1, UnitPrice = 5700.00m, ProductId = 7, OrderId = 2 }, // Precio modificado a propósito
            // Order 3
            new OrderDetail { Id = 4, Quantity = 1, UnitPrice = 5500.00m, ProductId = 7, OrderId = 3 },
            // Order 4
            new OrderDetail { Id = 5, Quantity = 2, UnitPrice = 22999.99m, ProductId = 1, OrderId = 4 }, // 2 unidades
            // Order 5
            new OrderDetail { Id = 6, Quantity = 1, UnitPrice = 2400.00m, ProductId = 10, OrderId = 5 }
        );


            // Relación Categoría -> Product (Uno a Muchos)
            modelBuilder.Entity<Category>()
                .HasMany(c => c.Products)
                .WithOne(p => p.Category)
                .HasForeignKey(p => p.CategoryId);

            // Relación Customer -> Order (Uno a Muchos)
            modelBuilder.Entity<Customer>()
                .HasMany(c => c.Orders)
                .WithOne(o => o.Customer)
                .HasForeignKey(o => o.CustomerId);

            // Relación Order -> OrderDetail (Uno a Muchos)
            modelBuilder.Entity<Order>()
                .HasMany(o => o.Items)
                .WithOne(od => od.Order)
                .HasForeignKey(od => od.OrderId);
        }
    }
}