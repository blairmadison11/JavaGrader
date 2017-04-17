using System;

namespace JavaGrader
{
    class Program
    {
        static void Main(string[] args)
        {
			Flags flags = new Flags(args);
			if (flags.AreValid)
			{
				try
				{
					JDK jdk = new JDK();
					jdk.LoadBinaries();

					Console.WriteLine("[Extracting]");
					JavaProject project = new JavaProject(flags.GradingFolder);
					project.ExtractArchive();

					Console.WriteLine("[Analyzing]");
					project.LoadAndCanonicalize();
					project.DeleteClassFiles();

					Console.WriteLine("[Compiling]");
					jdk.Compile(project);

					if (!flags.CompileOnly)
					{
						Console.WriteLine("[Running]");
						GradingInput input = new GradingInput();
						if (!flags.ManualMode && input.Exists)
						{
							while (input.HasNext)
							{
								Console.WriteLine(string.Format("[Test Input #{0}]\n(press enter)", input.Index));
								Console.ReadLine();
								jdk.Run(project, flags, input.Next);
							}
						}
						else
						{
							jdk.Run(project, flags);
						}
					}
				}
				catch (InvalidProjectException e)
				{
					Console.WriteLine(e.Message);
				}
			}
			else
			{
				Console.WriteLine("Usage: JavaGrader.exe [-cm8] <folder_name>");
			}
        }
	}	
}
