using InventoryShared;
using Microsoft.Data.SqlClient;

namespace InventoryBL.Interfaces
{
    public interface IBatchServiceForInvoice
    {
        Task<int> Add(BatchAddDTO batch, SqlConnection con, SqlTransaction tran);
        Task<BatchSimpleReadDTO> getBatchSimpleInfo(int batchId, SqlConnection con, SqlTransaction tran);
        Task<bool> UpdateQuantityForInvoices(BatchUpdateQuantityDTO batch, ResonsOfStockMovementsClass.enReasonOfMovement Reason, SqlConnection con, SqlTransaction tran);

    }
}