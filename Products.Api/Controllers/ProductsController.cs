using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Product.Api.Infrastructure;

namespace Product.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ProductDbContext _context;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(
            ProductDbContext context,
            ILogger<ProductsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Models.Product>>> GetProducts([FromQuery] IEnumerable<Guid>? productUIds, [FromQuery]IEnumerable<int>? productIds)
        {
            _logger.LogInformation("Getting products");

            if (productUIds is not null)
            {
                return await _context
                    .Products
                    .Where(x => productUIds.Contains(x.UId))
                    .ToListAsync();
            }

            if (productIds is not null)
            {
                return Ok(await _context
                    .Products
                    .Where(x => productIds.Contains(x.Id))
                    .ToListAsync());
            }

            // This is just an example - Should not get all products from the database!
            return Ok(await _context
                    .Products
                    .ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Models.Product>> GetProduct(int id)
        {
            _logger.LogInformation("Getting product with {id}", id);

            var product = await _context.Products.FindAsync(id);
            return product == null ? NotFound() : Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult<Models.Product>> CreateProduct(Models.Product product)
        {
            _logger.LogInformation("Creating Product");

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, Models.Product product)
        {
            _logger.LogInformation("Updating product with {id}", id);

            if (id != product.Id)
            {
                return BadRequest();
            }

            _context.Entry(product).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            _logger.LogInformation("Deleting product with {id}", id);

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}