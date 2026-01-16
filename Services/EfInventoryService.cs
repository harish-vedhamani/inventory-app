using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Playground.Data;
using Playground.Domain;
using Playground.Entities;
using Playground.Exceptions;

namespace Playground.Services
{
    public class EfInventoryService : IInventoryService
    {
        private readonly ApplicationDbContext _db;

        public EfInventoryService(ApplicationDbContext db)
        {
            _db = db;
        }

        private static Product ToDomain(ProductEntity e) => new Product(e.Id, e.Name, e.Price, e.Quantity);
        private static ProductEntity ToEntity(Product p) => new ProductEntity { Id = p.Id, Name = p.Name, Price = p.Price, Quantity = p.Quantity };

        public async Task AddProductAsync(Product product)
        {
            if (product == null) throw new ArgumentNullException(nameof(product));

            var exists = await _db.Products.AnyAsync(p => p.Name == product.Name);
            if (exists) throw new ArgumentException($"A product with name '{product.Name}' already exists.");

            var entity = ToEntity(product);
            _db.Products.Add(entity);
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<Product>> ListProductsAsync()
        {
            var list = await _db.Products.AsNoTracking().ToListAsync();
            return list.Select(ToDomain);
        }

        public async Task<Product> GetProductByIdAsync(Guid id)
        {
            var e = await _db.Products.FindAsync(id);
            if (e == null) throw new ProductNotFoundException(id);
            return ToDomain(e);
        }

        public async Task<IEnumerable<Product>> SearchProductsAsync(string query)
        {
            if (query is null) query = string.Empty;
            var q = query.Trim();
            var results = await _db.Products
                .Where(p => EF.Functions.Like(p.Name, $"%{q}%"))
                .AsNoTracking()
                .ToListAsync();
            return results.Select(ToDomain);
        }

        public async Task UpdatePriceAsync(Guid id, decimal newPrice)
        {
            if (newPrice < 0) throw new ArgumentOutOfRangeException(nameof(newPrice));
            var e = await _db.Products.FindAsync(id);
            if (e == null) throw new ProductNotFoundException(id);
            e.Price = newPrice;
            await _db.SaveChangesAsync();
        }

        public async Task UpdateQuantityAsync(Guid id, int newQuantity)
        {
            if (newQuantity < 0) throw new ArgumentOutOfRangeException(nameof(newQuantity));
            var e = await _db.Products.FindAsync(id);
            if (e == null) throw new ProductNotFoundException(id);
            e.Quantity = newQuantity;
            await _db.SaveChangesAsync();
        }

        public async Task DeleteProductAsync(Guid id)
        {
            var e = await _db.Products.FindAsync(id);
            if (e == null) throw new ProductNotFoundException(id);
            _db.Products.Remove(e);
            await _db.SaveChangesAsync();
        }

        public async Task<decimal> GetTotalInventoryValueAsync()
        {
            var total = await _db.Products.SumAsync(p => p.Price * p.Quantity);
            return total;
        }

        public async Task<Product?> GetHighestPricedProductAsync()
        {
            var e = await _db.Products.OrderByDescending(p => p.Price).FirstOrDefaultAsync();
            return e is null ? null : ToDomain(e);
        }

        public async Task<(IEnumerable<Product> Items, int TotalCount)> QueryProductsAsync(
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

            var query = _db.Products.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var tq = q.Trim();
                query = query.Where(p => EF.Functions.Like(p.Name, $"%{tq}%"));
            }

            if (minPrice.HasValue) query = query.Where(p => p.Price >= minPrice.Value);
            if (maxPrice.HasValue) query = query.Where(p => p.Price <= maxPrice.Value);
            if (lowStock.HasValue && lowStock.Value) query = query.Where(p => p.Quantity <= 5);

            var total = await query.CountAsync();

            // Sorting
            query = (sortBy ?? "name").ToLower() switch
            {
                "price" => desc ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
                _ => desc ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            };

            var skip = (page - 1) * pageSize;
            var pageItems = await query.Skip(skip).Take(pageSize).ToListAsync();

            return (pageItems.Select(ToDomain), total);
        }
    }
}
