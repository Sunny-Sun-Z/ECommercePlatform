using Catalog.Data;
using Catalog.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Catalog.Services;
namespace Catalog.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CatalogController : ControllerBase
    {
        private readonly CatalogContext _context;
        private readonly IRabbitMqPublisher _publisher;
        public CatalogController(CatalogContext context, IRabbitMqPublisher publisher)
        {
            _context = context;
            _publisher = publisher;
        }

        // GET api/catalog
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _context.Products.ToListAsync();
            return Ok(products);
        }

        // GET api/catalog/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            return Ok(product);
        }

        // POST api/catalog
        [HttpPost]
        public async Task<IActionResult> Create(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // publish the event
            _publisher.PublishProductCreated(product);

            return CreatedAtAction(nameof(GetById),
                                   new { id = product.Id },
                                   product);
        }
    }
}