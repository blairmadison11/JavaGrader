using System;
using System.Diagnostics;
using System.IO;

namespace JavaGrader
{
	class JDK
	{
		private const string DATA_FILE_NAME = "javapath.txt";

		private string myBinPath;
		private bool myValidFlag;

		public JDK()
		{
			myValidFlag = false;
		}

		public string JavaCompiler => string.Format("{0}\\{1}", myBinPath, "javac.exe");

		public string JavaRuntime => string.Format("{0}\\{1}", myBinPath, "java.exe");

		public void LoadBinaries()
		{
			if (!LoadFromFile())
			{
				SearchForBinPath();
				SaveToFile();
			}

			if (!myValidFlag)
			{
				throw new InvalidProjectException("[Unable to find JDK binaries!Please install the JDK on your system and try again.]");
			}
		}

		private bool LoadFromFile()
		{
			try
			{
				if (File.Exists(DATA_FILE_NAME))
				{
					myBinPath = File.ReadAllText(DATA_FILE_NAME);
					myValidFlag = true;
					return true;
				}
			} catch { }
			return false;
		}

		private void SaveToFile()
		{
			if (myValidFlag)
			{
				StreamWriter sw = new StreamWriter(DATA_FILE_NAME);
				sw.Write(myBinPath);
				sw.Close();
			}
		}

		private void SearchForBinPath()
		{
			string[] progFileDirs = { Environment.ExpandEnvironmentVariables("%ProgramW6432%"), Environment.ExpandEnvironmentVariables("%ProgramFiles(x86)%") };
			foreach (string progFileDir in progFileDirs)
			{
				string[] dirs = Directory.GetDirectories(progFileDir, "Java", SearchOption.TopDirectoryOnly);
				foreach (string dir in dirs)
				{
					string[] files = Directory.GetFiles(dir, "javac.exe", SearchOption.AllDirectories);
					if (files.Length > 0)
					{
						myValidFlag = true;
						DateTime mostRecent = DateTime.MinValue;
						foreach (string file in files)
						{
							DateTime fileCreation = File.GetCreationTime(file);
							if (fileCreation.CompareTo(mostRecent) > 0)
							{
								mostRecent = fileCreation;
								myBinPath = Path.GetDirectoryName(file);
							}
						}
					}
				}
			}
		}

		public void Compile(JavaProject project)
		{
			string storedCurDir = Directory.GetCurrentDirectory();
			int exitCode = 1;
			try
			{
				Console.WriteLine("[Compiling]");
				Directory.SetCurrentDirectory(project.WorkingPath);
				ProcessStartInfo javacStart = new ProcessStartInfo(this.JavaCompiler, project.MainFileName);
				javacStart.UseShellExecute = false;
				Process javac = Process.Start(javacStart);
				javac.WaitForExit();
				exitCode = javac.ExitCode;
			}
			catch { }
			finally
			{
				Directory.SetCurrentDirectory(storedCurDir);
			}

			if (exitCode != 0)
			{
				throw new InvalidProjectException("[Error compiling project]");
			}
		}

		public void Run(JavaProject project) => Run(project, "");

		public void Run(JavaProject project, string parameters)
		{
			string storedCurDir = Directory.GetCurrentDirectory();
			int exitCode = 1;
			try
			{
				Console.WriteLine("[Running]");
				Directory.SetCurrentDirectory(project.WorkingPath);
				string javaParams;
				if (parameters.Trim() == "")
				{
					javaParams = project.QualifiedMainClass;
				}
				else
				{
					javaParams = string.Format("{0} {1}", project.QualifiedMainClass, parameters);
				}
				ProcessStartInfo javaStart = new ProcessStartInfo(this.JavaRuntime, javaParams);
				javaStart.UseShellExecute = false;
				Process java = Process.Start(javaStart);
				java.WaitForExit();
				exitCode = java.ExitCode;
			}
			catch { }
			finally
			{
				Directory.SetCurrentDirectory(storedCurDir);
			}

			if (exitCode != 0)
			{
				throw new InvalidProjectException("[Error running project]");
			}
		}
	}
}
