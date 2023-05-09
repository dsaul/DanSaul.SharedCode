using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DanSaul.SharedCode.Extensions
{
	public static class String_Right
	{
        public static string Right(this string input, int length)
        {
            if (length >= input.Length)
            {
                return input;
            }
            else
            {
                return input.Substring(input.Length - length);
            }
        }
    }
}
