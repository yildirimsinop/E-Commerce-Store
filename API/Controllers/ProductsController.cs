using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly StoreContext context;
        public ProductsController(StoreContext context)
        {
            this.context = context;

        }


        [HttpGet]
        public ActionResult<List<Product>> GetProducts()

        {
            var products = context.Products.ToList();

            return Ok(products);
        }

        [HttpGet("{id}")]

        public ActionResult

    }
}