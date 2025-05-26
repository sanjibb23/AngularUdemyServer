using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApiQuespond.Models;

namespace WebApiQuespond.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private static List<Product> products = new List<Product>
        {
            new Product { Id = 1, Name = "Item 1" },
            new Product { Id = 2, Name = "Item 2" }
        };

        [HttpGet]
        public IActionResult Get()
        {
            throw new Exception();
            return Ok(products);
        }

        [HttpPost]
        public IActionResult Post([FromBody] Product newProduct)
        {
            newProduct.Id = products.Count + 1;
            products.Add(newProduct);
            return CreatedAtAction(nameof(Get), new { id = newProduct.Id }, newProduct);
        }
    }
}
