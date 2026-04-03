using POS.Data;
using POS.Services.Import;
using POS.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;
using LiteDB;
using POS.Repos;
using POS.Middleware;

var builder = WebApplication.CreateBuilder(args);
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Host.UseSerilog();

builder.Services.AddControllers();

// Add SQLite database context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

builder.Services.AddSingleton(
    new LiteDatabase(builder.Configuration
    .GetSection("LiteDb:ConnectionString")
    .Value!));

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
builder.Services.AddSingleton<ImportQueue>();
builder.Services.AddHostedService<ImportWorker>();

// Add business services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IPurchaseService, PurchaseService>();
builder.Services.AddScoped<IBatchService, BatchService>();
builder.Services.AddScoped<IBuyerService, BuyerService>();
builder.Services.AddScoped<IImportService, ImportService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<LiteDbContext>();
builder.Services.AddScoped<ILiteStore, LiteStore>();

builder.Services.AddSingleton<IConnectionManager, ConnectionManager>();


if (builder.Environment.IsDevelopment())
{
    builder.Services.AddHostedService<DevSystemMetricsLogger>();
}

var app = builder.Build();

if (args.Contains("migrate"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    Log.Logger.Information("Database migration completed.");
    return;
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseWebSockets();
app.UseMiddleware<WebSocketHandlerMiddleware>();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
app.Run();

