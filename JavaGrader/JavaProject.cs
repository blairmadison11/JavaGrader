using System;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace JavaGrader
{
	class JavaProject
	{
		private const string MAIN_METHOD_PATTERN = "void\\s+main\\s*\\(\\s*String", PACKAGE_DECLARATION_PATTERN = "package\\s+([^\\s;]+)";

		private string myMainFileName = "", myQualifiedMainClass = "", myWorkingPath;
		private bool myFoundMainClassFlag;

		public JavaProject(string userName)
		{
			myFoundMainClassFlag = false;
			myWorkingPath = Path.Combine(Directory.GetCurrentDirectory(), userName);
		}

		public void ExtractArchive()
		{
			string[] zipFiles = Directory.GetFiles(myWorkingPath, "*.zip", SearchOption.TopDirectoryOnly);
			if (zipFiles.Length == 0)
			{
				Console.WriteLine("[No archive found]");
			}
			else
			{
				Console.WriteLine("[Opening archive]");
				try
				{
					//ZipFile.ExtractToDirectory(zipFiles[0], myWorkingPath);

					using (ZipArchive zip = ZipFile.OpenRead(zipFiles[0]))
					{
						foreach (ZipArchiveEntry entry in zip.Entries)
						{
							if (!entry.FullName.Contains("__") && entry.Name.EndsWith(".java"))
							{
								Console.WriteLine("Extracting file: {0}", entry.Name);
								entry.ExtractToFile(Path.Combine(myWorkingPath, entry.Name));
							}
						}
					}
				}
				catch
				{
					Console.WriteLine("[Skipping archive extraction]");
				}
			}
		}

		private string SanitizeFileName(string str)
		{
			string regexSearch = new string(Path.GetInvalidFileNameChars());
			Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
			return r.Replace(str, "");
		}

		private string SanitizePathName(string str)
		{
			string regexSearch = new string(Path.GetInvalidPathChars());
			Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
			return r.Replace(str, "");
		}

		public void LoadAndCanonicalize()
		{
			string[] javaFileNames = Directory.GetFiles(myWorkingPath, "*.java", SearchOption.AllDirectories);
			foreach (string javaFileName in javaFileNames)
			{
				ProcessFile(javaFileName);
			}

			if (!myFoundMainClassFlag)
			{
				throw new InvalidProjectException("[Error locating Java main file]");
			}
		}

		private void ProcessFile(string fileName)
		{
			try
			{
				string targetPath, newFileName, packageName = "", text = File.ReadAllText(fileName);

				Match packageMatch = Regex.Match(text, PACKAGE_DECLARATION_PATTERN);
				if (packageMatch.Success)
				{
					packageName = packageMatch.Groups[1].Value;
					targetPath = Path.Combine(myWorkingPath, packageName.Replace('.', '\\'));
				}
				else
				{
					targetPath = myWorkingPath;
				}

				newFileName = Path.Combine(targetPath, Path.GetFileName(fileName));
				if (!File.Exists(newFileName))
				{
					Directory.CreateDirectory(targetPath);
					File.Copy(fileName, newFileName);
				}

				if (Regex.IsMatch(text, MAIN_METHOD_PATTERN))
				{
					string mainClassName = Path.GetFileNameWithoutExtension(newFileName);

					if (myFoundMainClassFlag)
					{
						Console.WriteLine("[WARNING: Found multiple classes with main method]");
						if (mainClassName.ToLower() != "main")
						{
							return;
						}
					}

					myMainFileName = newFileName;

					if (packageName == "")
					{
						myQualifiedMainClass = mainClassName;
					}
					else
					{
						myQualifiedMainClass = string.Format("{0}.{1}", packageName, mainClassName);
					}
					myFoundMainClassFlag = true;
				}
			}
			catch
			{
				Console.WriteLine("[Error attempting to process file]");
			}
		}

		public void DeleteClassFiles()
		{
			string[] classFiles = Directory.GetFiles(myWorkingPath, "*.class", SearchOption.AllDirectories);
			if (classFiles.Length > 0)
			{
				Console.WriteLine("[Deleting class files]");
				foreach (string classFile in classFiles)
				{
					try
					{
						File.Delete(classFile);
					}
					catch { }
				}
			}
		}

		public string MainFileName
		{
			get
			{
				return myMainFileName;
			}
		}

		public string QualifiedMainClass
		{
			get
			{
				return myQualifiedMainClass;
			}
		}

		public string WorkingPath
		{
			get
			{
				return myWorkingPath;
			}
		}
	}

	class InvalidProjectException : Exception
	{
		public InvalidProjectException(string msg) : base(msg) { }
	}
}
