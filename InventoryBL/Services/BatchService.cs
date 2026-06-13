using InventoryShared;
using Microsoft.Data.SqlClient;
using static InventoryShared.ResonsOfStockMovementsClass;

using InventoryBL.Interfaces;
using InventoryDAL.Repos;
using InventoryDAL.Interfaces;
using System.ComponentModel.DataAnnotations;
namespace InventoryBL.Services
{
    public class BatchService : IBatchService, IBatchServiceForInvoice
    {

        IBatchDAL _batchesDataAccess;
        IStockMovementDAL _stockMovementDataAccess;
        IProductService _productService;
        public BatchService(IBatchDAL batchDAL, IStockMovementDAL stockMovementDAL, IProductService productService)
        {
            _productService = productService;
            _batchesDataAccess = batchDAL;
            _stockMovementDataAccess = stockMovementDAL;
        }

        public async Task<int> Add(BatchAddDTO batch, SqlConnection con, SqlTransaction tran)
        {
            int batchid = -1;
            int stockMovementId = -1;
            if (batch.Quantity <= 0)
            {
                throw new ValidationException("Quantity Must be above than 0");
            }

            batch.LastUpdatedUserId = batch.CreatedByUserId;
            batch.AddDateTime = DateTime.Now;
            batch.LastUpdateDateTime = batch.AddDateTime;
            batchid = await _batchesDataAccess.Add(batch, con, tran);
            if (batchid < 1)
                throw new Exception("failed Added batch");
            stockMovementId = await _stockMovementDataAccess.Add(new StockMovementAddDTO(batchid, true, batch.Quantity, batch.AddDateTime, (int)enReasonOfMovement.Purchase, batch.CreatedByUserId), con, tran);
            return batchid;
        }
        void UpdateQuantityValidation(decimal amount)
        {

            if (amount <= 0)
            {
                throw new ValidationException("amount must be above than 0 ");
            }

        }


