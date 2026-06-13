using InventoryShared;
using Microsoft.Data.SqlClient;

namespace InventoryBL.Interfaces
{
    public interface IProductService
    {
        Task<int> Add(ProductAddDTO product);
        Task<bool> Delete(int productId);
        Task<AllProductReadDTO> getAllProducts(int pageNum, int pageSize);
        Task<ProductReadDTO> getProduct(int productId);
        Task<ProductReadDTO> getProduct(string productName);
        Task<bool> isProductExist(int productId);
      
        Task<bool> isProductExist(string productName);
        Task<bool> Update(ProductUpdateDTO product);
    }
}