using System.Text.RegularExpressions;

namespace DanSaul.SharedCode
{
	public static class RegexUtils
	{
		public static Regex NotLettersNumbersRegex { get; } = new Regex("[^a-zA-Z0-9]");
		public static Regex NotLettersNumbersUnderscoreRegex { get; } = new Regex("[^_a-zA-Z0-9]");
	}
}
