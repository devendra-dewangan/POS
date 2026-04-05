using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using POS.Entity;

namespace POS.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<PurchaseItem> PurchaseItems { get; set; }

        public DbSet<Buyer> Buyers { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleItem> SaleItems { get; set; }
        
        public DbSet<Batch> Batches { get; set; }
        public DbSet<SaleBatch> SaleBatches { get; set; }

        public DbSet<ImportInfo> ImportInfos { get; set; }
        
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
                entity.Property(e => e.Quantity).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.SaleRate).IsRequired().HasColumnType("decimal(18,2)");
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

            modelBuilder.Entity<PurchaseItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Purchase)
                      .WithMany(p => p.PurchaseItems)
                      .HasForeignKey(e => e.PurchaseId);
                entity.HasOne(e => e.Product)
                      .WithMany(p => p.PurchaseItems)
                      .HasForeignKey(e => e.ProductId);
                entity.Property(e => e.Quantity).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.PurchaseRate).IsRequired().HasColumnType("decimal(18,2)");
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
                entity.HasOne(e => e.PurchaseItem)
                      .WithMany(p => p.Batches)
                      .HasForeignKey(e => e.PurchaseItemId)
                      .OnDelete(DeleteBehavior.SetNull); // Allow null PurchaseItemId
                entity.Property(e => e.RemainingStock).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.OpeningStock).IsRequired().HasColumnType("decimal(18,2)");
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

            modelBuilder.Entity<SaleBatch>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.SaleItem)
                      .WithMany(s => s.SaleBatches)
                      .HasForeignKey(e => e.SaleItemId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Batch)
                      .WithMany(b => b.SaleBatches)
                      .HasForeignKey(e => e.BatchId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.Property(e => e.QuantityTaken).IsRequired().HasColumnType("decimal(18,2)");
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