// (c) 2023 Dan Saul
namespace DanSaul.SharedCode.Extensions
{
	public static class String_WithSpacesBetweenLetters
	{
		public static string WithSpacesBetweenLetters(this string str) {

			return str.Aggregate(string.Empty, (c, i) => c + i + ' ');

		}
	}
}
