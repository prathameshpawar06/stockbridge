using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace stockbridge_DAL.CacheHelper
{
    public static class CacheHelper
    {
        public static string GenerateCacheKey(object request)
        {
            string serializedRequest = JsonSerializer.Serialize(request);
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(serializedRequest));
                return Convert.ToBase64String(hashBytes);
            }
        }
    }
}
