using System;
using System.Security.Cryptography;
using System.Text;

namespace Corellian.Versio
{
    public static class Utils
    {
        public static string ComputeHash(string value)
        {
            using var sha256 = SHA256.Create();
            return BitConverter.ToString(sha256.ComputeHash(Encoding.ASCII.GetBytes(value))).Replace("-", string.Empty).ToLowerInvariant();
        }
    }
}
