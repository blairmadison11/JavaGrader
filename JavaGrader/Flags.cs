using System.Collections.Generic;

namespace JavaGrader
{
	class Flags
	{
		enum Flag { MANUAL_MODE, COMPILE_ONLY, UTF8_MODE }

		private List<Flag> myFlags;
		private bool myGradingFolderFlag = false, myInvalidFlagsFlag = false;
		private string myGradingFolder = "";

		public Flags(string[] args)
		{
			myFlags = new List<Flag>();
			foreach (string arg in args)
			{
				if (arg.StartsWith("-"))
				{
					ProcessFlag(arg.Substring(1));
				}
				else
				{
					myGradingFolder = arg;
					myGradingFolderFlag = true;
				}
			}
		}

		public bool ManualMode => myFlags.Contains(Flag.MANUAL_MODE);
		public bool CompileOnly => myFlags.Contains(Flag.COMPILE_ONLY);
		public bool Utf8Mode => myFlags.Contains(Flag.UTF8_MODE);
		public string GradingFolder => myGradingFolder;
		public bool AreValid => myGradingFolderFlag && !myInvalidFlagsFlag;

		private void ProcessFlag(string flag)
		{
			foreach(char c in flag.ToCharArray())
			{
				switch(c)
				{
					case 'm':
						myFlags.Add(Flag.MANUAL_MODE);
						break;
					case 'c':
						myFlags.Add(Flag.COMPILE_ONLY);
						break;
					case '8':
						myFlags.Add(Flag.UTF8_MODE);
						break;
					default:
						myInvalidFlagsFlag = true;
						break;
				}
			}
		}
	}
}
