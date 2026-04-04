using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using POS.Models;

namespace POS.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleItem> SaleItems { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<Batch> Batches { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Buyer> Buyers { get; set; }
        public DbSet<ImportInfo> ImportInfos { get; set; }
        public DbSet<ImportPurchaseTemp> ImportPurchaseTemp { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure Product entity
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ProductName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ProductCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Barcode).HasMaxLength(100);
            });

            // Configure Sale entity
            modelBuilder.Entity<Sale>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.InvoiceNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.InvoiceDate).IsRequired();
                entity.Property(e => e.TotalAmount).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.SaleDate).IsRequired();
                entity.Property(e => e.BuyerId).IsRequired();
                entity.HasOne(e => e.Buyer)
                      .WithMany(b => b.Sales)
                      .HasForeignKey(e => e.BuyerId);
            });

            // Configure SaleItem entity
            modelBuilder.Entity<SaleItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Sale)
                      .WithMany(s => s.SaleItems)
                      .HasForeignKey(e => e.SaleId);
                entity.HasOne(e => e.Batch)
                      .WithMany()
                      .HasForeignKey(e => e.BatchId);
                entity.Property(e => e.Quantity).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.UnitPrice).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalAmount).IsRequired().HasColumnType("decimal(18,2)");
            });

            // Configure Purchase entity
            modelBuilder.Entity<Purchase>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Supplier)
                      .WithMany(s => s.Purchases)
                      .HasForeignKey(e => e.SupplierId);
                entity.Property(e => e.InvoiceNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PurchaseDate).IsRequired();
            });

            // Configure Supplier entity
            modelBuilder.Entity<Supplier>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            });

            // Configure Batch entity
            modelBuilder.Entity<Batch>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Product)
                      .WithMany(p => p.Batches)
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.SetNull); // Allow null ProductId for empty batches
                entity.HasOne(e => e.Purchase)
                      .WithMany(p => p.PurchaseItems)
                      .HasForeignKey(e => e.PurchaseId)
                      .OnDelete(DeleteBehavior.SetNull); // Allow null PurchaseId
                entity.Property(e => e.Stock).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.PurchaseStock).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.PurchaseRate).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.MRP).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.SaleRate).IsRequired().HasColumnType("decimal(18,2)");
            });

            // Configure Buyer entity
            modelBuilder.Entity<Buyer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            });

            // Configure ImportInfo entity
            modelBuilder.Entity<ImportInfo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.TotalRecords).IsRequired();
                entity.Property(e => e.Status).IsRequired();
                entity.Property(e => e.ImportType).IsRequired();
                entity.Property(e => e.ImportDate).IsRequired();
            });

            // Configure ImportPurchaseTemp entity
            modelBuilder.Entity<ImportPurchaseTemp>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                // Configure relationship with ImportInfo
                entity.HasOne(e => e.ImportInfo)
                      .WithMany(i => i.ImportPurchaseTemps)
                      .HasForeignKey(e => e.ImportId)
                      .OnDelete(DeleteBehavior.Cascade); // Cascade delete when ImportInfo is deleted
            });

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User)
                      .WithMany(u => u.RefreshTokens)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.Property(e => e.TokenHash).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Expires).IsRequired();
                entity.Property(e => e.Created).IsRequired();
                entity.Property(e => e.CreatedByIp).IsRequired().HasMaxLength(45);
                entity.Property(e => e.Revoked).IsRequired(false);
                entity.Property(e => e.RevokedByIp).IsRequired(false).HasMaxLength(45);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}