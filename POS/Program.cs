using POS.Data;
using POS.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add SQLite database context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=pos.db"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
    SeedData(context);
}

app.Run();

void SeedData(AppDbContext context)
{
    if (!context.Products.Any())
    {
        var products = new[]
        {
            new Product { Name = "Laptop", Price = 50000, Stock = 10 },
            new Product { Name = "Mouse", Price = 500, Stock = 50 },
            new Product { Name = "Keyboard", Price = 1000, Stock = 30 }
        };
        
        context.Products.AddRange(products);
        context.SaveChanges();
    }
}
