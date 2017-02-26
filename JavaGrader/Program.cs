using System;
using System.Text;

namespace JavaGrader
{
	class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: JavaGrader.exe [-c] [-m] <folder_name>");
                return;
            }

			try
			{
				Console.InputEncoding = Encoding.UTF8;
				Console.OutputEncoding = Encoding.UTF8;

				Flags flags = new Flags(args);

				JDK jdk = new JDK();
				jdk.LoadBinaries();

				Console.WriteLine("[Extracting]");
				JavaProject project = new JavaProject(flags.GradingFolder);
				project.ExtractArchive();
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
							jdk.Run(project, input.Next);
						}
					}
					else
					{
						jdk.Run(project);
					}
				}
			}
			catch (InvalidProjectException e)
			{
				Console.WriteLine(e.Message);
			}
        }
	}	
}
