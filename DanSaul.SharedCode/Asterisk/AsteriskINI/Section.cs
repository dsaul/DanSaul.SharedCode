using System.Text;

namespace DanSaul.SharedCode.Asterisk.AsteriskINI
{
	public record Section
	{
		public string? Name { get; init; } = null;
		public bool IsTemplate { get; init; } = false;
		public string? UsesTemplate { get; init; } = null;
		public bool Disabled { get; init; } = false;
		public string? Comment { get; init; } = null;
		public IEnumerable<Entry> Entries { get; init; } = Array.Empty<Entry>();

		public void Generate(StringBuilder sb)
		{
			if (string.IsNullOrWhiteSpace(Name))
				return;

			string strDisabled = string.Empty;
			if (Disabled)
				strDisabled = "; ";

			string strComment = string.Empty;
			if (!string.IsNullOrWhiteSpace(Comment))
				strComment = " ; " + Comment;

			string strTemplate = string.Empty;
			if (IsTemplate)
				strTemplate = "(!)";
			else if (!string.IsNullOrWhiteSpace(UsesTemplate))
				strTemplate = $"({UsesTemplate.Trim()})";

			sb.AppendLine($"{strDisabled}[{Name.Trim()}]{strTemplate}{strComment}");

			foreach (Entry entry in Entries)
				entry.Generate(sb, Disabled);

			sb.AppendLine();
		}
	}
}
