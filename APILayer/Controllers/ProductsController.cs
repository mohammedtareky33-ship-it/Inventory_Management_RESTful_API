using InventoryBL.Interfaces;
using InventoryShared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace InventoryManagemetRESTFUL_API.Controllers
{


    [Authorize]
    [Route("api/Products")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        IProductService _productService;
        public ProductsController(IProductService productService) {
        
        _productService=productService;
        }
        [Authorize(Policy = "Permissions:Products")]
        [HttpGet(Name = "AllProducts")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AllProductReadDTO>>GetAll(int pageNum,int pageSize)
        {
            return Ok(await _productService.getAllProducts(pageNum, pageSize));
        }
        [Authorize(Policy = "Permissions:Products,Inventory")]
        [HttpGet("{productId}",Name ="byProductId")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType (StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ProductReadDTO>> GetById(int productId)
        {
            
            var product=await _productService.getProduct(productId);
           
            return Ok(product);

        }
        [Authorize(Policy = "Permissions:Products,Inventory")]
        [HttpGet("byProductName", Name = "byProductName")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ProductReadDTO>> GetByName(string productName)
        {
            
            var product = await _productService.getProduct(productName);
          
            return Ok(product);

        }
        [Authorize(Policy = "Permissions:Products")]
        [HttpPost(Name = "CreateProduct")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ProductLightReadDTO>> CreateProduct(ProductAddDTO product)
        {
          
                product.CreatedByUserId=int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                int id=await _productService.Add(product);
                return CreatedAtRoute("byProductId", new {productId=id },new ProductLightReadDTO(id,product.ProductName,product.CreatedByUserId,product.Notes));
                
            
           
     

        }
        [Authorize(Policy = "Permissions:Products")]
        [HttpPatch("{productId}",Name ="UpdateNotes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int productId,ProductUpdateDTO product)
        {


            await _productService.Update(new ProductUpdateDTO(productId, product.Notes));
            return Ok(new { productId, product.Notes });
           
        }
        [Authorize(Policy = "Permissions:Products")]
        [HttpDelete("{productId}",Name ="DeleteProduct")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Delete(int productId)
        {


            await _productService.Delete(productId);
                return NoContent();
           
        }

    }
}
