using POS.Models;

namespace POS.UnitTests.Builders
{
    public class ProductBuilder
    {
        private int _id = 1;
        private string _productName = "Test Product";
        private string _productCode = "TP001";
        private string _barcode = "1234567890";

        public ProductBuilder WithId(int id)
        {
            _id = id;
            return this;
        }

        public ProductBuilder WithProductName(string productName)
        {
            _productName = productName;
            return this;
        }

        public ProductBuilder WithProductCode(string productCode)
        {
            _productCode = productCode;
            return this;
        }

        public ProductBuilder WithBarcode(string barcode)
        {
            _barcode = barcode;
            return this;
        }

        public Product Build()
        {
            return new Product
            {
                Id = _id,
                ProductName = _productName,
                ProductCode = _productCode,
                Barcode = _barcode
            };
        }

        public static ProductBuilder Create()
        {
            return new ProductBuilder();
        }
    }
}