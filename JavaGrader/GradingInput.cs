using System;
using System.Text;
using System.IO;

namespace JavaGrader
{
	class GradingInput
	{
		private string[] myInputFiles;
		private int myCurrentIndex;

		public GradingInput()
		{
			myInputFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "input*.txt", SearchOption.TopDirectoryOnly);
			Array.Sort(myInputFiles);
			myCurrentIndex = 0;
		}

		public bool Exists => myInputFiles.Length > 0;

		public bool HasNext => myCurrentIndex < myInputFiles.Length;

		public StreamReader Next => new StreamReader(new FileStream(myInputFiles[myCurrentIndex++], FileMode.Open), Encoding.UTF8);

		public int Index => myCurrentIndex + 1;
	}
}
