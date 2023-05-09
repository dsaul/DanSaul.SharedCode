using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DanSaul.SharedCode.Extensions
{
	public static class String_LaTeXEscape
	{
		public static string LaTeXEscape(this string input) {

			if (input == null)
				throw new ArgumentNullException(nameof(input));

			// Map the characters to escape to their escaped values. The list is derived
			// from http://www.cespedes.org/blog/85/how-to-escape-latex-special-characters
			Dictionary<string, string> map = new Dictionary<string, string>();
			map.Add(@"{", @"\{");
			map.Add(@"}", @"\}");
			map.Add(@"\", @"\textbackslash{}");
			map.Add(@"#", @"\#");
			map.Add(@"$", @"\$");
			map.Add(@"%", @"\%");
			map.Add(@"&", @"\&");
			map.Add(@"^", @"\textasciicircum{}");
			map.Add(@"_", @"\_");
			map.Add(@"~", @"\textasciitilde{}");
			map.Add("\u2013", @"\--");
			map.Add("\u2014", @"\---");
			map.Add(@" ", @"~");
			map.Add(@"\t", @"\qquad{}");
			map.Add(@"\r\n", @"\newline{}");
			map.Add(@"\n", @"\newline{}");
			map.Add("\u007F", @"");
			map.Add("\u00D7", @"x");
			return input.MapReplace(map);
		}
	}
}
