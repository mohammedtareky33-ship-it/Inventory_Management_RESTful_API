using InventoryShared;

using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens.Experimental;
using System.ComponentModel.DataAnnotations;
using InventoryBL.Interfaces;
using InventoryDAL.Repos;
using InventoryDAL.Interfaces;

namespace InventoryBL.Services
{
    public class UserService : IUserService
    {
        IUserDAL _UserDataAccess;
        public UserService(IUserDAL userDAL) {
        
        _UserDataAccess = userDAL;
        }
        bool isValidToAdd(UserAddDTO user)
        {
            if (string.IsNullOrEmpty(user.UserName) || user.UserName.Length < 3 || string.IsNullOrEmpty(user.Password) || user.Password.Length < 4)
            { return false; }
            return true;
        }
        bool isValidNewPassword(string pass)
        {
            return pass != null && pass.Length > 3;
        }
        public async Task<int> Add(UserAddDTO user)
        {

            if (user == null || !isValidToAdd(user))
                throw new ValidationException("Username must be above 2 chars and password above 3 chars");
            user = new UserAddDTO(user.UserName, HashHelper.getHashedPassword(user.Password), user.CreatedByUserId, user.Permissions);

            return await _UserDataAccess.Add(user);


        }
        public async Task<UserReadDTO> getUser(int userId)
        {
            if (userId<1)
                throw new ValidationException("Id must be greater than 0");

            return await _UserDataAccess.getUser(userId);
        }
        public async Task<UserReadDTO> getUser(string userName)
        {
            if (string.IsNullOrEmpty(userName)||userName.Length<3)
                throw new ValidationException("invalid User name");
            userName = userName?.Trim();
          
            return await _UserDataAccess.getUser(userName);
        }
        public async Task<int> getPagesNum(int pageSize)
        {

            return await _UserDataAccess.getPagesNum(pageSize);
        }
        public async Task<(List<UserReadDTO> users, int usersCount)> getUserList(int pageNum, int pageSize)
        {

            return await _UserDataAccess.getUsersList(pageNum, pageSize);
        }
        public async Task<AuthenticatedUserDTO?> getUserForLogin(LoginRequestDTO request)
        {
            if (string.IsNullOrEmpty(request.UserName)||request.UserName.Length<3)
                throw new ValidationException("invalid User name");
            var user =await _UserDataAccess.getLoginResponseByUserName(request.UserName);
            if (user == null||!HashHelper.VerifyPassword(request.Password, user.HashPassword))
              return null;
            return new AuthenticatedUserDTO(user.UserId, user.UserName, user.Permissions);
        }
        public async Task<bool> UpdatePassword(UserUpdatePasswordDTO user)
        {
            try
            {
                if (user.UserId<1)
                    throw new ValidationException("Id must be greater than 0");

                if (!isValidNewPassword(user.Password))
                    throw new ValidationException("the new Password must be above or equal 4 chars");
                if(string.IsNullOrEmpty(user.PrevPassword ))
                    throw new ValidationException("Prev Password is Invalid");
                var prevPassword =await _UserDataAccess.getHashedPasswordUserName(user.UserId);
                if (prevPassword == null)
                    throw new ValidationException("this user not found or connection problem");
              if(!HashHelper.VerifyPassword(user.PrevPassword, prevPassword))
                {
                    return false;
                }
                user.Password=HashHelper.getHashedPassword(user.Password);
              return await _UserDataAccess.UpdatePassword(user);
              
            }
            catch (Exception e)
            {
                throw;
            }

        }
        public async Task<bool> UpdatePermissions(UserUpdatePermissionDTO user)
        {
            if (user.UserId<1)
                throw new ValidationException("Id must be greater than 0");
            return await _UserDataAccess.UpdatePermissions(user)?true:throw new NotFoundException("User not found");
        }
        public async Task<bool> Delete(int UserId)
        {
            if (UserId<1)
                throw new ValidationException("Id must be greater than 0");
            return await _UserDataAccess.Delete(UserId) ? true : throw new NotFoundException("User not found");
        }

    }
}
