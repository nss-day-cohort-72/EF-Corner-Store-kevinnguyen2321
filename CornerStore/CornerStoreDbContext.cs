using Microsoft.EntityFrameworkCore;
using CornerStore.Models;
public class CornerStoreDbContext : DbContext
{
    public DbSet<Cashier> Cashiers { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<OrderProduct> OrderProducts { get; set; }

    public CornerStoreDbContext(DbContextOptions<CornerStoreDbContext> context) : base(context)
    {

    }

    //allows us to configure the schema when migrating as well as seed data
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
         modelBuilder.Entity<OrderProduct>()
        .HasKey(op => new { op.OrderId, op.ProductId }); // Composite primary key
       
        modelBuilder.Entity<Cashier>().HasData(new Cashier[]
        {
            new Cashier { Id = 1, FirstName = "Alice", LastName = "Smith" },
            new Cashier { Id = 2, FirstName = "Bob", LastName = "Johnson" },
            new Cashier { Id = 3, FirstName = "Carlos", LastName = "Martinez" },
            new Cashier { Id = 4, FirstName = "Diana", LastName = "Brown" },
            new Cashier { Id = 5, FirstName = "Evelyn", LastName = "Garcia" },
        });


        modelBuilder.Entity<Category>().HasData(new Category[]
        {
            new Category { Id = 1, CategoryName = "Beverages" },
            new Category { Id = 2, CategoryName = "Snacks" },
            new Category { Id = 3, CategoryName = "Household Supplies" },
            new Category { Id = 4, CategoryName = "Personal Care" }
        });

        modelBuilder.Entity<Product>().HasData(new Product[]
        {
            new Product { Id = 1, ProductName = "Cola", Price = 1.49m, Brand = "Coca-Cola", CategoryId = 1 },
            new Product { Id = 2, ProductName = "Chips", Price = 2.99m, Brand = "Lay's", CategoryId = 2 },
            new Product { Id = 3, ProductName = "Dish Soap", Price = 3.79m, Brand = "Dawn", CategoryId = 3 },
            new Product { Id = 4, ProductName = "Toothpaste", Price = 4.29m, Brand = "Colgate", CategoryId = 4 },
            new Product { Id = 5, ProductName = "Orange Juice", Price = 2.99m, Brand = "Tropicana", CategoryId = 1 }
        });

         modelBuilder.Entity<Order>().HasData(new Order[]
        {
            new Order { Id = 1, CashierId = 1, PaidOnDate = DateTime.Now.AddDays(-2) },
            new Order { Id = 2, CashierId = 2, PaidOnDate = DateTime.Now.AddDays(-1) },
            new Order { Id = 3, CashierId = 3, PaidOnDate = null },
            new Order { Id = 4, CashierId = 1, PaidOnDate = DateTime.Now.AddDays(-7) },
            new Order { Id = 5, CashierId = 4, PaidOnDate = DateTime.Now.AddDays(-3) }
        });

        modelBuilder.Entity<OrderProduct>().HasData(new OrderProduct[]
        {
            new OrderProduct { OrderId = 1, ProductId = 1, Quantity = 2 },
            new OrderProduct { OrderId = 1, ProductId = 2, Quantity = 1 },
            new OrderProduct { OrderId = 2, ProductId = 3, Quantity = 1 },
            new OrderProduct { OrderId = 2, ProductId = 5, Quantity = 3 },
            new OrderProduct { OrderId = 3, ProductId = 4, Quantity = 1 },
            new OrderProduct { OrderId = 3, ProductId = 1, Quantity = 4 },
            new OrderProduct { OrderId = 4, ProductId = 2, Quantity = 2 },
            new OrderProduct { OrderId = 4, ProductId = 3, Quantity = 1 },
            new OrderProduct { OrderId = 5, ProductId = 4, Quantity = 2 },
            new OrderProduct { OrderId = 5, ProductId = 5, Quantity = 1 }
        });
    }
}