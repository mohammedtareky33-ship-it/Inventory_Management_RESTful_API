using InventoryShared;
using Microsoft.Data.SqlClient;

namespace InventoryDAL.Interfaces
{
    public interface IBatchDAL
    {
        Task<bool> ActivateBatchAndDeactivateOtherForProduct(int batchId);
        Task<int> Add(BatchAddDTO batch, SqlConnection con, SqlTransaction tran);
        Task<bool> DeactivateAllBatchesForProductId(int productId);
        Task<bool> DeactivateBatch(int batchId);
        Task<BatchSimpleReadDTO> getActiveBatchOfProduct(int productId);
        Task<AllBatchesReadDTO> getAllBatchesOfProduct(int productId, int pageNum, int pageSize);
        Task<BatchReadDTO> getBatch(int batchId);
        Task<BatchReadDTO> getBatchForBatchAndProductId(int batchId, int productId);
        Task<BatchSimpleReadDTO> getBatchSimpleInfo(int batchId, SqlConnection con, SqlTransaction tran);
        Task<bool> isBatchActive(int batchId, SqlConnection con, SqlTransaction tran);
        Task<bool> UpdateQuantity(BatchUpdateQuantityDTO batch, SqlConnection con, SqlTransaction tran);
        Task<bool> UpdateSalePriceAndNotes(BatchUpdateSalePriceAndNotesDTO batch);
    }
}