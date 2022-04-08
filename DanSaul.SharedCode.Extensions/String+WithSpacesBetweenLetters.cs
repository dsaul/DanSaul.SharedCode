using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedCode
{
	public static class String_WithSpacesBetweenLetters
	{
		public static string WithSpacesBetweenLetters(this string str) {

			return str.Aggregate(string.Empty, (c, i) => c + i + ' ');

		}
	}
}
