using InventoryManagemetRESTFUL_API.Authoraization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace InventoryManagemetRESTFUL_API.Authorization
{
    public class PermissionsPolicyProvider:DefaultAuthorizationPolicyProvider
    {
            public PermissionsPolicyProvider(IOptions<AuthorizationOptions> options):base(options)
        {
      
        }
        public override Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            if (policyName.StartsWith("Permissions:"))
            {
                var value = policyName.Replace("Permissions:", "");
                var permissionNames = value.Split(',');

                

                int[] Permissions=permissionNames.Select(x => (int)UserPermissions.PermissionsMapByName[x]).ToArray();

                var policy = new AuthorizationPolicyBuilder().AddRequirements(new PermissionsRequirement(Permissions)).Build();
                return Task.FromResult<AuthorizationPolicy?>(policy);
            }
            return base.GetPolicyAsync(policyName);
        }
        
    }
}
