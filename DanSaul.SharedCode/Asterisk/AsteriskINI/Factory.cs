// (c) 2023 Dan Saul
using System.Text;

namespace DanSaul.SharedCode.Asterisk.AsteriskINI
{
	public static class Factory
	{
		public static string Generate(AsteriskINIFile file)
		{
			StringBuilder sb = new();
			file.Generate(sb);
			return sb.ToString();
		}
	}
}
