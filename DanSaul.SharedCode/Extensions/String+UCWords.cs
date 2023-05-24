using System.Text;

namespace DanSaul.SharedCode.Extensions
{
	public static class String_UCWords
	{
		public static string UCWords(this string theString)
		{
			StringBuilder output = new StringBuilder();
			string[] pieces = theString.Split(' ');
			foreach (string piece in pieces)
			{
				char[] theChars = piece.ToCharArray();
				theChars[0] = char.ToUpper(theChars[0]);
				output.Append(' ');
				output.Append(new string(theChars));
			}

			return output.ToString();

		}
	}
}
