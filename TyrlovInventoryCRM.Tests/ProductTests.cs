using Xunit;
using TyrlovInventoryCRM.Models;
using TyrlovInventoryCRM.Data;
using System.Linq;

namespace TyrlovInventoryCRM.Tests
{
    public class ProductTests
    {
        [Fact]
        public void CanAddProductToDatabase()
        {
            using var db = new AppDbContext();
            db.Database.EnsureCreated();

            var testProduct = new Product
            {
                Name = "Тестовая видеокарта",
                Category = "Комплектующие",
                Price = 50000m,
                Quantity = 5
            };

            db.Products.Add(testProduct);
            db.SaveChanges();

            var productFromDb = db.Products.FirstOrDefault(p => p.Name == "Тестовая видеокарта");

            Assert.NotNull(productFromDb);
            Assert.Equal(50000m, productFromDb.Price);
            Assert.Equal(5, productFromDb.Quantity);

            db.Products.Remove(productFromDb);
            db.SaveChanges();
        }
    }
}