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
    public class UserDAL : IUserDAL
    {
        public async Task<int> Add(UserAddDTO user)
        {
            int ID = -1;
            string query = @"INSERT INTO [dbo].[Users]
           ([UserName]
           ,[Password]
           ,[CreatedByUserId]
           ,[Permissions])
     VALUES
           (@UserName
           ,@Password
           ,@CreatedByUserId
           ,@Permissions);
            select SCOPE_IDENTITY();";
            try
            {
                using (SqlConnection conn = new SqlConnection(DataAccessSettings.ConnectionString))
                {
                    await conn.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserName", user.UserName);
                        cmd.Parameters.AddWithValue("@Password", user.Password);
                        cmd.Parameters.AddWithValue("@CreatedByUserId", user.CreatedByUserId ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Permissions", user.Permissions);
                        object result = await cmd.ExecuteScalarAsync();
                        if (result == null || result == DBNull.Value)
                            throw new Exception("Added Was Failed");
                        ID = Convert.ToInt32(result);

                    }
                }

            }
            catch (Exception e)
            {
                throw;
            }
            return ID;
        }

       
        public bool isUsernameExist(string userName, SqlConnection con)
        {
            string query = @"select 1 from Users where Username=@userName ";
            bool isFound = false;
            try
            {


                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@username", userName);

                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)

                        isFound = true;

                }



            }
            catch (Exception e)
            {
                throw;
            }
            return isFound;
        }
        public bool isUsernameExist(string userName)
        {

            bool isFound = false;
            using (SqlConnection con = new SqlConnection(DataAccessSettings.ConnectionString))
            {
                isFound = isUsernameExist(userName, con);
            }
            return isFound;
        }
        public async Task<UserReadDTO> getUser(int UserId)
        {
            UserReadDTO user = null;
            string query = @"select u1.UserId,u1.UserName,u1.CreatedByUserId,U2.UserName as CreatedByUserName,u1.Permissions from Users u1 left join Users u2 On u1.CreatedByUserId=u2.UserId
where u1.UserId=@UserId ";
            try
            {
                using (SqlConnection con = new SqlConnection(DataAccessSettings.ConnectionString))
                {
                    await con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@UserId", UserId);

                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (reader.Read())
                            {
                                user = new UserReadDTO((int)reader["UserId"], (string)reader["UserName"],
                                    reader["CreatedByUserId"] == DBNull.Value ? null : (int?)reader["CreatedByUserId"], reader["CreatedByUserName"] == DBNull.Value ? null : (string)reader["CreatedByUserName"]
                                    , (int)reader["Permissions"]);

                            }



                        }
                    }

                }

            }
            catch (Exception e)
            {
                throw;
            }

            return user;
        }
        public async Task<UserReadDTO> getUser(string UserName)
        {
            UserReadDTO user = null;
            string query = @"select u1.UserId,u1.UserName,u1.CreatedByUserId,U2.UserName as CreatedByUserName,u1.Permissions from Users u1 left join Users u2 On u1.CreatedByUserId=u2.UserId
where u1.Username=@userName";
            try
            {
                using (SqlConnection con = new SqlConnection(DataAccessSettings.ConnectionString))
                {
                    await con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@username", UserName);

                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (reader.Read())
                            {
                                user = new UserReadDTO((int)reader["UserId"], (string)reader["UserName"],
                                    reader["CreatedByUserId"] == DBNull.Value ? null : (int?)reader["CreatedByUserId"], reader["CreatedByUserName"] == DBNull.Value ? null : (string)reader["CreatedByUserName"],
                                    (int)reader["Permissions"]);

                            }



                        }
                    }

                }

            }
            catch (Exception e)
            {
                throw;
            }

            return user;
        }
        public async Task<int> getPagesNum(int pageSize)
        {
            string query = @"select ceiling(Convert(decimal(6,1),count(UserId))/@pageSize )from Users";
            int count = 0;
            try
            {
                using (SqlConnection con = new SqlConnection(DataAccessSettings.ConnectionString))
                {
                    await con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {

                        cmd.Parameters.AddWithValue("@pageSize", pageSize);
                        object result = await cmd.ExecuteScalarAsync();
                        if (result != null && result != DBNull.Value)
                        {
                            count = Convert.ToInt32(result);
                        }

                    }

                }

            }
            catch (Exception e)
            {
                throw;
            }
            return count;
        }

        public async Task<(List<UserReadDTO> UsersList, int usersCount)> getUsersList(int pageNum, int pageSize)
        {

            List<UserReadDTO> users = new List<UserReadDTO>();
            int usersCount = 0;
            string query = @"select u1.UserId,u1.UserName,u1.CreatedByUserId,U2.UserName as CreatedByUserName,u1.Permissions from Users u1 left join Users u2 On u1.CreatedByUserId=u2.UserId
order by UserId
offset (@pageNum-1)*@pageSize rows
fetch next @pageSize rows only;
select count(*) from users";
            try
            {
                using (SqlConnection con = new SqlConnection(DataAccessSettings.ConnectionString))
                {
                    await con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@pageNum", pageNum);
                        cmd.Parameters.AddWithValue("@pageSize", pageSize);
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (reader.Read())
                            {
                                users.Add(new UserReadDTO((int)reader["UserId"], (string)reader["UserName"],
                                    reader["CreatedByUserId"] == DBNull.Value ? null : (int?)reader["CreatedByUserId"]
                                    , reader["CreatedByUserName"] == DBNull.Value ? null : (string)reader["CreatedByUserName"], (int)reader["Permissions"]));

                            }
                            reader.NextResult();
                            if (reader.Read())
                            {
                                usersCount = (int)reader[0];
                            }


                        }

                    }

                }

            }
            catch (Exception e)
            {
                throw;
            }
            return (users, usersCount);
        }
        public async Task<UserReadDTO> getUserByUserNameAndPassword(string UserName, string password)
        {
            UserReadDTO user = null;
            string query = @"select u1.UserId,u1.UserName,u1.CreatedByUserId,U2.UserName as CreatedByUserName,u1.Permissions from Users u1 left join Users u2 On u1.CreatedByUserId=u2.UserId
                                where u1.Username=@userName and u1.Password=@password";
            try
            {
                using (SqlConnection con = new SqlConnection(DataAccessSettings.ConnectionString))
                {
                    await con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@username", UserName);
                        cmd.Parameters.AddWithValue("@password", password);
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (reader.Read())
                            {
                                user = new UserReadDTO((int)reader["UserId"], (string)reader["UserName"],
                                    reader["CreatedByUserId"] == DBNull.Value ? null : (int?)reader["CreatedByUserId"]
                                    , reader["CreatedByUserName"] == DBNull.Value ? null : (string)reader["CreatedByUserName"], (int)reader["Permissions"]);

                            }



                        }
                    }

                }

            }
            catch (Exception e)
            {
                throw;
            }

            return user;
        }
        public async Task<LoginResponseDTO> getLoginResponseByUserName(string UserName)
        {
            LoginResponseDTO user = null;
            string query = @"select UserId,UserName,Password,Permissions from Users  
                                where Username=@userName;";
            try
            {
                using (SqlConnection con = new SqlConnection(DataAccessSettings.ConnectionString))
                {
                    await con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@username", UserName);
                   
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (reader.Read())
                            {
                                user = new LoginResponseDTO((int)reader["UserId"], (string)reader["UserName"],
                                   (string)reader["Password"], (int)reader["Permissions"]);

                            }



                        }
                    }

                }

            }
            catch (Exception e)
            {
                throw;
            }

            return user;
        }
        public async Task<string?> getHashedPasswordUserName(int UserId)
        {
            
            string? password = null;
            string query = @"select Password from Users  
                                where UserId=@UserId;";
            try
            {
                using (SqlConnection con = new SqlConnection(DataAccessSettings.ConnectionString))
                {
                    await con.OpenAsync();
                  
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@UserId", UserId);

                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (reader.Read())
                            {
                                password= (string)reader["Password"];

                            }



                        }
                    }

                }

            }
            catch (Exception e)
            {
                throw;
            }

            return password;
        }
        public async Task<bool> UpdatePassword(UserUpdatePasswordDTO user)
        {
            int RowsAffected = 0;
            string query = @"
         UPDATE [dbo].[Users]
          SET 
          [Password] = @Password
          WHERE UserId=@userId;";
            try
            {
                using (SqlConnection con = new SqlConnection(DataAccessSettings.ConnectionString))
                {
                    await con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@userId", user.UserId);
                        cmd.Parameters.AddWithValue("@Password", user.Password);
                        RowsAffected = await cmd.ExecuteNonQueryAsync();
                    }

                }
            }
            catch (Exception e)
            {
                throw;
            }
            return RowsAffected > 0;
        }
        public async Task<bool> UpdatePermissions(UserUpdatePermissionDTO user)
        {
            int RowsAffected = 0;
            string query = @"
UPDATE [dbo].[Users]
   SET 
      [Permissions] = @Permissions
 WHERE UserId=@userId;";
            try
            {
                using (SqlConnection con = new SqlConnection(DataAccessSettings.ConnectionString))
                {
                    await con.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@userId", user.UserId);
                        cmd.Parameters.AddWithValue("@Permissions", user.Permission);
                        RowsAffected = await cmd.ExecuteNonQueryAsync();
                    }

                }
            }
            catch (Exception e)
            {
                throw;
            }
            return RowsAffected > 0;
        }
        public async Task<bool> Delete(int UserId)
        {
            int rowsAffected = 0;
            string query = @"delete users where UserId=@userId";
            using (SqlConnection con = new SqlConnection(DataAccessSettings.ConnectionString))
            {
                await con.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@userId", UserId);
                    rowsAffected = await cmd.ExecuteNonQueryAsync();
                }
            }
            return rowsAffected > 0;
        }
    }
}

