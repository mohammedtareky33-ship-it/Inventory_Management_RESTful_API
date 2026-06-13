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
    public class InvoiceItemDAL : IInvoiceItemDAL
    {
        public async Task<int> Add(InvoiceItemAddDTO invoiceItem, SqlConnection con, SqlTransaction tran)
        {
            int id = -1;
            string query = @"INSERT INTO [dbo].[InvoiceItems]
           ([InvoiceId]
           ,[BatchID]
           ,[PurchasePrice]
           ,[SalePrice]
           ,[Quantity])
     VALUES
           (@InvoiceId
           ,@BatchID
           ,@PurchasePrice
           ,@SalePrice
           ,@Quantity);
		   select SCOPE_IDENTITY();";
            try
            {
                using (SqlCommand cmd = new SqlCommand(query, con, tran))
                {
                    cmd.Parameters.AddWithValue("@InvoiceId", invoiceItem.InvoiceId);
                    cmd.Parameters.AddWithValue("@BatchID", invoiceItem.BatchId);
                    cmd.Parameters.AddWithValue("@PurchasePrice", invoiceItem.PurchasePrice);
                    cmd.Parameters.AddWithValue("@SalePrice", (object)invoiceItem.SalePrice ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Quantity", invoiceItem.Quantity);
                    object result = await cmd.ExecuteScalarAsync();
                    if (result != null && result != DBNull.Value)
                    {
                        id = Convert.ToInt32(result);
                    }

                }
            }
            catch (Exception e) { throw; }
            return id;
        }
        public async Task<List<InvoiceItemReadDTO>> getInvoiceItems(int invoiceId, SqlConnection con)
        {
            List<InvoiceItemReadDTO> invoiceItems = null;
            string query = @"select i.*,p.ProductName from invoiceItems i join Batches b on i.BatchID=b.batchId join Products p on p.ProductId=b.productId where InvoiceId=@InvoiceId";
            try
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@InvoiceId", invoiceId);
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            invoiceItems = new List<InvoiceItemReadDTO>();
                            while (reader.Read())
                            {
                                invoiceItems.Add(new InvoiceItemReadDTO((int)reader["ItemId"], (int)reader["InvoiceId"], (int)reader["batchId"], (decimal)reader["PurchasePrice"],
                                    reader["SalePrice"] == DBNull.Value ? null : (decimal?)reader["SalePrice"], (decimal)reader["Quantity"], (string)reader["ProductName"]));

                            }
                        }

                    }
                }
            }
            catch (Exception e) { throw; }
            return invoiceItems;

        }
        public async Task<List<InvoiceItemReadDTO>> getInvoiceItems(int invoiceId)
        {
            List<InvoiceItemReadDTO> invoiceItems = null;
            try
            {
                using (SqlConnection con = new SqlConnection(DataAccessSettings.ConnectionString))
                {
                    await con.OpenAsync();
                    invoiceItems = await getInvoiceItems(invoiceId, con);


                }
            }
            catch (Exception e) { throw; }
            return invoiceItems;
        }
        public async Task<List<InvoiceItemReadDTO>> getInvoiceItems(int invoiceId, SqlConnection con, SqlTransaction tran)
        {
            List<InvoiceItemReadDTO> invoiceItems = null;
            string query = @"select i.*,p.ProductName from invoiceItems i join Batches b on i.BatchID=b.batchId join Products p on p.ProductId=b.productId where InvoiceId=@InvoiceId";
            try
            {
                using (SqlCommand cmd = new SqlCommand(query, con, tran))
                {
                    cmd.Parameters.AddWithValue("@InvoiceId", invoiceId);
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            invoiceItems = new List<InvoiceItemReadDTO>();
                            while (reader.Read())
                            {
                                invoiceItems.Add(new InvoiceItemReadDTO((int)reader["ItemId"], (int)reader["InvoiceId"], (int)reader["batchId"], (decimal)reader["PurchasePrice"],
                                    reader["SalePrice"] == DBNull.Value ? null : (decimal?)reader["SalePrice"], (decimal)reader["Quantity"], (string)reader["productName"]));

                            }
                        }

                    }
                }
            }
            catch (Exception e) { throw; }
            return invoiceItems;

        }

        public async Task<List<InvoiceItemSimpleReadDTO>> getRemainderOfItemsQuantityToReturn(int MainInvoiceID, SqlConnection con, SqlTransaction tran)
        {
            List<InvoiceItemSimpleReadDTO> AvilableItemsQuantity = null;
            string query = @"select BatchId,Quantity from RemainingInvoiceItemsFromRetuns where invoiceId=@InvoiceId";
            try
            {
                using (SqlCommand cmd = new SqlCommand(query, con, tran))
                {
                    cmd.Parameters.AddWithValue("@InvoiceId", MainInvoiceID);
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            AvilableItemsQuantity = new List<InvoiceItemSimpleReadDTO>();
                        }
                        while (reader.Read())
                        {
                            AvilableItemsQuantity.Add(new InvoiceItemSimpleReadDTO((int)reader["BatchId"], (decimal)reader["Quantity"]));
                        }

                    }
                }

            }
            catch (Exception e) { throw; }
            return AvilableItemsQuantity;
        }
        //        public async Task<List<InvoiceItemSimpleReadDTO>> getRemainderOfItemsQuantityToReturn(int MainInvoiceID, SqlConnection con)
        //        {
        //            List<InvoiceItemSimpleReadDTO> AvilableItemsQuantity = null;
        //            string query = @"with returnItems as(
        //select
        //it.BatchID,sum(it.Quantity) as ReturnedQuantity,it.SalePrice
        //from 
        //InvoiceItems it
        //join Invoices i 
        //on it.InvoiceId=i.InvoiceId
        //where prevInvoiceId=@InvoiceId
        //group by it.BatchID,it.SalePrice

        //)
        //, remainingItemsTable as(
        //select i.ItemId,(i.Quantity-ISNULL(ri.ReturnedQuantity,0)) as Quantity from InvoiceItems i left join returnItems ri on i.BatchID=ri.BatchID and i.SalePrice=ri.SalePrice
        //where InvoiceId=@InvoiceId )
        //select*from remainingItemsTable where Quantity>0";
        //            try
        //            {
        //                using (SqlCommand cmd = new SqlCommand(query, con))
        //                {
        //                    cmd.Parameters.AddWithValue("@InvoiceId", MainInvoiceID);
        //                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
        //                    {
        //                        if (reader.HasRows)
        //                        {
        //                            AvilableItemsQuantity=new List<InvoiceItemSimpleReadDTO>();
        //                        }
        //                        while (reader.Read())
        //                        {
        //                            AvilableItemsQuantity.Add(new InvoiceItemSimpleReadDTO((int)reader["ItemId"], (decimal)reader["Quantity"]));
        //                        }

        //                    }
        //                }

        //            }
        //            catch (Exception e) { throw; }
        //            return AvilableItemsQuantity;
        //        }
        public async Task<List<InvoiceItemReadDTO>> getRemainderOfItemsQuantityToReturnWithDetails(int MainInvoiceID, SqlConnection con)
        {
            List<InvoiceItemReadDTO> AvilableItemsQuantity = null;
            string query = @"select*from RemainingInvoiceItemsFromRetuns where invoiceId=@InvoiceId";
            try
            {

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@InvoiceId", MainInvoiceID);
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            AvilableItemsQuantity = new List<InvoiceItemReadDTO>();
                        }
                        while (reader.Read())
                        {
                            AvilableItemsQuantity.
                            Add(new InvoiceItemReadDTO((int)reader["ItemId"], (int)reader["InvoiceId"], (int)reader["batchId"], (decimal)reader["PurchasePrice"],
                                reader["SalePrice"] == DBNull.Value ? null : (decimal?)reader["SalePrice"], (decimal)reader["Quantity"], (string)reader["productName"]));
                        }

                    }
                }


            }
            catch (Exception e) { throw; }
            return AvilableItemsQuantity;
        }
    }
}
