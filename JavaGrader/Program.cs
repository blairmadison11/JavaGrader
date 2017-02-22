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
				if (File.Exists("input.txt"))
				{
					jdk.Run(project, Path.GetFullPath("input.txt"));
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
