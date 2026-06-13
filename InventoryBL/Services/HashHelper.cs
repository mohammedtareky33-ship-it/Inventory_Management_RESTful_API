using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
namespace InventoryBL.Services
{
    public static class HashHelper
    {
        public static string getHashedPassword(string password)
        {
         return BCrypt.Net.BCrypt.HashPassword(password);
        }
        public static bool VerifyPassword(string password,string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}
