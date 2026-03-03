using Moq;
using POS.Data;
using POS.Models;
using POS.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace POS.UnitTests.Mocks
{
    public static class MockFactory
    {
        #region DbContext Mocks

        public static Mock<AppDbContext> CreateMockContext()
        {
            var dbOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            return new Mock<AppDbContext>(dbOptions);
        }

        public static Mock<DbSet<T>> CreateMockDbSet<T>(IQueryable<T> data) where T : class
        {
            var mockSet = new Mock<DbSet<T>>();
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            
            return mockSet;
        }

        #endregion

        #region Service Mocks

        public static Mock<IBatchService> CreateMockBatchService()
        {
            return new Mock<IBatchService>();
        }

        public static Mock<IBuyerService> CreateMockBuyerService()
        {
            return new Mock<IBuyerService>();
        }

        public static Mock<ISupplierService> CreateMockSupplierService()
        {
            return new Mock<ISupplierService>();
        }

        public static Mock<IPurchaseService> CreateMockPurchaseService()
        {
            return new Mock<IPurchaseService>();
        }

        public static Mock<IImportService> CreateMockImportService()
        {
            return new Mock<IImportService>();
        }

        #endregion

        #region Data Setup Helpers

        public static List<Product> CreateSampleProducts()
        {
            return new List<Product>
            {
                new Product { Id = 1, ProductName = "Product 1", ProductCode = "P001", Barcode = "1111111111" },
                new Product { Id = 2, ProductName = "Product 2", ProductCode = "P002", Barcode = "2222222222" },
                new Product { Id = 3, ProductName = "Product 3", ProductCode = "P003", Barcode = "3333333333" }
            };
        }

        public static List<Batch> CreateSampleBatches()
        {
            return new List<Batch>
            {
                new Batch { Id = 1, ProductId = 1, PurchaseId = 1, Stock = 100, PurchaseStock = 100, PurchaseRate = 10.00m, MRP = 15.00m, SaleRate = 12.00m },
                new Batch { Id = 2, ProductId = 2, PurchaseId = 2, Stock = 50, PurchaseStock = 50, PurchaseRate = 20.00m, MRP = 25.00m, SaleRate = 22.00m },
                new Batch { Id = 3, ProductId = 3, PurchaseId = 3, Stock = 75, PurchaseStock = 75, PurchaseRate = 5.00m, MRP = 8.00m, SaleRate = 6.00m }
            };
        }

        public static List<Buyer> CreateSampleBuyers()
        {
            return new List<Buyer>
            {
                new Buyer { Id = 1, Name = "Buyer 1" },
                new Buyer { Id = 2, Name = "Buyer 2" },
                new Buyer { Id = 3, Name = "Buyer 3" }
            };
        }

        public static List<Supplier> CreateSampleSuppliers()
        {
            return new List<Supplier>
            {
                new Supplier { Id = 1, Name = "Supplier 1" },
                new Supplier { Id = 2, Name = "Supplier 2" },
                new Supplier { Id = 3, Name = "Supplier 3" }
            };
        }

        #endregion
    }
}