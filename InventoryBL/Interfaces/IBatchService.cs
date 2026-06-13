using InventoryShared;

namespace InventoryBL.Interfaces
{
    public interface IBatchService
    {
        Task<bool> ActivateBatchAndDeactivateOtherForProduct(int batchId);
        Task<bool> DeactivateAllBatchesForProductId(int productId);
        Task<bool> DeactivateBatch(int batchId);
        Task<bool> DisposalStock(BatchUpdateQuantityDTO batch);
        Task<BatchSimpleReadDTO> getActiveBatchOfProduct(int productId);
        Task<AllBatchesReadDTO> getAllBatchesOfProduct(int productId, int pageNum, int pageSize);
        Task<AllStockMovementsReadDTO> getAllMovementsForBatchIdFiltered(int batchId, StockMovementsFilter filter);
        Task<BatchReadDTO> getBatch(int batchId);
        Task<BatchReadDTO> getBatchForBatchAndProductId(int batchId, int productId);
        Task<bool> UpdateSalePriceAndNotes(BatchUpdateSalePriceAndNotesDTO batch);
    }
}