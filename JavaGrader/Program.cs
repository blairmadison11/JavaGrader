using System;
using System.IO;

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
				JDK jdk = new JDK();
				jdk.LoadBinaries();

				JavaProject project = new JavaProject(args[0]);
				project.ExtractArchive();
				project.LoadAndCanonicalize();
				project.DeleteClassFiles();

				jdk.Compile(project);

				GradingInput input = new GradingInput();
				if (input.Exists)
				{
					Console.WriteLine(string.Format("Press enter to run test input #{0}\n"), input.Index);
					while (input.HasNext)
					{
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
