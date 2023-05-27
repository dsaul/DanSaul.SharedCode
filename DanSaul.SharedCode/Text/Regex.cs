// (c) 2023 Dan Saul
using System.Text.RegularExpressions;

namespace DanSaul.SharedCode.Text
{
    public static class RegexUtils
    {
        public static Regex NotLettersNumbersRegex { get; } = new Regex("[^a-zA-Z0-9]");
        public static Regex NotLettersNumbersUnderscoreRegex { get; } = new Regex("[^_a-zA-Z0-9]");
    }
}
