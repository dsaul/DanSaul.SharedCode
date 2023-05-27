// (c) 2023 Dan Saul
using System.Text;

namespace DanSaul.SharedCode.Asterisk.AsteriskINI
{
	public record Entry
	{
		public string? Key { get; init; } = null;
		public string? Value { get; init; } = null;
		public bool Disabled { get; init; } = false;
		public string? Comment { get; init; } = null;
		public string Delimiter { get; init; } = " = ";

		public void Generate(StringBuilder sb, bool forceDisabled = false)
		{
			if (string.IsNullOrWhiteSpace(Key))
				return;

			string strDisabled = string.Empty;
			if (Disabled || forceDisabled)
				strDisabled = "; ";

			string strValue = string.Empty;
			if (!string.IsNullOrWhiteSpace(Value))
				strValue = Value.Trim();

			string strComment = string.Empty;
			if (!string.IsNullOrWhiteSpace(Comment))
				strComment = " ; " + Comment;

			sb.AppendLine($"{strDisabled}{Key.Trim()}{Delimiter}{strValue}{strComment}");
		}
	}
}
