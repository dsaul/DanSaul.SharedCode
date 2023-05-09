
namespace DanSaul.SharedCode
{
	public static class ISO8601Compare
	{
		public static int Compare(string iso1, string iso2)
		{
			DateTime dt1 = DateTime.Parse(iso1, Culture.DevelopmentCulture);
			DateTime dt2 = DateTime.Parse(iso2, Culture.DevelopmentCulture);

			return dt1.CompareTo(dt2);
		}

	}
}
