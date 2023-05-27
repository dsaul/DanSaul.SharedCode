// (c) 2023 Dan Saul
namespace DanSaul.SharedCode.Extensions
{
	public static class String_ReplaceEvenOdd
	{
		public static string ReplaceEvenOdd(this string haystack, string needle, string replacementEven, string replacementOdd) {
			string[] split = haystack.Split(new[] { needle }, StringSplitOptions.None);
			string result = string.Empty;
			for (int i = 0; i < split.Length; i++) {
				result += split[i];
				if (i < split.Length - 1)
					result += (i + 1) % 2 == 0 ? replacementEven : replacementOdd;
			}
			return result;
		}
	}
}
