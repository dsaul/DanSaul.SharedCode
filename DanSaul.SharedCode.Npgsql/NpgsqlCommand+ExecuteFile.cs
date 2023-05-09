using Npgsql;
using System.Reflection;

namespace DanSaul.SharedCode.Npgsql
{
	public static class NpgsqlCommandExecuteFile
	{
		// https://stackoverflow.com/questions/19510685/execute-sql-file-with-npgsql-provider-net
		public static int ExecuteFile(this NpgsqlCommand command, string fileName) {
			return ExecuteFile(command, fileName, System.Text.Encoding.UTF8);
		}

		// https://stackoverflow.com/questions/19510685/execute-sql-file-with-npgsql-provider-net
		public static int ExecuteFile(this NpgsqlCommand command, string fileName, System.Text.Encoding encoding) {
			if (null == command)
				throw new ArgumentNullException(nameof(command));

			string strText = File.ReadAllText(fileName, encoding);
			string withoutComments = SQLUtility.RemoveCommentsFromSQLString(strText, true);
			command.CommandText = withoutComments;
			return command.ExecuteNonQuery();
		}

		public static int ExecuteFileFromResourceSuffix(this NpgsqlCommand command, string resourceSuffix) {
			return ExecuteFileFromResourceSuffix(command, resourceSuffix, Assembly.GetExecutingAssembly());
		}

		public static int ExecuteFileFromResourceSuffix(this NpgsqlCommand command, string resourceSuffix, Assembly assembly) {

			if (assembly == null)
				throw new ArgumentNullException(nameof(assembly));

			string resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith(resourceSuffix, true, Culture.DevelopmentCulture));

			return ExecuteFileFromResourceName(command, resourceName, assembly);
		}

		public static int ExecuteFileFromResourceName(this NpgsqlCommand command, string resourceName) {
			return ExecuteFileFromResourceName(command, resourceName, Assembly.GetExecutingAssembly());
		}

		public static int ExecuteFileFromResourceName(this NpgsqlCommand command, string resourceName, Assembly assembly) {

			if (assembly == null)
				throw new ArgumentNullException(nameof(assembly));
			if (command == null)
				throw new ArgumentNullException(nameof(command));


			using Stream? stream = assembly.GetManifestResourceStream(resourceName);
			if (null == stream)
				return 0;

			using StreamReader reader = new StreamReader(stream);


			string result = reader.ReadToEnd();
			string withoutComments = SQLUtility.RemoveCommentsFromSQLString(result, true);

			command.CommandText = withoutComments;

			return command.ExecuteNonQuery();
		}
	}
}
