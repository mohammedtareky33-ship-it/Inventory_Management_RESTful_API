using InventoryDAL.Interfaces;
using InventoryShared;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryDAL.Repos
{
    public class StockMovementDAL : IStockMovementDAL
    {
        public async Task<int> Add(StockMovementAddDTO stockMovement, SqlConnection con, SqlTransaction tran)
        {
            int Id = -1;
            string query = @"INSERT INTO [dbo].[StockMovements]
           ([BatchId]
           ,[IsIn]
           ,[Quantity]
           ,[PurchasePrice]
       
           ,[DateTimeMove]
           ,[Reason]
           ,[CreatedByUserId])
     VALUES(
@BatchId
           ,@IsIn
           ,@Quantity
           ,(select purchasePrice from batches where batchId=@BatchId)
            
           ,@DateTimeMove
           ,@Reason
           ,@CreatedByUserId
);
select SCOPE_IDENTITY();

";
            try
            {
                using (SqlCommand cmd = new SqlCommand(query, con, tran))
                {
                    cmd.Parameters.AddWithValue("@BatchId", stockMovement.BatchId);
                    cmd.Parameters.AddWithValue("@IsIn", stockMovement.IsIn);
                    cmd.Parameters.AddWithValue("@Quantity", stockMovement.Quantity);


                    cmd.Parameters.AddWithValue("@DateTimeMove", stockMovement.DateTimeMove);
                    cmd.Parameters.AddWithValue("@Reason", stockMovement.Reason);
                    cmd.Parameters.AddWithValue("@CreatedByUserId", stockMovement.CreatedByUserId);

                    object result = await cmd.ExecuteScalarAsync();
                    if (result != null && result != DBNull.Value)
                    {
                        Id = Convert.ToInt32(result);
                    }
                    else
                    {
                        throw new Exception("Adding Stock Movement Failed Data Access");
                    }
                }

            }
            catch (Exception ex)
            {
                throw;
            }


            return Id;
        }
        public async Task<AllStockMovementsReadDTO> getAllMovementsForBatchIdFiltered(int batchId, StockMovementsFilter filter)
        {
            string additionalWhereQueries = "", countQuery = "select count(*) from StockMovements s where BatchId=@batchId ";
            List<string> reasonParamsNames = new List<string>();
            (List<StockMovementReadDTO> Movements, int count) allMovements = (new List<StockMovementReadDTO>(), 0);
            string query = @"select s.id,s.BatchId,s.IsIn,s.Quantity,s.PurchasePrice,s.DateTimeMove,s.Reason,s.CreatedByUserId ,u.UserName as createdByUsername
from StockMovements s join Users u on s.CreatedByUserId=u.UserId 
where BatchId=@batchId ";

            if (filter.byUserId != null)
                additionalWhereQueries += " and s.CreatedByUserId=@CreatedByUserId";
            if (filter.ReasonIds != null && filter.ReasonIds.Count > 0)
            {
                for (int i = 1; i <= filter.ReasonIds.Count; i++)
                {
                    reasonParamsNames.Add($"@p{i}");
                }
                additionalWhereQueries += $" and Reason in ({string.Join(",", reasonParamsNames)} )";
            }
            if (filter.TimeInterval!=null)
                additionalWhereQueries += " and DateTimeMove between @start and @end ";

            query += additionalWhereQueries + @" order by s.Id 

offset (@pageNum-1)*@pageSize Rows
fetch next @pageSize rows only; ";
            query += countQuery + additionalWhereQueries;
            using (SqlConnection con = new SqlConnection(DataAccessSettings.ConnectionString))
            {
                await con.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@batchId", batchId);
                    if (filter.byUserId != null)
                        cmd.Parameters.AddWithValue("@CreatedByUserId", filter.byUserId);


                    if (filter.ReasonIds != null &&filter.ReasonIds.Count > 0)
                    {
                        for (int i = 0; i < filter.ReasonIds.Count; i++)
                        {
                            cmd.Parameters.AddWithValue(reasonParamsNames[i], filter.ReasonIds[i]);
                        }
                    }
                    cmd.Parameters.AddWithValue("@pageNum",filter. pageNum);
                    cmd.Parameters.AddWithValue("@pageSize", filter.pageSize);
                    if (filter.TimeInterval!=null)
                    {
                        cmd.Parameters.AddWithValue("@start",filter. TimeInterval.From);
                        cmd.Parameters.AddWithValue("@end", filter.TimeInterval.To);

                    }
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            allMovements.Movements.Add(new StockMovementReadDTO((int)reader["id"],
                (int)reader["BatchId"],
                (bool)reader["IsIn"],
                (decimal)reader["Quantity"],
                (decimal)reader["PurchasePrice"],
                (DateTime)reader["DateTimeMove"],
               (byte)reader["Reason"],
                (int)reader["CreatedByUserId"],
                (string)reader["createdByUsername"]));
                        }
                        if (reader.NextResult())
                        {
                            if (reader.Read())
                            {
                                allMovements.count = (int)reader[0];

                            }

                        }

                    }

                }
                return new(allMovements.Movements,allMovements.count);
            }
        }



    }
}
