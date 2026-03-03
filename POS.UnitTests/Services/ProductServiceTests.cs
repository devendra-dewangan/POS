using Moq;
using POS.Data;
using POS.Models;
using POS.Services;
using POS.UnitTests.Builders;
using POS.UnitTests.Mocks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace POS.UnitTests.Services
{
    public class ProductServiceTests
    {
        private readonly Mock<IBatchService> _mockBatchService;
        private readonly AppDbContext _context;
        private readonly ProductService _productService;

        public ProductServiceTests()
        {
            // Setup in-memory database
            var dbOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDatabase_{System.Guid.NewGuid()}")
                .Options;

            _context = new AppDbContext(dbOptions);
            _mockBatchService = POS.UnitTests.Mocks.MockFactory.CreateMockBatchService();
            _productService = new ProductService(_context, _mockBatchService.Object);
        }

        #region AddProductAsync Tests

        [Fact]
        public async Task AddProductAsync_WhenGivenValidProductDetails_ShouldAddProductToDatabase()
        {
            // Arrange
            var productName = "Test Product";
            var productCode = "TP001";
            var barcode = "1234567890";

            // Act
            var result = await _productService.AddProductAsync(productName, productCode, barcode);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(productName, result.ProductName);
            Assert.Equal(productCode, result.ProductCode);
            Assert.Equal(barcode, result.Barcode);
            
            var savedProduct = await _context.Products.FindAsync(result.Id);
            Assert.NotNull(savedProduct);
            Assert.Equal(productName, savedProduct.ProductName);
            Assert.Equal(productCode, savedProduct.ProductCode);
            Assert.Equal(barcode, savedProduct.Barcode);
        }

        #endregion

        #region GetOrCreateProductAsync Tests

        [Fact]
        public async Task GetOrCreateProductAsync_WhenProductExists_ShouldReturnExistingProduct()
        {
            // Arrange
            var existingProduct = ProductBuilder.Create()
                .WithProductName("Existing Product")
                .WithProductCode("EP001")
                .WithBarcode("0987654321")
                .Build();
            
            _context.Products.Add(existingProduct);
            await _context.SaveChangesAsync();

            // Act
            var result = await _productService.GetOrCreateProductAsync(existingProduct.ProductName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(existingProduct.ProductName, result.ProductName);
            Assert.Equal(existingProduct.ProductCode, result.ProductCode);
            Assert.Equal(existingProduct.Barcode, result.Barcode);
            
            var productCount = await _context.Products.CountAsync();
            Assert.Equal(1, productCount);
        }

        [Fact]
        public async Task GetOrCreateProductAsync_WhenProductDoesNotExist_ShouldCreateNewProduct()
        {
            // Arrange
            var productName = "New Product";
            var productCode = "NP001";
            var barcode = "1122334455";

            // Act
            var result = await _productService.GetOrCreateProductAsync(productName, productCode, barcode);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(productName, result.ProductName);
            Assert.Equal(productCode, result.ProductCode);
            Assert.Equal(barcode, result.Barcode);
            
            var savedProduct = await _context.Products.FindAsync(result.Id);
            Assert.NotNull(savedProduct);
            Assert.Equal(productName, savedProduct.ProductName);
            Assert.Equal(productCode, savedProduct.ProductCode);
            Assert.Equal(barcode, savedProduct.Barcode);
        }

        #endregion

        #region GetProductByNameAsync Tests

        [Fact]
        public async Task GetProductByNameAsync_WhenProductExists_ShouldReturnProduct()
        {
            // Arrange
            var testProduct = ProductBuilder.Create()
                .WithProductName("Test Product")
                .WithProductCode("TP001")
                .WithBarcode("1234567890")
                .Build();
            
            _context.Products.Add(testProduct);
            await _context.SaveChangesAsync();

            // Act
            var result = await _productService.GetProductByNameAsync(testProduct.ProductName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(testProduct.ProductName, result.ProductName);
        }

        [Fact]
        public async Task GetProductByNameAsync_WhenProductDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var productName = "Non-Existent Product";

            // Act
            var result = await _productService.GetProductByNameAsync(productName);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region GetProductByBarcodeAsync Tests

        [Fact]
        public async Task GetProductByBarcodeAsync_WhenBarcodeExists_ShouldReturnProduct()
        {
            // Arrange
            var barcode = "1234567890";
            var testProduct = ProductBuilder.Create()
                .WithProductName("Test Product")
                .WithProductCode("TP001")
                .WithBarcode(barcode)
                .Build();
            
            _context.Products.Add(testProduct);
            await _context.SaveChangesAsync();

            // Act
            var result = await _productService.GetProductByBarcodeAsync(barcode);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(testProduct.Barcode, result.Barcode);
        }

        [Fact]
        public async Task GetProductByBarcodeAsync_WhenBarcodeDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var barcode = "0000000000";

            // Act
            var result = await _productService.GetProductByBarcodeAsync(barcode);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetProductByBarcodeAsync_WhenBarcodeIsNullOrEmpty_ShouldReturnNull()
        {
            // Arrange
            // No need to setup products as method should return early

            // Act
            var result1 = await _productService.GetProductByBarcodeAsync(null);
            var result2 = await _productService.GetProductByBarcodeAsync(string.Empty);

            // Assert
            Assert.Null(result1);
            Assert.Null(result2);
        }

        #endregion

        #region GetAllProductsAsync Tests

        [Fact]
        public async Task GetAllProductsAsync_WhenProductsExist_ShouldReturnAllProducts()
        {
            // Arrange
            var products = POS.UnitTests.Mocks.MockFactory.CreateSampleProducts();
            _context.Products.AddRange(products);
            await _context.SaveChangesAsync();

            // Act
            var result = await _productService.GetAllProductsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IEnumerable<Product>>(result);
            Assert.Equal(3, result.Count());
        }

        [Fact]
        public async Task GetAllProductsAsync_WhenNoProductsExist_ShouldReturnEmptyList()
        {
            // Arrange
            // No products in database

            // Act
            var result = await _productService.GetAllProductsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IEnumerable<Product>>(result);
            Assert.Empty(result);
        }

        #endregion
    }
}
