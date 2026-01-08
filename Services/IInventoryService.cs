using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Playground.Domain;

namespace Playground.Services
{
    public interface IInventoryService
    {
      Task AddProductAsync(Product product);
      Task<IEnumerable<Product>> ListProductsAsync();
      Task<Product> GetProductByIdAsync(Guid id);
      Task<IEnumerable<Product>> SearchProductsAsync(string query);
      Task UpdatePriceAsync(Guid id, decimal newPrice);
      Task UpdateQuantityAsync(Guid id, int newQuantity);
      Task DeleteProductAsync(Guid id);
      Task<decimal> GetTotalInventoryValueAsync();
      Task<Product?> GetHighestPricedProductAsync();
    }
}
