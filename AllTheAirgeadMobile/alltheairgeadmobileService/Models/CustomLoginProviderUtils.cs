using System.Security.Cryptography;

namespace alltheairgeadmobileService.Models
{
    public class CustomLoginProviderUtils
    {
        public static byte[] hash(string plaintext)
        {
            SHA512Cng hashFunc = new SHA512Cng();
            byte[] plainBytes = System.Text.Encoding.ASCII.GetBytes(plaintext);
            byte[] toHash = new byte[plainBytes.Length];
            plainBytes.CopyTo(toHash, 0);
            return hashFunc.ComputeHash(toHash);
        }

        public static bool slowEquals(byte[] a, byte[] b)
        {
            int diff = a.Length ^ b.Length;
            for (int i = 0; i < a.Length && i < b.Length; i++)
            {
                diff |= a[i] ^ b[i];
            }
            return diff == 0;
        }
    }
}