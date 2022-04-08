using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedCode
{
	public static class String_Truncate
	{
		public static string Truncate(this string str, int numChars, bool useWordBoundary = true, bool useLatex = false) {

			// If string is less then the character limit.
			if (str.Length <= numChars) {
				return str;
			}

			// Chop to character limit.
			string chopped = str.Substring(0, numChars - 1);

			if (false == useWordBoundary) {
				if (useLatex) {
					return $"{chopped}\\ldots";
				} else {
					return $"{chopped}\u2026";
				}
			}


			int lastSpace = chopped.LastIndexOf(' ');
			if (lastSpace == -1) {
				if (useLatex) {
					return $"{chopped}\\ldots";
				} else {
					return $"{chopped}\u2026";
				}
			}

			chopped = chopped.Substring(0, lastSpace);
			if (useLatex) {
				return $"{chopped}\\ldots";
			} else {
				return $"{chopped}\u2026";
			}

		}
	}
}
