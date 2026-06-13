using InventoryShared;

namespace InventoryBL.Interfaces
{
    public interface IUserService
    {
        Task<int> Add(UserAddDTO user);
        Task<bool> Delete(int UserId);
        Task<int> getPagesNum(int pageSize);
        Task<UserReadDTO> getUser(int userId);
        Task<UserReadDTO> getUser(string userName);
        Task<AuthenticatedUserDTO?> getUserForLogin(LoginRequestDTO request);
        Task<(List<UserReadDTO> users, int usersCount)> getUserList(int pageNum, int pageSize);
        Task<bool> UpdatePassword(UserUpdatePasswordDTO user);
        Task<bool> UpdatePermissions(UserUpdatePermissionDTO user);
    }
}