        async Task<bool> UpdateQuantity(BatchUpdateQuantityDTO batch, enReasonOfMovement Reason, SqlConnection con, SqlTransaction tran)
        {
            bool isUpdated = false;
            bool isIn = true;
            decimal amount = batch.Amount;
            try
            {
                UpdateQuantityValidation(batch.Amount);
                if (batch.BatchId < 1)
                    throw new ValidationException("id must be greater than 0");
                if (Reason == enReasonOfMovement.Purchase)
                    throw new ValidationException("this Type not Updated quantity its for add only");
                else if (Reason == enReasonOfMovement.ReturnPurchase || Reason == enReasonOfMovement.Sale || Reason == enReasonOfMovement.DisposalStock)
                {
                    batch.Amount = -batch.Amount;
                    isIn = false;
                }
                isUpdated = await _batchesDataAccess.UpdateQuantity(batch, con, tran);
                int movementId = await _stockMovementDataAccess.Add(new StockMovementAddDTO(batch.BatchId, isIn, amount, batch.LastUpdateDateTime,
                    (byte)Reason, batch.LastUpdatedUserId), con, tran);
                if (!isUpdated || movementId < 0)
                    throw new Exception("Added is Failed");
                return isUpdated;
            }
            catch (Exception ex) {
                
                throw; }
        }
        public async Task<bool> DisposalStock(BatchUpdateQuantityDTO batch)
        {
            if (batch.BatchId < 1)
                throw new ValidationException("id must be greater than 0");

            bool isUpdated = false;
            try
            {

                using (SqlConnection con = new SqlConnection(DataAccessSettings.ConnectionString))
                {
                    await con.OpenAsync();
                    using (SqlTransaction tran = con.BeginTransaction())
                    {
                        try
                        {
                            isUpdated = await UpdateQuantity(batch, enReasonOfMovement.DisposalStock, con, tran);
                            if (!isUpdated)
                            {
                                throw new NotFoundException("Batch Not Found");
                            }
                            tran.Commit();
                        }
                        catch
                        {
                            tran.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex) { throw; }
            return isUpdated;
        }
        public async Task<bool> UpdateQuantityForInvoices(BatchUpdateQuantityDTO batch, enReasonOfMovement Reason, SqlConnection con, SqlTransaction tran)
        {
            if (batch.BatchId < 1)
                throw new ValidationException("id must be greater than 0");
            if (Reason == enReasonOfMovement.DisposalStock)
                throw new ValidationException("its not invoice operation");
            return await UpdateQuantity(batch, Reason, con, tran);
        }
        public async Task<bool> UpdateSalePriceAndNotes(BatchUpdateSalePriceAndNotesDTO batch)
        {
            if (batch.BatchId < 1)
                throw new ValidationException("id must be greater than 0");
            if (await _batchesDataAccess.UpdateSalePriceAndNotes(batch))
                return true;
            else throw new NotFoundException("Batch not Found");
        }
        public async Task<bool> ActivateBatchAndDeactivateOtherForProduct(int batchId)
        {
            if (batchId < 1)
                throw new ValidationException("id must be greater than 0");

            return await _batchesDataAccess.ActivateBatchAndDeactivateOtherForProduct(batchId)?true:throw new NotFoundException("not found batch");
        }
        public async Task<bool> DeactivateAllBatchesForProductId(int productId)
        {
            if (productId < 1)
                throw new ValidationException("id must be greater than 0");
            if ( await _batchesDataAccess.DeactivateAllBatchesForProductId(productId))
                return true;
             else throw new NotFoundException("Batch not Found");
        }
        public async Task<BatchSimpleReadDTO> getActiveBatchOfProduct(int productId)
        {
            if (productId < 1)
                throw new ValidationException("id must be greater than 0");

            BatchSimpleReadDTO batch = await _batchesDataAccess.getActiveBatchOfProduct(productId);
            if (batch == null)
                throw new NotFoundException("this product dosent have any active batches");
            return batch;
        }
        public async Task<bool> DeactivateBatch(int batchId)
        {
            if (batchId < 1)
                throw new ValidationException("id must be greater than 0");

            if ( await _batchesDataAccess.DeactivateBatch(batchId))
                return true;
            else throw new NotFoundException("Batch not Found");

        }
        public async Task<BatchSimpleReadDTO> getBatchSimpleInfo(int batchId, SqlConnection con, SqlTransaction tran)
        {
            if (batchId < 1)
                throw new ValidationException("id must be greater than 0");
            BatchSimpleReadDTO batch = await _batchesDataAccess.getBatchSimpleInfo(batchId, con, tran);
            if (batch == null)
                throw new NotFoundException("this batch isn't found");
            return batch;
        }
        public async Task<BatchReadDTO> getBatch(int batchId)
        {
            if (batchId < 1)
                throw new ValidationException("id must be greater than 0");
            BatchReadDTO batch = await _batchesDataAccess.getBatch(batchId);
            if (batch == null)
                throw new NotFoundException("this batch isn't found");
            return batch;
        }

        public async Task<BatchReadDTO> getBatchForBatchAndProductId(int batchId, int productId)
        {
            if (batchId < 1)
                throw new ValidationException("id must be greater than 0");

            BatchReadDTO batch = await _batchesDataAccess.getBatchForBatchAndProductId(batchId, productId);
            if (batch == null)
                throw new NotFoundException("this batch isn't found");
            return batch;
        }
        public async Task<AllBatchesReadDTO> getAllBatchesOfProduct(int productId, int pageNum, int pageSize)
        {

            if (productId < 1)
                throw new ValidationException("id must be greater than 0");

            if (!await _productService.isProductExist(productId))
                throw new NotFoundException("this product isnt existed");
            return await _batchesDataAccess.getAllBatchesOfProduct(productId, pageNum, pageSize);


        }
      public async  Task<AllStockMovementsReadDTO> getAllMovementsForBatchIdFiltered(int batchId, StockMovementsFilter filter)
        {


            return await _stockMovementDataAccess.getAllMovementsForBatchIdFiltered(batchId,filter);
        }

    }
}
