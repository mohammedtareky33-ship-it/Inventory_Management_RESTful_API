using InventoryShared;
using Microsoft.Data.SqlClient;

namespace InventoryDAL.Interfaces
{
    public interface IStockMovementDAL
    {
        Task<int> Add(StockMovementAddDTO stockMovement, SqlConnection con, SqlTransaction tran);
        Task<AllStockMovementsReadDTO> getAllMovementsForBatchIdFiltered(int batchId, StockMovementsFilter filter);
    }
}