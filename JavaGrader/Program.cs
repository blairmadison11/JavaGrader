using System;
using System.IO;
using System.Text;

namespace JavaGrader
{
	class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: JavaGrader.exe [folder_name]");
                return;
            }

			try
			{
				Console.InputEncoding = Encoding.UTF8;
				Console.OutputEncoding = Encoding.UTF8;

				JDK jdk = new JDK();
				jdk.LoadBinaries();

				Console.WriteLine("[Extracting]");
				JavaProject project = new JavaProject(args[0]);
				project.ExtractArchive();
				project.LoadAndCanonicalize();
				project.DeleteClassFiles();

				Console.WriteLine("[Compiling]");
				jdk.Compile(project);

				Console.WriteLine("[Running]");
				GradingInput input = new GradingInput();
				if (input.Exists)
				{
					while (input.HasNext)
					{
						Console.WriteLine(string.Format("TEST INPUT #{0} (press enter)", input.Index));
						Console.ReadLine();
						jdk.Run(project, input.Next);
					}
				}
				else
				{
					jdk.Run(project);
				}
			}
			catch (InvalidProjectException e)
			{
				Console.WriteLine(e.Message);
			}
        }
	}	
}
