using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using RefactorThis.Models;

namespace RefactorThis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        [HttpGet]
        public Products Get()
        {
            return new Products(null);
        }

        [HttpGet("test")]
        public Products GetTest()
        {
            return new Products("test");
        }

        [HttpGet("{id}")]
        public Product Get(int id) {

            var guid = new Guid(BitConverter.GetBytes(id));
            var product = new Product(guid);
            if (product.IsNew)
                throw new Exception();

            return product;
        }

        [HttpPost]
        public IActionResult Post(Product product)
        {
            if (product.IsNew) 
            {
                product.Save();
                return Accepted();
            }
            else {
                return NotFound();
            }
        }

        [HttpPut("{id}")]
        public void Update(Guid id, Product product)
        {
            var orig = new Product(id)
            {
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                DeliveryPrice = product.DeliveryPrice
            };

            if (!orig.IsNew)
                orig.Save();
        }

        [HttpDelete("{id}")]
        public void Delete(Guid id) {
            // Ignore the seed data
            if (id == new Guid("3d81192d-773d-4f09-866b-ab48f9674396")) return;
            var product = new Product(id);
            product.Delete();
        }

        [HttpGet("{productId}/options")]
        public ProductOptions GetOptions(Guid productId)
        {
            return new ProductOptions(productId);
        }

        [HttpGet("{productId}/options/{id}")]
        public ProductOption GetOption(Guid productId, Guid id)
        {
            var option = new ProductOption(id);

            if (productId != option.Id || option.IsNew)
                throw new Exception();

            return option;
        }

        [HttpPost("{productId}/options")]
        public void CreateOption(Guid productId, ProductOption option)
        {
            option.ProductId = productId;
            option.Save();
        }

        [HttpPut("{productId}/options/{id}")]
        public void UpdateOption(Guid id, ProductOption option)
        {
            var orig = new ProductOption(id)
            {
                Id = id,
                Name = option.Name,
                Description = option.Name
            };

            if (!orig.IsNew)
                orig.Save();
        }

        [HttpDelete("{productId}/options/{id}")]
        public void DeleteOption(Guid productId, Guid id)
        {
            var opt = new ProductOption(productId);
            opt.Delete();
        }
    }
}