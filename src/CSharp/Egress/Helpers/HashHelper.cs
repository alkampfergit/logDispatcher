using System;
using System.Security.Cryptography;
using System.Text;

namespace Egress.Helpers
{
    public static class HashHelper
    {
        public static string ToSha256(this String str)
        {
            using (var provider = new SHA256CryptoServiceProvider())
            {
                var hash = provider.ComputeHash(Encoding.UTF8.GetBytes(str));
                return BitConverter.ToString(hash).Replace("-", "");
            }
        }

        public static string ToSha1(this String str)
        {
            using (var provider = new SHA1CryptoServiceProvider())
            {
                var hash = provider.ComputeHash(Encoding.UTF8.GetBytes(str));
                return BitConverter.ToString(hash).Replace("-", "");
            }
        }
    }
}
