using System;
using System.Diagnostics;
using System.IO;
using System.Threading;


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
				throw new InvalidProjectException("Error: Unable to find JDK binaries. Please install the JDK on your system and try again.");
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
				throw new InvalidProjectException("Error: Project compilation failed.");
			}
		}

		public void Run(JavaProject project, Flags flags) => Run(project, flags, null);

		public void Run(JavaProject project, Flags flags, StreamReader input)
		{
			string storedCurDir = Directory.GetCurrentDirectory(), inputLine = "", errorMsg = "";
			int exitCode = 1;
			try
			{
				Directory.SetCurrentDirectory(project.WorkingPath);
				String javaParams;
				if (flags.Utf8Mode)
				{
					javaParams = string.Format("-Dfile.encoding=utf8 {0}", project.QualifiedMainClass);
				}
				else
				{
					javaParams = project.QualifiedMainClass;
				}
				ProcessStartInfo javaStart = new ProcessStartInfo(this.JavaRuntime, javaParams);
				javaStart.UseShellExecute = false;
				javaStart.RedirectStandardError = true;
				Process java;

				if (input != null)
				{
					javaStart.RedirectStandardInput = true;
					java = Process.Start(javaStart);
                    java.StandardInput.Flush();

					// *** MAJOR HACK ***
					// For some reason, the first input when using UTF-8 mode
					// is "corrupt" (treated as bad input by the receiving program).
					// So we give it a dummy input before sending the real input.
					//
					// Obviously, this doesn't work for all programs.
					// This really shouldn't be here.
					// Why is this here?
					// This is bad, very bad. Please delete this.
					/*
                    if (utf8Mode)
					{
						java.StandardInput.WriteLine("a");
					}
                    */

					while (!java.HasExited && !input.EndOfStream)
					{
						Thread.Sleep(200);
						inputLine = input.ReadLine();
						Console.WriteLine(inputLine);
						java.StandardInput.WriteLine(inputLine);
						/*
						foreach (ProcessThread thread in java.Threads)
						{
							if (thread.ThreadState == System.Diagnostics.ThreadState.Wait && thread.WaitReason == ThreadWaitReason.UserRequest)
							{
								inputLine = input.ReadLine();
								Console.WriteLine(inputLine);
								java.StandardInput.WriteLine(inputLine);
							}
						}
						*/
					}
				}
				else
				{
					java = Process.Start(javaStart);
				}

				java.WaitForExit();

				errorMsg = java.StandardError.ReadToEnd();
				exitCode = java.ExitCode;
			}
			catch (Exception e) { Console.WriteLine(e.Message); }
			finally
			{
				Directory.SetCurrentDirectory(storedCurDir);
			}

			if (exitCode != 0)
			{
				throw new InvalidProjectException("Runtime Error:\n" + errorMsg);
			}
		}
	}
}
