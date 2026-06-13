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
    public class InvoiceDAL : IInvoiceDAL
    {
        public async Task<int> Add(InvoiceAddDTO invoice, SqlConnection con, SqlTransaction tran)
        {
            int id = -1;
            string query = @"INSERT INTO [dbo].[Invoices]
           ([TypeId]
           ,[TotalPurchasePrice]
           ,[TotalSalePrice]
           ,[InvoiceDateTime]
           ,[Notes]
           ,[prevInvoiceId]
           ,[CreatedByUserId])
     VALUES
           (@TypeId
           ,@TotalPurchasePrice
           ,@TotalSalePrice
           ,@InvoiceDateTime
           ,@Notes
           ,@prevInvoiceId
           ,@CreatedByUserId);
select SCOPE_IDENTITY();


    ";

            try
            {
                using (SqlCommand cmd = new SqlCommand(query, con, tran))
                {
                    cmd.Parameters.AddWithValue("@TypeId", invoice.TypeId);

                    cmd.Parameters.AddWithValue("@TotalPurchasePrice", invoice.TotalPurchasePrice);
                    cmd.Parameters.AddWithValue("@TotalSalePrice", (object)invoice.TotalSalePrice ?? DBNull.Value);

                    cmd.Parameters.AddWithValue("@InvoiceDateTime", invoice.InvoiceDateTime);

                    cmd.Parameters.AddWithValue("@Notes", (object)invoice.Notes ?? DBNull.Value);

                    cmd.Parameters.AddWithValue("@PrevInvoiceId", (object)invoice.PrevInvoiceId ?? DBNull.Value);

                    cmd.Parameters.AddWithValue("@CreatedByUserId", invoice.CreatedByUserId);
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
        public async Task<int> Add(InvoiceAddDTO invoice)
        {
            int id = -1;
            string query = @"INSERT INTO [dbo].[Invoices]
           ([TypeId]
           ,[TotalPurchasePrice]
           ,[TotalSalePrice]
           ,[InvoiceDateTime]
           ,[Notes]
           ,[prevInvoiceId]
           ,[CreatedByUserId])
     VALUES
           (@TypeId
           ,@TotalPurchasePrice
           ,@TotalSalePrice
           ,@InvoiceDateTime
           ,@Notes
           ,@prevInvoiceId
           ,@CreatedByUserId);
select SCOPE_IDENTITY();


    ";

            try
            {
                using (SqlConnection con = new SqlConnection(DataAccessSettings.ConnectionString))
                {
                    await con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@TypeId", invoice.TypeId);

                        cmd.Parameters.AddWithValue("@TotalPurchasePrice", invoice.TotalPurchasePrice);
                        cmd.Parameters.AddWithValue("@TotalSalePrice", (object)invoice.TotalSalePrice ?? DBNull.Value);

                        cmd.Parameters.AddWithValue("@InvoiceDateTime", invoice.InvoiceDateTime);

                        cmd.Parameters.AddWithValue("@Notes", string.IsNullOrEmpty(invoice.Notes) ? DBNull.Value : invoice.Notes);

                        cmd.Parameters.AddWithValue("@PrevInvoiceId", (object)invoice.PrevInvoiceId ?? DBNull.Value);

                        cmd.Parameters.AddWithValue("@CreatedByUserId", invoice.CreatedByUserId);
                        object result = await cmd.ExecuteScalarAsync();
                        if (result != null && result != DBNull.Value)
                        {
                            id = Convert.ToInt32(result);
                        }
                    }

                }


            }
            catch (Exception e) { throw; }
            return id;
        }
        public async Task<InvoiceSimpleReadDTO> getInvoiceSimpleInfo(int invoiceId, SqlConnection con)
        {
            InvoiceSimpleReadDTO invoice = null;
            string query = @"SELECT [InvoiceId]
      ,[TypeId]
      ,[TotalPurchasePrice]
      ,[TotalSalePrice]
   
     
  FROM [dbo].[Invoices]
where InvoiceId=@InvoiceId";
            try
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {

                    cmd.Parameters.AddWithValue("@InvoiceId", invoiceId);
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (reader.Read())
                        {

                            invoice = new InvoiceSimpleReadDTO((int)reader["InvoiceId"], (byte)reader["TypeId"]
      , (decimal)reader["TotalPurchasePrice"]
      , reader["TotalSalePrice"] == DBNull.Value ? null : (decimal?)reader["TotalSalePrice"]);
                        }

                    }

                }
            }
            catch (Exception e) { throw; }

            return invoice;


        }
        public async Task<InvoiceSimpleReadDTO> getInvoiceSimpleInfo(int invoiceId, SqlConnection con, SqlTransaction tran)
        {
            InvoiceSimpleReadDTO invoice = null;
            string query = @"SELECT [InvoiceId]
      ,[TypeId]
      ,[TotalPurchasePrice]
      ,[TotalSalePrice]
   
     
  FROM [dbo].[Invoices]
where InvoiceId=@InvoiceId";
            try
            {
                using (SqlCommand cmd = new SqlCommand(query, con, tran))
                {

                    cmd.Parameters.AddWithValue("@InvoiceId", invoiceId);
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (reader.Read())
                        {

                            invoice = new InvoiceSimpleReadDTO((int)reader["InvoiceId"], (byte)reader["TypeId"]
      , (decimal)reader["TotalPurchasePrice"]
      , reader["TotalSalePrice"] == DBNull.Value ? null : (decimal?)reader["TotalSalePrice"]);
                        }

                    }

                }
            }
            catch (Exception e) { throw; }

            return invoice;


        }

        public async Task<CashFlowReadDTO> getMoneyFlowBetween(DateTime start, DateTime end)
        {
            CashFlowReadDTO moneyFlow = null;
            string query = @"

select
isnull(sum(case 
when TypeName in ('ReturnPurchase','Sale') then totalSalePrice else 0 end) ,0) as  CashIn,
isnull(sum(case 
when TypeName in ('ReturnSale','Purchase','Expenses') then TotalPurchasePrice else 0 end
) ,0) as CashOut

from  Invoices i join InvoiceTypes t on i.TypeId=t.TypeId  where InvoiceDateTime between @startDate and @endDate";

            using (SqlConnection con = new SqlConnection(DataAccessSettings.ConnectionString))
            {
                await con.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@startDate", start);
                    cmd.Parameters.AddWithValue("@endDate", end);
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (reader.Read())
                        {
                            moneyFlow = new CashFlowReadDTO((decimal)reader["CashIn"], (decimal)reader["CashOut"]);
                        }

                    }
                }
            }

            return moneyFlow;
        }
        public async Task<CashFlowReadDTO> getSalesMoneyFlowBetween(DateTime start, DateTime end)
        {

            CashFlowReadDTO moneyFlow = null;
            string query = @"select
isnull(sum(case 
when TypeName in ('Sale') then totalSalePrice else 0 end) ,0) as CashIn ,
isnull(sum(case 
when TypeName in ('ReturnSale') then TotalPurchasePrice else 0 end
) ,0) as CashOut

from Invoices i join InvoiceTypes t on i.TypeId=t.TypeId  where InvoiceDateTime  between  @startDate and @endDate";

            using (SqlConnection con = new SqlConnection(DataAccessSettings.ConnectionString))
            {
                await con.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@startDate", start);
                    cmd.Parameters.AddWithValue("@endDate", end);
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (reader.Read())
                        {
                            moneyFlow = new CashFlowReadDTO((decimal)reader["CashIn"], (decimal)reader["CashOut"]);
                        }

                    }
                }
            }

            return moneyFlow;
        }

        public async Task<InvoiceReadDTO> getInvoiceWithDetails(int invoiceId)
        {
            InvoiceReadDTO invoice = null;
            string query = @"

select  i.InvoiceId,
    i.TypeId,
    i.InvoiceDateTime,
    i.CreatedByUserId,
    i.PrevInvoiceId,
    i.TotalPurchasePrice,
	i.TotalSalePrice,
    i.Notes,
    u.UserName AS CreatedByUserName  from Invoices i join Users u on i.CreatedByUserId=u.UserId
where InvoiceId=@InvoiceId ;

select i.*,p.ProductName from invoiceItems i join Batches b on i.BatchID=b.batchId join Products p on p.ProductId=b.productId where InvoiceId=@InvoiceId";
            using (SqlConnection con = new SqlConnection(DataAccessSettings.ConnectionString))
            {
                await con.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@invoiceId", invoiceId);

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {

                        if (reader.Read())
                        {
                            invoice = new InvoiceReadDTO
                            (
                                (int)reader["InvoiceId"],
                               (byte)reader["typeid"],
                                 (decimal)reader["TotalPurchasePrice"],

                                reader["TotalSalePrice"] == DBNull.Value ? null : (decimal?)reader["TotalSalePrice"],
                                (DateTime)reader["InvoiceDateTime"],
                                 reader["Notes"] == DBNull.Value ? null : (string)reader["Notes"],
                               reader["PrevInvoiceId"] == DBNull.Value ? null : (int?)reader["PrevInvoiceId"],
                                 (int)reader["CreatedByUserId"],
                                  (string)reader["CreatedByUsername"], new List<InvoiceItemReadDTO>()
                            );


                            reader.NextResult();
                            while (reader.Read())
                            {
                                invoice.Items.Add(new InvoiceItemReadDTO((int)reader["itemId"], (int)reader["InvoiceId"], (int)reader["batchId"], (decimal)reader["purchasePrice"], reader["salePrice"] == DBNull.Value ?
                                    null : (decimal?)reader["salePrice"], (decimal)reader["quantity"], (string)reader["ProductName"]));
                            }


                        }

                    }
                }
            }
            return invoice;
        }
        public async Task<InvoiceReadDTO> getInvoiceWithDetails(int invoiceId, SqlConnection con)
        {
            InvoiceReadDTO invoice = null;
            string query = @"

select  i.InvoiceId,
    i.TypeId,
    i.InvoiceDateTime,
    i.CreatedByUserId,
    i.PrevInvoiceId,
    i.TotalPurchasePrice,
	i.TotalSalePrice,
    i.Notes,
    u.UserName AS CreatedByUserName  from Invoices i join Users u on i.CreatedByUserId=u.UserId
where InvoiceId=@InvoiceId ;

select i.*,p.ProductName from invoiceItems i join Batches b on i.BatchID=b.batchId join Products p on p.ProductId=b.productId where InvoiceId=@InvoiceId";

            await con.OpenAsync();
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@invoiceId", invoiceId);

                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {

                    if (reader.Read())
                    {
                        invoice = new InvoiceReadDTO
                        (
                            (int)reader["InvoiceId"],
                           (byte)reader["typeid"],
                             (decimal)reader["TotalPurchasePrice"],

                            reader["TotalSalePrice"] == DBNull.Value ? null : (decimal?)reader["TotalSalePrice"],
                            (DateTime)reader["InvoiceDateTime"],
                             reader["Notes"] == DBNull.Value ? null : (string)reader["Notes"],
                           reader["PrevInvoiceId"] == DBNull.Value ? null : (int?)reader["PrevInvoiceId"],
                             (int)reader["CreatedByUserId"],
                              (string)reader["CreatedByUsername"], new List<InvoiceItemReadDTO>()
                        );


                        reader.NextResult();
                        while (reader.Read())
                        {
                            invoice.Items.Add(new InvoiceItemReadDTO((int)reader["itemId"], (int)reader["InvoiceId"], (int)reader["batchId"], (decimal)reader["purchasePrice"], reader["salePrice"] == DBNull.Value ?
                                null : (decimal?)reader["salePrice"], (decimal)reader["quantity"], (string)reader["ProductName"]));
                        }




                    }
                }
            }
            return invoice;
        }
       public  async Task<InvoiceReadDTO> getInvoiceWithDetailsWithOutItems(int invoiceId, SqlConnection con)
        {
            InvoiceReadDTO invoice = null;
            string query = @"

select  i.InvoiceId,
    i.TypeId,
    i.InvoiceDateTime,
    i.CreatedByUserId,
    i.PrevInvoiceId,
    i.TotalPurchasePrice,
	i.TotalSalePrice,
    i.Notes,
    u.UserName AS CreatedByUserName  from Invoices i join Users u on i.CreatedByUserId=u.UserId
where InvoiceId=@InvoiceId ;
";

            await con.OpenAsync();
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@invoiceId", invoiceId);

                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {

                    if (reader.Read())
                    {
                        invoice = new InvoiceReadDTO
                        (
                            (int)reader["InvoiceId"],
                           (byte)reader["typeid"],
                             (decimal)reader["TotalPurchasePrice"],

                            reader["TotalSalePrice"] == DBNull.Value ? null : (decimal?)reader["TotalSalePrice"],
                            (DateTime)reader["InvoiceDateTime"],
                             reader["Notes"] == DBNull.Value ? null : (string)reader["Notes"],
                           reader["PrevInvoiceId"] == DBNull.Value ? null : (int?)reader["PrevInvoiceId"],
                             (int)reader["CreatedByUserId"],
                              (string)reader["CreatedByUsername"], new List<InvoiceItemReadDTO>()
                        );


                      




                    }
                }
            }
            return invoice;
        }

        public async Task<ReadAllInvoicesDTO> getAllInvoicesWithFilters(InvoicesFilterDTO Filter)
        {
            (List<InvoiceReadDTO> invoices, int count) allInvoices = (new List<InvoiceReadDTO>(), 0);
            List<string> InvoiceTypeParamNames = new List<string>();

            string additionalWhereQueries = "", countQuery;
            string query = @"select  i.InvoiceId,
    i.TypeId,
    i.InvoiceDateTime,
    i.CreatedByUserId,
    i.PrevInvoiceId,
    i.TotalPurchasePrice,
	i.TotalSalePrice,
    i.Notes,
    u.UserName AS CreatedByUserName  from Invoices i join Users u on i.CreatedByUserId=u.UserId where 1=1 ";
            if (Filter.InvoiceTypeIds != null && Filter.InvoiceTypeIds.Count > 0)
            {
                for (int i = 1; i <= Filter.InvoiceTypeIds.Count; i++)
                {
                    InvoiceTypeParamNames.Add($"@p{i}");
                }

                additionalWhereQueries += $" and TypeId in ({string.Join(",", InvoiceTypeParamNames)}) ";

            }
          
            if (Filter.PrevInvoiceId!=null)
            {
                if (Filter.PrevInvoiceId.PrevId == null && Filter.PrevInvoiceId.IsNullInFilter==true)
                    additionalWhereQueries += " and prevInvoiceId is null";
                if (Filter.PrevInvoiceId.PrevId != null)
                    additionalWhereQueries += " and prevInvoiceId=@prevInvoiceId";
              
            }
            if (Filter.UserId != null) additionalWhereQueries += " and i.CreatedByUserId=@CreatedByUserId";
            if (Filter.TimeInterval != null)
                additionalWhereQueries += " and InvoiceDateTime between @startDate and @endDate ";
            query += additionalWhereQueries;
            query += @" order by InvoiceId
offset (@pageNum-1)*@pageSize rows
fetch next @pageSize  rows only;
";
            countQuery = " select count(*) from invoices i where 1=1 " + additionalWhereQueries;
            query += countQuery;
            using (SqlConnection con = new SqlConnection(DataAccessSettings.ConnectionString))
            {
                await con.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    if (Filter.InvoiceTypeIds != null && Filter.InvoiceTypeIds.Count > 0)
                    {
                        for (int i = 0; i < Filter.InvoiceTypeIds.Count; i++)
                        {
                            cmd.Parameters.AddWithValue(InvoiceTypeParamNames[i], Filter.InvoiceTypeIds[i]);
                        }
                    }
                    if (Filter.PrevInvoiceId is {PrevId:not null })
                        cmd.Parameters.AddWithValue("@prevInvoiceId", Filter.PrevInvoiceId.PrevId);
                    if (Filter.UserId != null)
                        cmd.Parameters.AddWithValue("@CreatedByUserId", Filter.UserId);
                    if (Filter.TimeInterval != null)
                    {
                        cmd.Parameters.AddWithValue("@startDate", Filter.TimeInterval.From);
                        cmd.Parameters.AddWithValue("@endDate", Filter.TimeInterval.To);
                    }

                    cmd.Parameters.AddWithValue("@pageNum", Filter.PageNum);
                    cmd.Parameters.AddWithValue("@pageSize", Filter.PageSize);
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {

                        while (reader.Read())
                        {
                            allInvoices.invoices.Add(new InvoiceReadDTO(
        (int)reader["InvoiceId"],
        (byte)reader["TypeId"],
        (decimal)reader["TotalPurchasePrice"],
        reader["TotalSalePrice"] == DBNull.Value ? null : (decimal?)reader["TotalSalePrice"],
        (DateTime)reader["InvoiceDateTime"],
        reader["Notes"] == DBNull.Value ? null : (string)reader["Notes"],
        reader["PrevInvoiceId"] == DBNull.Value ? null : (int?)reader["PrevInvoiceId"],
        (int)reader["CreatedByUserId"],
        (string)reader["CreatedByUserName"],
        new List<InvoiceItemReadDTO>()));
                        }
                        if (reader.NextResult())
                        {
                            if (reader.Read())
                            {
                                allInvoices.count = (int)reader[0];

                            }

                        }
                    }
                }
            }
            return new ReadAllInvoicesDTO(allInvoices.invoices,allInvoices.count);
        }




    }
}
