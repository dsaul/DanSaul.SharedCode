using System;
using System.Collections.Generic;
using System.Text;
using SharedCode;

namespace DanSaul.SharedCode
{
	public static class BadPhoneHash
	{
		public static string CreateBadPhoneHash(string input, int length = 6) {
			// Use input string to calculate MD5 hash
			using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create()) {
				byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
				byte[] hashBytes = md5.ComputeHash(inputBytes);

				// Convert the byte array to hexadecimal string
				StringBuilder sb = new StringBuilder();
				for (int i = 0; i < hashBytes.Length; i++) {
					sb.Append(hashBytes[i].ToString("X2"));
				}

				
				string md5str = sb.ToString();
				md5str = md5str.ToLower();

				Dictionary<string, string> map = new Dictionary<string, string>();
				map.Add(@"a", @"2");
				map.Add(@"b", @"2");
				map.Add(@"c", @"2");
				map.Add(@"d", @"3");
				map.Add(@"e", @"3");
				map.Add(@"f", @"3");
				map.Add(@"g", @"4");
				map.Add(@"h", @"4");
				map.Add(@"i", @"4");
				map.Add(@"j", @"5");
				map.Add(@"k", @"5");
				map.Add(@"l", @"5");
				map.Add(@"m", @"6");
				map.Add(@"n", @"6");
				map.Add(@"o", @"6");
				map.Add(@"p", @"7");
				map.Add(@"q", @"7");
				map.Add(@"r", @"7");
				map.Add(@"s", @"7");
				map.Add(@"t", @"8");
				map.Add(@"u", @"8");
				map.Add(@"v", @"8");
				map.Add(@"w", @"9");
				map.Add(@"x", @"9");
				map.Add(@"y", @"9");
				map.Add(@"z", @"9");

				return md5str.MapReplace(map).Substring(0, length);
			}
		}
	}
}
