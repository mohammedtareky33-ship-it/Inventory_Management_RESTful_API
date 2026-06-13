using InventoryShared;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InventoryDAL.Interfaces;
namespace InventoryDAL.Repos
{
    public class BatchDAL : IBatchDAL
    {
        public async Task<int> Add(BatchAddDTO batch, SqlConnection con, SqlTransaction tran)
        {
            int Id = -1;
            string query = @"INSERT INTO [dbo].[Batches]
           ([productId]
           ,[Quantity]
           ,[PurchasePrice]
           ,[SalePrice]
           ,[CreatedByUserId]
           ,[LastUpdatedUserId]
           ,[AddDateTime]
           ,[LastUpdateDateTime]
           ,[Notes]
           )
     VALUES(

           @productId
           ,@Quantity
           ,@PurchasePrice
           ,@SalePrice
           ,@CreatedByUserId
           ,@LastUpdatedUserId
           ,@AddDateTime
           ,@LastUpdateDateTime
           ,@Notes

); select SCOPE_IDENTITY();
";
            try
            {
                using (SqlCommand cmd = new SqlCommand(query, con, tran))
                {
                    cmd.Parameters.AddWithValue("@productId", batch.ProductId);
                    cmd.Parameters.AddWithValue("@Quantity", batch.Quantity);
                    cmd.Parameters.AddWithValue("@PurchasePrice", batch.PurchasePrice);
                    cmd.Parameters.AddWithValue("@SalePrice",
                        batch.SalePrice ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@CreatedByUserId", batch.CreatedByUserId);
                    cmd.Parameters.AddWithValue("@LastUpdatedUserId", batch.LastUpdatedUserId);

                    cmd.Parameters.AddWithValue("@AddDateTime", batch.AddDateTime);
                    cmd.Parameters.AddWithValue("@LastUpdateDateTime", batch.LastUpdateDateTime);

                    cmd.Parameters.AddWithValue("@Notes",
                        string.IsNullOrEmpty(batch.Notes) ? DBNull.Value : batch.Notes);
                    object result = await cmd.ExecuteScalarAsync();
                    if (result != null && result != DBNull.Value)
                    {
                        Id = Convert.ToInt32(result);
                    }
                }

            }
            catch (Exception ex)
            {
                throw;
            }



            return Id;
        }
        public async Task<bool> UpdateSalePriceAndNotes(BatchUpdateSalePriceAndNotesDTO batch)
        {
            int RowsAffected = 0;
            string query = @"update Batches
set SalePrice=@SalePrice,Notes=@Notes,LastUpdatedUserId=@LastUpdatedUserId,LastUpdateDateTime=GetDate()
where batchId=@batchId";
            try
            {
                using (SqlConnection con = new SqlConnection(DataAccessSettings.ConnectionString))
                {
                    await con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@SalePrice", (object)batch.SalePrice ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Notes", string.IsNullOrEmpty(batch.Notes) ? DBNull.Value : batch.Notes);
                        cmd.Parameters.AddWithValue("@LastUpdatedUserId", batch.LastUpdatedUserId);

                        cmd.Parameters.AddWithValue("@batchId", batch.BatchId);
                        RowsAffected = await cmd.ExecuteNonQueryAsync();



                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return RowsAffected > 0;
        }
        public async Task<bool> UpdateQuantity(BatchUpdateQuantityDTO batch, SqlConnection con, SqlTransaction tran)
        {
            int RowsAffected = 0;
            try
            {
                string query = @"update Batches with (updlock,rowlock)
set
Quantity+=@amount,LastUpdatedUserId=@LastUpdatedUserId,LastUpdateDateTime=@LastUpdateDateTime,IsActive=case when  Quantity+@amount=0  then 0 else isActive end 
where batchId=@batchId and Quantity+@amount>=0";
                using (SqlCommand cmd = new SqlCommand(query, con, tran))
                {
                    cmd.Parameters.AddWithValue("@amount", batch.Amount);
                    cmd.Parameters.AddWithValue("@LastUpdatedUserId", batch.LastUpdatedUserId);
                    cmd.Parameters.AddWithValue("@LastUpdateDateTime", batch.LastUpdateDateTime);
                    cmd.Parameters.AddWithValue("@batchId", batch.BatchId);
                    RowsAffected = await cmd.ExecuteNonQueryAsync();



                }
            }
            catch (Exception ex)
            {

                throw;
            }
            return RowsAffected > 0;
        }

        //        public async Task<bool> ActivateBatchAndDeactivateOtherForProduct(int batchId)
        //        {
        //            int RowsAffected = 0;
        //            string query = @"update Batches
        //set isActive=case
        //when batchId=@batchId then 1
        //else 0
        //end
        //where productId=(select productId from Batches where batchId=@batchId)";

        //            try
        //            {
        //                using (SqlConnection con = new SqlConnection(DataAccessSettings.ConnectionString))
        //                {
        //                    using (SqlCommand cmd = new SqlCommand(query, con))
        //                    {
        //                        cmd.Parameters.AddWithValue("@batchId", batchId);
        //                        RowsAffected=await cmd.ExecuteNonQueryAsync();
        //                    }
        //                }
        //            }
        //            catch(Exception ex) { throw; }
        //            return RowsAffected > 0;
        //        }
        public async Task<bool> ActivateBatchAndDeactivateOtherForProduct(int batchId)
        {
            int RowsAffected = 0;
            string query = @"update Batches
set isActive=case
when batchId=@batchId and SalePrice is not null and  Quantity>0 then 1
else 0
end
where productId=(select productId from Batches where batchId=@batchId) ";

            try
            {
                using (SqlConnection con = new SqlConnection(DataAccessSettings.ConnectionString))
                {
                    await con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@batchId", batchId);
                        RowsAffected = await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex) { throw; }
            return RowsAffected > 0;
        }


        public async Task<bool> DeactivateBatch(int batchId)
        {
            int RowsAffected = 0;
            string query = @"update Batches
                        set isActive=0
                        where batchId=@batchId";
            try
            {
                using (SqlConnection con = new SqlConnection(DataAccessSettings.ConnectionString))
                {
                    await con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@batchId", batchId);
                        RowsAffected = await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex) { throw; }
            return RowsAffected > 0;

        }
        public async Task<bool> DeactivateAllBatchesForProductId(int productId)
        {
            int RowsAffected = 0;
            string query = @"update Batches
                            set isActive=0
                            where productId=@productId and isActive=1";
            try
            {
                using (SqlConnection con = new SqlConnection(DataAccessSettings.ConnectionString))
                {

                    await con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@productId", productId);

                        RowsAffected = await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex) { throw; }
            return RowsAffected > 0;
        }
        public async Task<bool> isBatchActive(int batchId, SqlConnection con, SqlTransaction tran)
        {
            string query = @"select isActive from Batches where batchId=@batchId";
            bool isActive = false;
            try
            {
                using (SqlCommand cmd = new SqlCommand(query, con, tran))
                {
                    cmd.Parameters.AddWithValue("@batchId", batchId);
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (reader.Read())
                            isActive = (bool)reader[0];
                    }
                }

            }
            catch (Exception ex) { throw; }
            return isActive;
        }
        public async Task<BatchSimpleReadDTO> getActiveBatchOfProduct(int productId)
        {
            string query = @"select * from BatchesWithProductsAndUsersName where isActive=1 and productId=@productId";
            BatchSimpleReadDTO batch = null;
            try
            {
                using (SqlConnection con = new SqlConnection(DataAccessSettings.ConnectionString))
                {
                    await con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@productId", productId);
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (reader.Read())
                                batch = new BatchSimpleReadDTO((int)reader["batchId"], (int)reader["productId"], (string)reader["ProductName"], (decimal)reader["Quantity"], (decimal)reader["purchasePrice"],
                                    reader["SalePrice"] == DBNull.Value ? null : (decimal?)reader["SalePrice"],
                                   (bool)reader["isActive"]);

                        }
                    }

                }
            }
            catch (Exception ex) { throw; }
            return batch;
        }
        public async Task<BatchSimpleReadDTO> getBatchSimpleInfo(int batchId, SqlConnection con, SqlTransaction tran)
        {

            string query = @"select * from Batches where batchId=@batchId";
            BatchSimpleReadDTO batch = null;
            try
            {
                using (SqlCommand cmd = new SqlCommand(query, con, tran))
                {
                    cmd.Parameters.AddWithValue("@batchId", batchId);
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (reader.Read())
                            batch = new BatchSimpleReadDTO((int)reader["batchId"], (int)reader["productId"], null, (decimal)reader["Quantity"], (decimal)reader["purchasePrice"],
                                reader["SalePrice"] == DBNull.Value ? null : (decimal?)reader["SalePrice"],
                               (bool)reader["isActive"]);

                    }
                }
            }
            catch (Exception ex) { throw; }
            return batch;
        }
        public async Task<BatchReadDTO> getBatch(int batchId)
        {
            string query = @"select * from BatchesWithProductsAndUsersName where  batchId=@batchId";
            BatchReadDTO batch = null;
            try
            {
                using (SqlConnection con = new SqlConnection(DataAccessSettings.ConnectionString))
                {
                    await con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@batchId", batchId);
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (reader.Read())
                                batch = new BatchReadDTO((int)reader["batchId"], (int)reader["productId"], (string)reader["productName"],
                                    (decimal)reader["Quantity"], (decimal)reader["purchasePrice"],
                                    reader["SalePrice"] == DBNull.Value ? null : (decimal?)reader["SalePrice"], (int)reader["createdByUserId"], (string)reader["createdByUsername"],
                                    (int)reader["LastUpdatedUserId"], (string)reader["LastUpdatedByUsername"],
                                     (DateTime)reader["AddDateTime"], (DateTime)reader["lastUpdateDateTime"], reader["Notes"] == DBNull.Value ? null : (string)reader["Notes"],
                                   (bool)reader["isActive"]);

                        }
                    }

                }
            }
            catch (Exception ex) { throw; }
            return batch;
        }
        public async Task<BatchReadDTO> getBatchForBatchAndProductId(int batchId, int productId)
        {
            string query = @"select * from BatchesWithProductsAndUsersName where  batchId=@batchId and productId=@productId";
            BatchReadDTO batch = null;
            try
            {
                using (SqlConnection con = new SqlConnection(DataAccessSettings.ConnectionString))
                {
                    await con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@batchId", batchId);
                        cmd.Parameters.AddWithValue("@productId", productId);
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (reader.Read())
                                batch = new BatchReadDTO((int)reader["batchId"], (int)reader["productId"], (string)reader["productName"],
                                    (decimal)reader["Quantity"], (decimal)reader["purchasePrice"],
                                    reader["SalePrice"] == DBNull.Value ? null : (decimal?)reader["SalePrice"], (int)reader["createdByUserId"], (string)reader["createdByUsername"],
                                    (int)reader["LastUpdatedUserId"], (string)reader["LastUpdatedByUsername"],
                                     (DateTime)reader["AddDateTime"], (DateTime)reader["lastUpdateDateTime"], reader["Notes"] == DBNull.Value ? null : (string)reader["Notes"],
                                   (bool)reader["isActive"]);

                        }
                    }

                }
            }
            catch (Exception ex) { throw; }
            return batch;
        }

        public async Task<AllBatchesReadDTO> getAllBatchesOfProduct(int productId, int pageNum, int pageSize)
        {
            string query = @"select * from BatchesWithProductsAndUsersName where  productId=@productId
order by isActive desc,LastUpdateDateTime desc
offset (@pageNum-1)*@pageSize rows
fetch next @pageSize rows only;select count(*) from BatchesWithProductsAndUsersName where  productId=@productId
";
            List<BatchReadDTO> batches = new List<BatchReadDTO>();
            int count = 0;
            try
            {
                using (SqlConnection con = new SqlConnection(DataAccessSettings.ConnectionString))
                {
                    await con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@productId", productId);
                        cmd.Parameters.AddWithValue("@pageNum", pageNum);
                        cmd.Parameters.AddWithValue("@pageSize", pageSize);
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {

                            while (reader.Read())
                                batches.Add(new BatchReadDTO((int)reader["batchId"], (int)reader["productId"], (string)reader["productName"],
                                     (decimal)reader["Quantity"], (decimal)reader["purchasePrice"],
                                     reader["SalePrice"] == DBNull.Value ? null : (decimal?)reader["SalePrice"], (int)reader["createdByUserId"], (string)reader["createdByUsername"],
                                     (int)reader["LastUpdatedUserId"], (string)reader["LastupdatedByUsername"],
                                      (DateTime)reader["AddDateTime"], (DateTime)reader["lastUpdateDateTime"], reader["Notes"] == DBNull.Value ? null : (string)reader["Notes"],
                                    (bool)reader["isActive"]));
                            if (reader.NextResult())
                            {
                                if (reader.Read())
                                    count = (int)reader[0];
                            }
                        }
                    }

                }
            }
            catch (Exception ex) { throw; }
            return new AllBatchesReadDTO(batches, count);
        }

    }
}
