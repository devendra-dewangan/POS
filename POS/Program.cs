using POS.Data;
using POS.Services.Import;
using POS.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add SQLite database context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=pos.db"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();

// Add ExcelReaderService
builder.Services.AddScoped<ExcelReaderService>();

// Add business services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IPurchaseService, PurchaseService>();
builder.Services.AddScoped<IBatchService, BatchService>();
builder.Services.AddScoped<IBuyerService, BuyerService>();
builder.Services.AddScoped<IImportService, ImportService>();

var app = builder.Build();

app.UseCors("AllowAll");
app.MapControllers();
app.UseHttpsRedirection();
app.Run();

