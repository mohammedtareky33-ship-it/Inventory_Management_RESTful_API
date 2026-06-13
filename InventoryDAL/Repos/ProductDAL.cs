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
    public class ProductDAL : IProductDAL
    {
        public async Task<int> Add(ProductAddDTO product)
        {
            int Id = -1;
            string query = @"INSERT INTO [dbo].[Products]
           ([ProductName]
           ,[Notes]
           ,[CreatedByUserId]
          )
     VALUES
           (@ProductName,
           @Notes,
           @CreatedByUserId);
		   select SCOPE_IDENTITY();";
            try
            {
                using (SqlConnection con = new SqlConnection(DataAccessSettings.ConnectionString))
                {
                    await con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@ProductName", product.ProductName);
                        cmd.Parameters.AddWithValue("@CreatedByUserId", product.CreatedByUserId);
                        cmd.Parameters.AddWithValue("@Notes", string.IsNullOrEmpty(product.Notes) ? DBNull.Value : product.Notes);
                        object result = await cmd.ExecuteScalarAsync();
                        if (result != null && result != DBNull.Value)
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
        public async Task<ProductReadDTO> getProduct(int productId)
        {
            ProductReadDTO product = null;
            string query = @"select p.*,u.UserName from Products p join Users u on p.CreatedByUserId=u.UserId where productId=@productId";
            using (SqlConnection con = new SqlConnection(DataAccessSettings.ConnectionString))
            {
                await con.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@productId", productId);
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (reader.Read())
                        {
                            product = new ProductReadDTO((int)reader["productId"], (string)reader["productName"], (int)reader["CreatedByUserId"], (string)reader["UserName"],
                                reader["Notes"] == DBNull.Value ? "" : (string)reader["Notes"], (DateTime)reader["AddDateTime"]);
                        }
                    }
                }
            }
            return product;
        }
        public async Task<ProductReadDTO> getProduct(string productName)
        {

            ProductReadDTO product = null;
            string query = @"select p.*,u.UserName from Products p join Users u on p.CreatedByUserId=u.UserId where productName=@productName";
            using (SqlConnection con = new SqlConnection(DataAccessSettings.ConnectionString))
            {
                await con.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@productName", productName);
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (reader.Read())
                        {
                            product = new ProductReadDTO((int)reader["productId"], (string)reader["productName"], (int)reader["CreatedByUserId"], (string)reader["UserName"],
                                reader["Notes"] == DBNull.Value ? "" : (string)reader["Notes"], (DateTime)reader["AddDateTime"]);
                        }
                    }
                }
            }
            return product;

        }
        public async Task<AllProductReadDTO> getAllProducts(int pageNum, int pageSize)
        {
            List<ProductReadDTO> products = null;
            int count = 0;
            string query = @"select p.*,u.UserName from Products p join Users u on p.CreatedByUserId=u.UserId
order by p.ProductId
offSet (@PageNum-1)*@pageSize rows
fetch next @pageSize rows only;
                                select Count(*) from products;";
            using (SqlConnection con = new SqlConnection(DataAccessSettings.ConnectionString))
            {
                await con.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@PageNum", pageNum);
                    cmd.Parameters.AddWithValue("@pageSize", pageSize);

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            products = new List<ProductReadDTO>();
                        }

                        while (reader.Read())
                        {
                            products.Add(new ProductReadDTO((int)reader["productId"], (string)reader["productName"], (int)reader["CreatedByUserId"], (string)reader["UserName"],
                                reader["Notes"] == DBNull.Value ? "" : (string)reader["Notes"], (DateTime)reader["AddDateTime"]));
                        }
                        if (reader.NextResult())
                        {
                            if (reader.Read())
                            {
                                count = (int)reader[0];
                            }
                        }
                    }
                }
            }
            return new AllProductReadDTO(products,count);
        }
        public async Task<bool> Update(ProductUpdateDTO product)
        {
            int numRowsAffected = 0;
            string query = @"Update Products
                             set Notes=@Notes
                             where ProductId=@productId";
            using (SqlConnection con = new SqlConnection(DataAccessSettings.ConnectionString))
            {
                await con.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@productId", product.ProductId);
                    cmd.Parameters.AddWithValue("@Notes", product.Notes);
                    numRowsAffected = await cmd.ExecuteNonQueryAsync();
                }
                return numRowsAffected > 0;
            }
        }
        public async Task<bool> Delete(int productId)
        {


            int numRowsAffected = 0;
            string query = @"delete from Products where ProductId=@productId";
            using (SqlConnection con = new SqlConnection(DataAccessSettings.ConnectionString))
            {
                await con.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@productId", productId);
                    numRowsAffected = await cmd.ExecuteNonQueryAsync();
                }
                return numRowsAffected > 0;
            }
        }
        public async Task<bool> isProductExist(string productName)
        {
            bool isFound = false;

            string query = @"select 1 from Products where productName=@productName";
            using (SqlConnection con = new SqlConnection(DataAccessSettings.ConnectionString))
            {
                await con.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@productName", productName);

                    object result = await cmd.ExecuteScalarAsync();
                    if (result!=null&&result != DBNull.Value)
                    {
                        isFound = true;
                    }

                }
            }
            return isFound;

        }
        public async Task<bool> isProductExist(int productId)
        {
            bool isFound = false;

            string query = @"select 1 from Products where productId=@productId";
            using (SqlConnection con = new SqlConnection(DataAccessSettings.ConnectionString))
            {
                await con.OpenAsync();
                isFound = await isProductExist(productId, con);
            }
            return isFound;

        }
        public async Task<bool> isProductExist(int productId, SqlConnection con)
        {
            bool isFound = false;

            string query = @"select 1 from Products where productId=@productId";
            try
            {

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@productId", productId);

                    object result = await cmd.ExecuteScalarAsync();
                    if (result!=null&&result != DBNull.Value)
                    {
                        isFound = true;
                    }

                }
            }
            catch (Exception ex) { throw; }

            return isFound;

        }
        public async Task<bool> isProductExist(int productId, SqlConnection con, SqlTransaction tran)
        {
            bool isFound = false;

            string query = @"select 1 from Products where productId=@productId";
            try
            {

                using (SqlCommand cmd = new SqlCommand(query, con, tran))
                {
                    cmd.Parameters.AddWithValue("@productId", productId);

                    object result = await cmd.ExecuteScalarAsync();
                    if (result!=null&&result != DBNull.Value)
                    {
                        isFound = true;
                    }

                }
            }
            catch (Exception ex) { throw; }

            return isFound;

        }
    }
}
