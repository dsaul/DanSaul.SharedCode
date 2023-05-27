// (c) 2023 Dan Saul
using System.Text;
using DanSaul.SharedCode.Extensions;

namespace DanSaul.SharedCode.Utility
{
    public static class BadPhoneHash
    {
        public static string CreateBadPhoneHash(string input, int length = 6)
        {
            // Use input string to calculate MD5 hash
            using System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            // Convert the byte array to hexadecimal string
            StringBuilder sb = new();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }


            string md5str = sb.ToString();
            md5str = md5str.ToLower();

            Dictionary<string, string> map = new()
            {
                { @"a", @"2" },
                { @"b", @"2" },
                { @"c", @"2" },
                { @"d", @"3" },
                { @"e", @"3" },
                { @"f", @"3" },
                { @"g", @"4" },
                { @"h", @"4" },
                { @"i", @"4" },
                { @"j", @"5" },
                { @"k", @"5" },
                { @"l", @"5" },
                { @"m", @"6" },
                { @"n", @"6" },
                { @"o", @"6" },
                { @"p", @"7" },
                { @"q", @"7" },
                { @"r", @"7" },
                { @"s", @"7" },
                { @"t", @"8" },
                { @"u", @"8" },
                { @"v", @"8" },
                { @"w", @"9" },
                { @"x", @"9" },
                { @"y", @"9" },
                { @"z", @"9" }
            };

            return md5str.MapReplace(map)[..length];
        }
    }
}
