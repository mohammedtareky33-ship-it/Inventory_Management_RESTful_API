using InventoryBL.Interfaces;
using InventoryDAL.Interfaces;
using InventoryDAL.Repos;
using InventoryShared;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryBL.Services
{
    public class ProductService : IProductService
    {
        IProductDAL _ProductsDataAccess ;
        public ProductService(IProductDAL productDAL) {
        
        _ProductsDataAccess = productDAL;
        }
        bool IsValidToAdd(ProductAddDTO product)
        {
            return product.ProductName.Length > 3;
        }

        public async Task<int> Add(ProductAddDTO product)
        {
            if (!IsValidToAdd(product))
                throw new ValidationException("this Product didnt valid To Add");
      
            return await _ProductsDataAccess.Add(product);
        }
        public async Task<ProductReadDTO> getProduct(int productId)
        {
            if (productId < 1)
                throw new ValidationException("id must be greater than 0");
            var product= await _ProductsDataAccess.getProduct(productId);
            return product==null?throw new NotFoundException("Product isnt found"):product;
        }
        public async Task<ProductReadDTO> getProduct(string productName)
        {

            var product = await _ProductsDataAccess.getProduct(productName);
            return product==null ? throw new NotFoundException("Product isnt found") : product;
        }
        public async Task<AllProductReadDTO> getAllProducts(int pageNum, int pageSize)
        {

            return await _ProductsDataAccess.getAllProducts(pageNum, pageSize);
        }
        public async Task<bool> Update(ProductUpdateDTO product)
        {
            if (product.ProductId < 1)
                throw new ValidationException("id must be greater than 0");
            if (await _ProductsDataAccess.Update(product))
            { return true; }
            else throw new NotFoundException("Product isnt found");
        }
        public async Task<bool> Delete(int productId)
        {
            if (productId < 1)
                throw new ValidationException("id must be greater than 0");
            return await _ProductsDataAccess.Delete(productId)?true :  throw new NotFoundException("Product isnt found") ;
        }
        public async Task<bool> isProductExist(string productName)
        {
            return await _ProductsDataAccess.isProductExist(productName)?true :  throw new NotFoundException("Product isnt found");
        }
        public async Task<bool> isProductExist(int productId)
        {
            if (productId < 1)
                throw new ValidationException("id must be greater than 0");
            return await _ProductsDataAccess.isProductExist(productId) ? true : throw new NotFoundException("Product isnt found");
        }
        public async Task<bool> isProductExist(int productId, SqlConnection con, SqlTransaction tran)
        {
            return await _ProductsDataAccess.isProductExist(productId, con, tran);
        }
    }
}
