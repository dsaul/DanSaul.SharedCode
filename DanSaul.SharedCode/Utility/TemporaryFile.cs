// (c) 2023 Dan Saul
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Textitude
{
	public class TemporaryFile : IDisposable
	{
		public string FilePath { get; init; }

		public TemporaryFile(Stream copyStream): this()
		{
			using FileStream writeStream = OpenWrite();
			copyStream.CopyTo(writeStream);
		}

		public TemporaryFile() : this(Path.GetTempPath())
		{
		}

		public TemporaryFile(string directory)
		{
			FilePath = Path.Combine(directory, Path.GetRandomFileName());
			File.Create(FilePath).Dispose();

		}

		~TemporaryFile()
		{
			Delete();
		}

		public void Dispose()
		{
			Delete();
			GC.SuppressFinalize(this);
		}


		private void Delete()
		{
			if (FilePath != null)
				File.Delete(FilePath);
		}


		public FileStream OpenWrite()
		{
			return File.OpenWrite(FilePath);
		}

		public FileStream OpenRead()
		{
			return File.OpenRead(FilePath);
		}
	}
}
