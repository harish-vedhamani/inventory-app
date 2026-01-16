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
      Task<(IEnumerable<Product> Items, int TotalCount)> QueryProductsAsync(
          int page,
          int pageSize,
          decimal? minPrice,
          decimal? maxPrice,
          bool? lowStock,
          string? sortBy,
          bool desc,
          string? q);
      Task<Product> GetProductByIdAsync(Guid id);
      Task<IEnumerable<Product>> SearchProductsAsync(string query);
      Task UpdatePriceAsync(Guid id, decimal newPrice);
      Task UpdateQuantityAsync(Guid id, int newQuantity);
      Task DeleteProductAsync(Guid id);
      Task<decimal> GetTotalInventoryValueAsync();
      Task<Product?> GetHighestPricedProductAsync();
    }
}
