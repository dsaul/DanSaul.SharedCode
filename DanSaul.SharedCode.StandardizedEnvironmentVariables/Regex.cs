using System.Text.RegularExpressions;

namespace SharedCode
{
	public static class RegexUtils
	{
		public static System.Text.RegularExpressions.Regex NotLettersNumbersRegex { get; } = new System.Text.RegularExpressions.Regex("[^a-zA-Z0-9]");
		public static System.Text.RegularExpressions.Regex NotLettersNumbersUnderscoreRegex { get; } = new System.Text.RegularExpressions.Regex("[^_a-zA-Z0-9]");
	}
}
