// (c) 2023 Dan Saul
using System.Text;

namespace DanSaul.SharedCode.Extensions
{
	public static class String_SplitStringByLineLength
	{
		public static string MaxLineLengthAddPrefix(this string payload, int maxLineLength, string linePrefix = "")
		{
			List<string> lines = new();

			string[] words = payload.Split(' ');

			StringBuilder currentLine = new();

			foreach (string word in words)
			{
				if (currentLine.Length + word.Length + linePrefix.Length <= maxLineLength)
				{
					currentLine.Append(word + " ");
				}
				else
				{
					lines.Add(linePrefix + currentLine.ToString().Trim());
					currentLine.Clear();
					currentLine.Append(word + " ");
				}
			}

			if (currentLine.Length > 0)
			{
				lines.Add(linePrefix + currentLine.ToString().Trim());
			}

			return string.Join('\n', lines);
		}
	}
}
