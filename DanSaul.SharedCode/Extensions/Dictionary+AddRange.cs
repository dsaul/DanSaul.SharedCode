// (c) 2023 Dan Saul
namespace DanSaul.SharedCode.Extensions
{
	public static class Dictionary_AddRange
	{
		public static void AddRange<T, S>(
			this Dictionary<T, S> source, 
			Dictionary<T, S> collection, 
			bool overwrite = true
			) where T : notnull
		{
			if (source == null)
				throw new ArgumentNullException(nameof(source));
			if (collection == null)
				throw new ArgumentNullException(nameof(collection));

			foreach (var item in collection)
			{
				if (!source.ContainsKey(item.Key))
				{
					source.Add(item.Key, item.Value);
				}
				else
				{
					if (overwrite)
					{
						source[item.Key] = item.Value;
					}
				}
			}
		}
	}
}
