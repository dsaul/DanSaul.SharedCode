using Serilog;

namespace DanSaul.SharedCode.Extensions
{
	public static class String_FindStringsBetweenStrings
	{
		public static IEnumerable<string> FindStringsBetweenStrings(
			this string haystack, 
			string start, 
			string end, 
			int startIdx = 0, 
			bool includeStart = false, 
			int maxIterations = 20000
			)
		{
			haystack = haystack.ReplaceLineEndings();
			end = end.ReplaceLineEndings();

			for (int i = 0; i < maxIterations; i++) // Unlikely to have more than 20k trunks, if so we can update this.
			{
				int subStrStart = haystack.IndexOf(start, startIdx) + (includeStart ? 0 : start.Length);
				if (subStrStart == -1)
					yield break;

				int subStrEnd = haystack.IndexOf(end, subStrStart + (includeStart ? start.Length : 0));
				if (subStrEnd == -1)
					yield break;

				startIdx = subStrEnd + end.Length;

				yield return haystack.Substring(subStrStart, subStrEnd - subStrStart);
			}

			Log.Warning("Reached max {Iterations} iterations!", maxIterations);
		}
	}
}
