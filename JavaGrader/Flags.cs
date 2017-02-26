namespace JavaGrader
{
	class Flags
	{
		private bool myManualModeFlag = false, myCompileOnlyFlag = false;
		private string myGradingFolder = "";

		public Flags(string[] args)
		{
			foreach (string arg in args)
			{
				if (arg.StartsWith("-"))
				{
					ProcessFlag(arg.Substring(1));
				}
				else
				{
					myGradingFolder = arg;
				}
			}
		}

		public bool ManualMode => myManualModeFlag;
		public bool CompileOnly => myCompileOnlyFlag;
		public string GradingFolder => myGradingFolder;

		private void ProcessFlag(string flag)
		{
			foreach(char c in flag.ToCharArray())
			{
				switch(c)
				{
					case 'm':
						myManualModeFlag = true;
						break;
					case 'c':
						myCompileOnlyFlag = true;
						break;
				}
			}
		}
	}
}
