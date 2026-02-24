using Microsoft.EntityFrameworkCore;
using POS.Models;

namespace POS.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<Purchase> Purchases { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure Product entity
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Price).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.Stock).IsRequired();
            });

            // Configure Sale entity
            modelBuilder.Entity<Sale>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Product)
                      .WithMany(p => p.Sales)
                      .HasForeignKey(e => e.ProductId);
                entity.Property(e => e.Quantity).IsRequired();
                entity.Property(e => e.TotalAmount).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.SaleDate).IsRequired();
            });

            // Configure Purchase entity
            modelBuilder.Entity<Purchase>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Product)
                      .WithMany(p => p.Purchases)
                      .HasForeignKey(e => e.ProductId);
                entity.Property(e => e.Quantity).IsRequired();
                entity.Property(e => e.UnitPrice).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalAmount).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.PurchaseDate).IsRequired();
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}