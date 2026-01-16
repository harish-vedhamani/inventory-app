using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Playground.Domain;
using Playground.Exceptions;

namespace Playground.Services
{
    public class PlainInventoryService : IInventoryService
    {
        private readonly List<Product> _products = new();

        public Task AddProductAsync(Product product)
        {
            if (product is null) throw new ArgumentNullException(nameof(product));

            if (_products.Any(p => string.Equals(p.Name, product.Name, StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException($"A product with name '{product.Name}' already exists.");

            _products.Add(product);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<Product>> ListProductsAsync()
        {
            var snapshot = _products.ToList();
            return Task.FromResult<IEnumerable<Product>>(snapshot);
        }

        public Task<IEnumerable<Product>> SearchProductsAsync(string query)
        {
            if (query is null) query = string.Empty;
            var q = query.Trim();
            var results = _products
                .Where(p => p.Name.Contains(q, StringComparison.OrdinalIgnoreCase))
                .ToList();
            return Task.FromResult<IEnumerable<Product>>(results);
        }

        public Task<Product> GetProductByIdAsync(Guid id)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product is null) throw new Exceptions.ProductNotFoundException(id);
            return Task.FromResult(product);
        }

        public Task UpdatePriceAsync(Guid id, decimal newPrice)
        {
            if (newPrice < 0) throw new ArgumentOutOfRangeException(nameof(newPrice), "Price cannot be negative");

            var idx = _products.FindIndex(p => p.Id == id);
            if (idx < 0) throw new ProductNotFoundException(id);
            var existing = _products[idx];
            _products[idx] = existing.WithPrice(newPrice);
            return Task.CompletedTask;
        }

        public Task UpdateQuantityAsync(Guid id, int newQuantity)
        {
            if (newQuantity < 0) throw new ArgumentOutOfRangeException(nameof(newQuantity), "Quantity cannot be negative");

            var idx = _products.FindIndex(p => p.Id == id);
            if (idx < 0) throw new ProductNotFoundException(id);
            var existing = _products[idx];
            _products[idx] = existing.WithQuantity(newQuantity);
            return Task.CompletedTask;
        }

        public Task DeleteProductAsync(Guid id)
        {
            var removed = _products.RemoveAll(p => p.Id == id);
            if (removed == 0) throw new ProductNotFoundException(id);
            return Task.CompletedTask;
        }

        public Task<decimal> GetTotalInventoryValueAsync()
        {
            var total = _products.Sum(p => p.Price * p.Quantity);
            return Task.FromResult(total);
        }

        public Task<Product?> GetHighestPricedProductAsync()
        {
            Product? result = null;
            if (_products.Any()) result = _products.OrderByDescending(p => p.Price).First();
            return Task.FromResult(result);
        }

        public Task<(IEnumerable<Product> Items, int TotalCount)> QueryProductsAsync(
            int page,
            int pageSize,
            decimal? minPrice,
            decimal? maxPrice,
            bool? lowStock,
            string? sortBy,
            bool desc,
            string? q)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var query = _products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var tq = q.Trim();
                query = query.Where(p => p.Name.Contains(tq, StringComparison.OrdinalIgnoreCase));
            }

            if (minPrice.HasValue) query = query.Where(p => p.Price >= minPrice.Value);
            if (maxPrice.HasValue) query = query.Where(p => p.Price <= maxPrice.Value);
            if (lowStock.HasValue && lowStock.Value) query = query.Where(p => p.Quantity <= 5);

            var total = query.Count();

            query = (sortBy ?? "name").ToLower() switch
            {
                "price" => desc ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
                _ => desc ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            };

            var skip = (page - 1) * pageSize;
            var items = query.Skip(skip).Take(pageSize).ToList();
            return Task.FromResult<(IEnumerable<Product>, int)>((items, total));
        }
    }
}
