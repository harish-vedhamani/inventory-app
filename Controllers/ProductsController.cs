using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Playground.Services;
using Microsoft.AspNetCore.Authorization;
using Playground.DTOs;
using Playground.Domain;

namespace Playground.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    
    public class ProductsController : ControllerBase
    {
        private readonly IInventoryService _inventory;

        public ProductsController(IInventoryService inventory)
        {
            _inventory = inventory;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] bool? lowStock = null,
            [FromQuery] string? sortBy = "name",
            [FromQuery] bool desc = false,
            [FromQuery] string? q = null)
        {
            var (items, total) = await _inventory.QueryProductsAsync(page, pageSize, minPrice, maxPrice, lowStock, sortBy, desc, q);
            var dto = items.Select(p => new ProductResponseDto { Id = p.Id, Name = p.Name, Price = p.Price, Quantity = p.Quantity });
            return Ok(new { items = dto, totalCount = total, page, pageSize });
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string? q)
        {
            var items = await _inventory.SearchProductsAsync(q ?? string.Empty);
            var dto = items.Select(p => new ProductResponseDto { Id = p.Id, Name = p.Name, Price = p.Price, Quantity = p.Quantity });
            return Ok(dto);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var p = await _inventory.GetProductByIdAsync(id);
                var dto = new ProductResponseDto { Id = p.Id, Name = p.Name, Price = p.Price, Quantity = p.Quantity };
                return Ok(dto);
            }
            catch (Playground.Exceptions.ProductNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] ProductCreateDto create)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var product = new Product(Guid.NewGuid(), create.Name, create.Price, create.Quantity);
            try
            {
                await _inventory.AddProductAsync(product);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }

            var dto = new ProductResponseDto { Id = product.Id, Name = product.Name, Price = product.Price, Quantity = product.Quantity };
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, dto);
        }

        [HttpPut("{id:guid}/price")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdatePrice(Guid id, [FromBody] UpdatePriceDto body)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            try
            {
                await _inventory.UpdatePriceAsync(id, body.Price);
                return NoContent();
            }
            catch (Playground.Exceptions.ProductNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPut("{id:guid}/quantity")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateQuantity(Guid id, [FromBody] UpdateQuantityDto body)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            try
            {
                await _inventory.UpdateQuantityAsync(id, body.Quantity);
                return NoContent();
            }
            catch (Playground.Exceptions.ProductNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _inventory.DeleteProductAsync(id);
                return NoContent();
            }
            catch (Playground.Exceptions.ProductNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
