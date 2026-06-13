using Microsoft.AspNetCore.Authorization;

namespace InventoryManagemetRESTFUL_API.Authoraization
{
    public class PermissionsRequirement : IAuthorizationRequirement
    {
        public int[] Permissions { get; }
        public PermissionsRequirement(int[] permissions) { 
        Permissions = permissions;
        }
    }
}
