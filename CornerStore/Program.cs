using CornerStore.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using CornerStore.Models.DTOs;

#nullable enable


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// allows passing datetimes without time zone data 
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// allows our api endpoints to access the database through Entity Framework Core and provides dummy value for testing
builder.Services.AddNpgsql<CornerStoreDbContext>(builder.Configuration["CornerStoreDbConnectionString"] ?? "testing");

// Set the JSON serializer options
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//endpoints go here

app.MapPost("/cashiers", (CornerStoreDbContext db, Cashier cashier) => {
    db.Cashiers.Add(cashier);
    db.SaveChanges();
    
    return Results.Created($"/cashiers/{cashier.Id}", cashier);

});

app.MapGet("/cashiers/{id}", (CornerStoreDbContext db, int id) => {
    Cashier foundCashier = db.Cashiers 
                .Include(c => c.Orders)
                .ThenInclude(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
                .FirstOrDefault(c => c.Id == id);

        if (foundCashier == null)
        {
            return Results.NotFound();
        }

    
    CashierDTO cashier = new CashierDTO
    {
        Id = foundCashier.Id,
        FirstName = foundCashier.FirstName,
        LastName = foundCashier.LastName,
        Orders = foundCashier.Orders != null ? foundCashier.Orders
        .Select(o => new OrderDTO
        {
            Id = o.Id,
            CashierId = o.CashierId,
            PaidOnDate = o.PaidOnDate != null ? o.PaidOnDate : null,
            OrderProducts = o.OrderProducts != null ? o.OrderProducts
            .Select(op => new OrderProductDTO
            {
                ProductId = op.ProductId,
                Product = new ProductDTO
                {
                    Id = op.Product.Id,
                    ProductName = op.Product.ProductName,
                    Price = op.Product.Price,
                    Brand = op.Product.Brand,
                    CategoryId = op.Product.CategoryId
                },
                OrderId = op.OrderId,
                Quantity = op.Quantity

            }
            ).ToList():null
        }
        ).ToList(): null
    };

    return Results.Ok(cashier);

});


app.MapGet("/products", (CornerStoreDbContext db, string? productName, string? categoryName  ) => {
   var query = db.Products.Include(p => p.Category).AsQueryable();

    if (!string.IsNullOrEmpty(productName))
    {
      query = query.Where(p => p.ProductName.ToLower() == productName.ToLower());
    }

    if (!string.IsNullOrEmpty(categoryName))
    {
         query = query.Where(p => p.Category.CategoryName.ToLower() == categoryName.ToLower());
    }

    var products = query.Select( p => new ProductDTO
    {
        Id = p.Id,
        ProductName = p.ProductName,
        Price = p.Price,
        Brand = p.Brand,
        CategoryId = p.CategoryId,
        Category = new CategoryDTO
        {
            Id = p.Category.Id,
            CategoryName = p.Category.CategoryName
        }
    }
    ).ToList();
    
    
    return Results.Ok(products);

});


app.MapPost("/products", (CornerStoreDbContext db, Product product) => {
    
    db.Products.Add(product);
    db.SaveChanges();

    return Results.Created($"/products/{product.Id}", product);

});


app.MapPut("/products/{id}", (CornerStoreDbContext db, int id, Product updatedProduct) => {
      if (updatedProduct == null)
    {
        return Results.BadRequest("Product data is required.");
    }
    
    
    Product foundProduct = db.Products.FirstOrDefault(p => p.Id == id);

    if (foundProduct == null)
    {
        return Results.NotFound();
    }

    foundProduct.ProductName = updatedProduct.ProductName;
    foundProduct.Price = updatedProduct.Price;
    foundProduct.Brand = updatedProduct.Brand;
    foundProduct.CategoryId = updatedProduct.CategoryId;

    db.SaveChanges();

    return Results.NoContent();

});


app.MapGet("/orders/{id}", (CornerStoreDbContext db, int id) => {
    Order foundOrder = db.Orders
        .Include(o => o.Cashier)
        .Include(o=> o.OrderProducts)
        .ThenInclude(op => op.Product)
        .ThenInclude(p => p.Category)
        .FirstOrDefault(o => o.Id == id);

        
        if (foundOrder == null)
        {
            return Results.NotFound();
        }

        
        OrderDTO order = new OrderDTO
        {
            Id = foundOrder.Id,
            CashierId = foundOrder.CashierId,
            Cashier = new CashierDTO
            {
                Id = foundOrder.Cashier.Id,
                FirstName = foundOrder.Cashier.FirstName,
                LastName = foundOrder.Cashier.LastName
            },
            PaidOnDate = foundOrder.PaidOnDate ?? null,
            OrderProducts = foundOrder.OrderProducts != null ? foundOrder.OrderProducts
                    .Select(op => new OrderProductDTO
                    {
                        OrderId = op.OrderId,
                        Product = new ProductDTO
                        {
                            Id = op.Product.Id,
                            ProductName = op.Product.ProductName,
                            Price = op.Product.Price,
                            Brand = op.Product.Brand,
                            CategoryId = op.Product.CategoryId,
                            Category = new CategoryDTO
                            {
                                Id = op.Product.Category.Id,
                                CategoryName = op.Product.Category.CategoryName
                            }
                        },
                        Quantity = op.Quantity

                    }).ToList():null
        };

        return Results.Ok(order);
});

app.MapGet("/orders", (CornerStoreDbContext db, DateTime? paidOnDate) => {
  
  var query = db.Orders.AsQueryable();
    
    if (paidOnDate.HasValue)
    {
        query = query.Where(o => o.PaidOnDate.HasValue && o.PaidOnDate.Value.Date == paidOnDate.Value.Date);
    } 

    var finalOrders = query.Select(o => new OrderDTO
    {
        Id = o.Id,
        CashierId = o.CashierId,
        PaidOnDate = o.PaidOnDate,
    
    }).ToList();

    return Results.Ok(finalOrders);

});

app.MapDelete("/orders/{id}", (CornerStoreDbContext db, int id) => {
    Order foundOrder = db.Orders.FirstOrDefault(o => o.Id == id);

    if (foundOrder == null)
    {
        return Results.NotFound();
    }

    db.Orders.Remove(foundOrder);
    db.SaveChanges();

    return Results.NoContent();
});

app.MapPost("/orders", (CornerStoreDbContext db, Order order) => {
    order.PaidOnDate = DateTime.Now;
    
    db.Orders.Add(order);
  
   
   foreach (OrderProduct op in order.OrderProducts)
   {
        op.OrderId = order.Id;
        db.OrderProducts.Add(op);
    }
    
    db.SaveChanges();


     // Reload the order with related data
    var savedOrder = db.Orders
        .Where(o => o.Id == order.Id)
        .Include(o => o.OrderProducts)
        .ThenInclude(op => op.Product)
        .FirstOrDefault();

    var newOrder = new OrderDTO
    {
        Id = savedOrder.Id,
        CashierId = savedOrder.CashierId,
        PaidOnDate = savedOrder.PaidOnDate,
        OrderProducts = savedOrder.OrderProducts.Select(op => new OrderProductDTO
        {
            ProductId = op.Product.Id,
            Product = new ProductDTO
            {
                Id = op.Product.Id,
                ProductName = op.Product.ProductName,
                Price = op.Product.Price,
                Brand = op.Product.Brand,
                CategoryId = op.Product.CategoryId
            },
            OrderId = order.Id,
            Quantity = op.Quantity,
        }
        ).ToList()

    };

    return Results.Created($"/orders/{order.Id}", newOrder);
});

app.Run();

//don't move or change this!
public partial class Program { }