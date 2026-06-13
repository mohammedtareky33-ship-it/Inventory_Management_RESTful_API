using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace InventoryManagemetRESTFUL_API.Authoraization
{
    public class PermissionsHandler: AuthorizationHandler<PermissionsRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionsRequirement requirement)
        {
            int userPermissions = 0;
            if (int.TryParse(context.User.FindFirstValue("Permissions"), out int value))
              userPermissions  = value;
            else
            {
              return Task.CompletedTask;
            }

            foreach (var per in requirement.Permissions)
            {
                if (UserPermissions.HasPermission(userPermissions, per))
                { context.Succeed(requirement);
                    return Task.CompletedTask;
                }
            }
           
            return Task.CompletedTask;
        }
    }
}
