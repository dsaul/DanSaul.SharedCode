using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SharedCode
{
	public static class String_MapReplace
	{
		// Dictionary<string, string> map = new Dictionary<string, string>();
		// map.Add("cat", "dog");
		// map.Add("dog", "goat");
		// map.Add("goat", "cat");

		// string mod = StringMapReplace("I have a cat, a dog, and a goat.", map);
		// Log.Debug(mod);
		public static string MapReplace(this string words, Dictionary<string, string> map) {

			if (words == null)
				throw new ArgumentNullException(nameof(words));
			if (map == null)
				throw new ArgumentNullException(nameof(map));

			StringBuilder pattern = new();

			List<string> keys = map.Keys.ToList();
			for (int i=0; i<keys.Count; i++) {
				pattern.Append(Regex.Escape(keys[i]));
				if (i != keys.Count-1) {
					pattern.Append('|');
				}
			}

			string Replacer(Match match) {
				return map[match.Value];
			}

			MatchEvaluator evaluator = new MatchEvaluator(Replacer);

			return Regex.Replace(words, pattern.ToString(), evaluator,
										 RegexOptions.IgnoreCase | RegexOptions.Multiline,
										 TimeSpan.FromSeconds(5));

		}
	}
}
