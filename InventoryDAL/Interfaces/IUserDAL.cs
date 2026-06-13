using InventoryShared;
using Microsoft.Data.SqlClient;

namespace InventoryDAL.Interfaces
{
    public interface IUserDAL
    {
        Task<int> Add(UserAddDTO user);
        Task<bool> Delete(int UserId);
        Task<int> getPagesNum(int pageSize);
        Task<UserReadDTO> getUser(int UserId);
        Task<UserReadDTO> getUser(string UserName);
        Task<LoginResponseDTO> getLoginResponseByUserName(string UserName);
        Task<(List<UserReadDTO> UsersList, int usersCount)> getUsersList(int pageNum, int pageSize);
        Task<string?> getHashedPasswordUserName(int UserId);
        bool isUsernameExist(string userName);
        bool isUsernameExist(string userName, SqlConnection con);
        Task<bool> UpdatePassword(UserUpdatePasswordDTO user);
        Task<bool> UpdatePermissions(UserUpdatePermissionDTO user);
    }
}