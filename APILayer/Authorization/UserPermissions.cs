using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagemetRESTFUL_API.Authoraization
{
    public static class UserPermissions
    {
        [Flags]
        public enum enPermissions
        {
            None = 0,

            // Users
            Users = 1,

            // Products
            Products = 2,

            // Inventory (Batches + Stock)
            Inventory = 4,

            // Invoices (Sales + Purchase + Returns)
            Invoices = 8,



            // Reports
            Reports = 16,

            // Full Access
            All =
                Users |
                Products |
                Inventory |
                Invoices |

                Reports
        }

        public static readonly Dictionary<enPermissions, int> PermissionsNumMap =
            new Dictionary<enPermissions, int>
        {
                { enPermissions.None, 0 },  
       { enPermissions.Users, (int)enPermissions.Users },
        { enPermissions.Products, (int)enPermissions.Products },
        { enPermissions.Inventory, (int)enPermissions.Inventory  },
        { enPermissions.Invoices, (int)enPermissions.Invoices  },

        { enPermissions.Reports, (int)enPermissions.Reports  },
        {enPermissions.All, (int)enPermissions.All }
        };
        public static readonly Dictionary<string, enPermissions> PermissionsMapByName =
          new Dictionary<string, enPermissions>
      {
        { enPermissions.None.ToString(), enPermissions.None},
         { enPermissions.Users.ToString(), enPermissions.Users },
        { enPermissions.Products.ToString(), enPermissions.Products },
        { enPermissions.Inventory.ToString(), enPermissions.Inventory  },
        { enPermissions.Invoices.ToString(), enPermissions.Invoices  },

        { enPermissions.Reports.ToString(), enPermissions.Reports  },
        {enPermissions.All.ToString(), enPermissions.All }
      };

        public static List<enPermissions> getPermissionsList(int permission)
        {
            List<enPermissions> permissions = new List<enPermissions>();
            if (permission == (int)enPermissions.All)
            {
                permissions.Add(enPermissions.All);
                return permissions;
            }
            if (permission == 0)
            {
                permissions.Add(enPermissions.None);
                return permissions;
            }
            foreach (var item in PermissionsNumMap)
            {
                if ((permission & (int)item.Key) != 0)
                {
                    permissions.Add(item.Key);
                }
            }


            return permissions;
        }
       public static bool HasPermission(int user_permissions, int permission)
        {
            return (user_permissions & permission) != 0;
        }

    }
}
