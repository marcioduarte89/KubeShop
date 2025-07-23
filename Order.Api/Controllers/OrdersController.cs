using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Trace;
using Order.Api.Extensions;
using Order.Api.Infrastructure;
using Order.Api.Services;
using System.Diagnostics;

namespace Order.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly OrderDbContext _context;
        private readonly IProductService _productService;
        private readonly Tracer _tracer;
        private readonly ILogger<OrdersController> _logger;
        private const string GET_ORDER = "GetOrder";

        public OrdersController(
            OrderDbContext context, 
            IProductService productService,
            Tracer tracer,
            ILogger<OrdersController> logger)
        {
            _context = context;
            _productService = productService;
            _tracer = tracer;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Models.Ouput.Order>>> GetOrders(CancellationToken cancellationToken)
        => Ok((await _context.Orders.Include(o => o.Items).ToListAsync(cancellationToken)).Select(x => x.ToOutputOrder()));

        [HttpGet("{id}", Name= GET_ORDER)]
        public async Task<ActionResult<Models.Ouput.Order>> GetOrder(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting order {id}", id);
            var order = await _context.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
            return order == null ? NotFound() : Ok(order.ToOutputOrder());
        }

        [HttpPost]
        public async Task<ActionResult<Models.Ouput.Order>> CreateOrder(Models.Input.Order order, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating orders");

            var productUIds = order.Items.Select(x => x.ProductUId).ToList();

            Console.WriteLine($"Before calling the products");

            var products = await _productService.GetProducts(productUIds, cancellationToken);

            using var span = _tracer.StartActiveSpan("DoWorkMethod");
            span.SetAttribute("operation.value", 1);
            span.AddEvent("Prior to calling the method");
            await DoWorkMethod();
            span.AddEvent("After calling the method");

            Console.WriteLine($"Managed to deserialize it");

            if (products is null)
            {
                _logger.LogInformation("Products not found");
                return NotFound("Products provided were not found");
            }

            // TODO:
            // - Check product ids exist
            // - Check stock is <= to the requested amount

            var domainOrder = order.ToModelOrder();
            _context.Orders.Add(domainOrder);
            await _context.SaveChangesAsync();

            Console.WriteLine($"Saved products");

            return CreatedAtAction(GET_ORDER, new { id = domainOrder.Id }, domainOrder.ToOutputOrder());
        }

        private async Task DoWorkMethod()
        {
            await Task.Delay(2000);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, Models.Input.Order order)
        {
            _logger.LogInformation("Updating order {id}", id);

            if (id != order.Id)
            {
                return BadRequest();
            }

            _context.Entry(order).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
