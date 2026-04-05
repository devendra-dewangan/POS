using POS.Data;
using POS.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;
using LiteDB;
using POS.Repos;
using POS.Middleware;
using Microsoft.AspNetCore.Identity;
using POS.Entity;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Host.UseSerilog();


builder.Services.AddSingleton(
    new LiteDatabase(builder.Configuration
    .GetSection("LiteDb:ConnectionString")
    .Value!));

// Add SQLite database context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "JwtBearer";
    options.DefaultChallengeScheme = "JwtBearer";
})
.AddJwtBearer("JwtBearer", options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes("THIS_IS_SUPER_SECRET_KEY_1234567890"))
    };
});


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

builder.Services.AddControllers();
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
builder.Services.AddScoped<IIdentityService, IdentityService>();

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

app.UseAuthentication();
app.UseAuthorization();

app.UseWebSockets();
app.UseMiddleware<WebSocketHandlerMiddleware>();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
app.Run();

