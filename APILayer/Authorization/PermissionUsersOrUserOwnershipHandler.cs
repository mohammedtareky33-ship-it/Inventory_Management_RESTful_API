using InventoryManagemetRESTFUL_API.Authoraization;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace InventoryManagemetRESTFUL_API.Authorization
{
    public class PermissionUsersOrUserOwnershipHandler:AuthorizationHandler<PermissionUsersOrUserOwnershipRequirement,int>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,PermissionUsersOrUserOwnershipRequirement requirement,int id)
        {
            int usersPermissions = (int)UserPermissions.enPermissions.Users;
            int CurrentUserPermission = int.Parse(context.User.FindFirstValue("Permissions"));
            bool HasPermission=UserPermissions.HasPermission(CurrentUserPermission, usersPermissions);
            if (HasPermission||context.User.FindFirstValue(ClaimTypes.NameIdentifier)==id.ToString())
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}
