using CSharpConsole.Models;

namespace CSharpConsole
{
	public class Program
	{
		private static List<DeviceFile> allDeviceFiles = new List<DeviceFile>();

		static void Main(string[] args)
		{
			PrintSource();

			//GenerateDeviceFiles_Approach1();
			//GenerateDeviceFiles_Approach2();
			//GenerateDeviceFiles_Approach3();
			GenerateDeviceFiles_Approach4();
		}

		static void PrintSource()
		{
			List<string> zipFilePaths = GetData();

			Console.WriteLine("Original List");
			Console.WriteLine("-----------------------");
			foreach (string dataItem in zipFilePaths)
				Console.WriteLine(dataItem);

			Console.WriteLine(""); 
		}

		#region Approach 1

		static void GenerateDeviceFiles_Approach1()
		{
			List<string> zipFilePaths = GetData();

			allDeviceFiles.Clear();

			foreach (string zipFilePath in zipFilePaths.Where(x => !x.Contains("/")))
				allDeviceFiles.Add(new DeviceFile { Name = zipFilePath });

			zipFilePaths = zipFilePaths.Where(x => x.Contains("/")).ToList();

			allDeviceFiles.AddRange(GetDeviceFiles(zipFilePaths));

			Console.WriteLine("Device Files List");
			Console.WriteLine("-----------------------");
			GroupDeviceFiles(allDeviceFiles);
		}

		static void GroupDeviceFiles(List<DeviceFile> deviceFiles)
		{
			List<DeviceFile> deviceFileList = new List<DeviceFile>();
			IEnumerable<string> distinctFolderNames = deviceFiles.Select(x => x.Name).Distinct();

			foreach (string folderName in distinctFolderNames)
			{
				DeviceFile deviceFile = new DeviceFile { IsDirectory = true, Name = folderName };

				foreach (DeviceFile deviceFile1 in deviceFiles.Where(x => x.Name == folderName))
					deviceFile.Contents.AddRange(deviceFile1.Contents);

				deviceFileList.Add(deviceFile);
			}

			PrintDeviceFiles(deviceFileList);
		}

		static void PrintDeviceFiles(List<DeviceFile> deviceFiles, int tabCount = 0)
		{
			foreach (DeviceFile deviceFile in deviceFiles)
			{
				Console.WriteLine(new string(' ', tabCount * 2) + deviceFile.Name);

				if (deviceFile.HasContents)
					PrintDeviceFiles(deviceFile.Contents, tabCount + 1);

				tabCount = 0;
			}
		}

		static List<DeviceFile> GetDeviceFiles(List<string> filePaths)
		{
			List<DeviceFile> deviceFiles = new List<DeviceFile>();
			IEnumerable<IEnumerable<string>> splitFilePaths = filePaths.Select(x => x.Split('/'));

			foreach (IGrouping<string, IEnumerable<string>> folder in splitFilePaths.GroupBy(x => x.First()))
			{
				foreach (IEnumerable<string> folderContents in folder)
				{
					deviceFiles.Add(GetDeviceFile(folder.Key, string.Join('/', folderContents.Where(x => x != folder.Key))));
				}
			}

			return deviceFiles;
		}

		static DeviceFile GetDeviceFile(string folderName, string filePath)
		{
			DeviceFile deviceFile = new DeviceFile { IsDirectory = true, Name = folderName };

			if (filePath.Contains("/"))
				deviceFile.Contents.AddRange(GetDeviceFiles(new List<string> { filePath }));
			else
				deviceFile.Contents.Add(new DeviceFile { Name = filePath });

			return deviceFile;
		}

		#endregion

		#region Approach 2

		static void GenerateDeviceFiles_Approach2()
		{
			List<string> zipFilePaths = GetData();

			Console.WriteLine("GenerateDeviceFiles_Approach2");
			Console.WriteLine("--------------------------------");

			allDeviceFiles.Clear();
			foreach(string zipFilePath in zipFilePaths)
				allDeviceFiles.AddRange(ScanForDeviceFiles(zipFilePath));

			allDeviceFiles = CombineDeviceFiles(allDeviceFiles);

			foreach (var deviceFile in allDeviceFiles)
			{
				PrintTree(deviceFile, "", true);
			}
		}

		static List<DeviceFile> CombineDeviceFiles(List<DeviceFile> source)
		{
			List<DeviceFile> deviceFiles = new List<DeviceFile>();

			foreach(DeviceFile deviceFile in source)
			{
				if (deviceFile.HasContents)
					deviceFile.Contents = CombineDeviceFiles(deviceFile.Contents);

				if (deviceFiles.Any(x => x.IsDirectory && x.Name == deviceFile.Name))
				{
					int nodeIndex = deviceFiles.IndexOf(deviceFiles.First(x => x.IsDirectory && x.Name == deviceFile.Name));
					deviceFiles[nodeIndex].Contents.AddRange(deviceFile.Contents);
				}
				else
					deviceFiles.Add(deviceFile);
			} 

			return deviceFiles;
		}

		static List<DeviceFile> ScanForDeviceFiles(string folderPath)
		{
			List<DeviceFile> deviceFiles = new List<DeviceFile>();

			if (folderPath.Contains("/"))
			{
				var fileParts = folderPath.Split('/');
				var subFolderOrFileName = string.Join('/', fileParts.Where(x => x != fileParts[0]));
				deviceFiles.Add(new DeviceFile { IsDirectory = true, Name = fileParts[0], Contents = ScanForDeviceFiles(subFolderOrFileName) });
			}
			else
			{
				deviceFiles.Add(new DeviceFile { Name = folderPath });
			}

			return deviceFiles;
		}

		static List<DeviceFile> ScanForDeviceFiles2(string folderPath)
		{
			List<DeviceFile> deviceFiles = new List<DeviceFile>();

			DeviceFile deviceFile;
			if (folderPath.Contains("/"))
			{
				var fileParts = folderPath.Split('/');
				var subFolderOrFileName = string.Join('/', fileParts.Where(x => x != fileParts[0]));
				deviceFile = new DeviceFile { IsDirectory = true, Name = fileParts[0], Contents = ScanForDeviceFiles(subFolderOrFileName) };
			}
			else
			{
				deviceFile = new DeviceFile { Name = folderPath };
			}

			if (deviceFiles.Any(x => x.Name == deviceFile.Name))
			{
				int matchIndex = deviceFiles.IndexOf(deviceFiles.First(x => x.Name == deviceFile.Name));
				deviceFiles[matchIndex].Contents.Add(deviceFile);
			}
			else
				deviceFiles.Add(deviceFile);

			return deviceFiles;
		}

		#endregion

		#region Approach 3

		List<Tuple<string, int>> currentFolderWatcher = new List<Tuple<string, int>>();

		static void GenerateDeviceFiles_Approach3()
		{
			List<string> zipFilePaths = GetData();

			Console.WriteLine("GenerateDeviceFiles_Approach3");
			Console.WriteLine("--------------------------------");

			allDeviceFiles.Clear();
			ConvertListToGroups(zipFilePaths);

			foreach (var deviceFile in allDeviceFiles)
			{
				PrintTree(deviceFile, "", true);
			}
		}

		static void ConvertListToGroups(List<string> deviceFilePaths)
		{
			List<DeviceFile> deviceFiles = new List<DeviceFile>();
			List<List<string>> splitFilePaths = deviceFilePaths.Select(x => x.Split('/').ToList()).ToList();

			int maxFolderLevel = splitFilePaths.Max(x => x.Count());
			
			for (int i = 0; i < maxFolderLevel; i++)
			{
				var groupedList = splitFilePaths.GroupBy(x => x.First());

				foreach (var group in groupedList)
				{
					foreach (var groupContents in group)
					{
						foreach (var fileOrFolder in groupContents)
						{
							bool isFolder = string.IsNullOrEmpty(Path.GetExtension(fileOrFolder));

							if (isFolder)
							{
								
							}

							if (!string.IsNullOrEmpty(Path.GetExtension(fileOrFolder)))
								deviceFiles.Add(new DeviceFile { Name = fileOrFolder });
							else
								deviceFiles.Add(new DeviceFile { IsDirectory = true, Name = fileOrFolder }) ;
						}
					}
				}

				for (int j = 0; j < splitFilePaths.Count(); j++)
					splitFilePaths[j].RemoveAt(0);
			}
		}

		static List<DeviceFile> ScanForDeviceFilesMock(string folderPath)
		{
			List<DeviceFile> deviceFiles = new List<DeviceFile>();

			IEnumerable<string> directories = GetDirectories(folderPath);
			foreach (string directory in directories)
			{
				string folderName = string.IsNullOrEmpty(folderPath) ? directory.Split('/')[0] : directory.Replace(folderPath, "").Split('/')[0];
				string subFolderPathOrFileName = directory.Replace(folderPath + folderName, "");

				if (!subFolderPathOrFileName.StartsWith('/') && subFolderPathOrFileName.Contains('/'))
					subFolderPathOrFileName = folderName + "/" + subFolderPathOrFileName.Split('/')[0];
				else
					subFolderPathOrFileName = directory.Split('/').Last();

				deviceFiles.Add(new DeviceFile { IsDirectory = true, Name = folderName, Contents = ScanForDeviceFilesMock(subFolderPathOrFileName) });
			}

			IEnumerable<string> files = GetFiles(folderPath);
			if (files.Any())
				deviceFiles.AddRange(files.Select(x => new DeviceFile { Name = x }));

			return deviceFiles;
		}

		static IEnumerable<string> GetDirectories(string folderPath)
		{
			List<string> zipFilePaths = GetData();

			return zipFilePaths.Where(x => {
				if (string.IsNullOrEmpty(folderPath))
					return x.Contains('/');
				else if (x.StartsWith(folderPath))
					return x.Replace(folderPath, "").Contains('/');

				return false;
			});
		}

		static IEnumerable<string> GetFiles(string folderPath)
		{
			if (!folderPath.Contains('/'))
				return new string[] { folderPath };

			List<string> zipFilePaths = GetData();

			return zipFilePaths.Where(x => {
				if (string.IsNullOrEmpty(folderPath))
					return !x.Contains('/');
				else if (x.StartsWith(folderPath))
					return !x.Replace(folderPath, "").Contains('/');

				return false;
			});
		}


		//static List<DeviceFile> ScanForDeviceFiles(string folderPath)
		//{
		//	List<DeviceFile> deviceFiles = new List<DeviceFile>();

		//	string[] directories = Directory.GetDirectories(folderPath);
		//	foreach (string directory in directories)
		//	{
		//		deviceFiles.Add(new DeviceFile { IsDirectory = true, Name = Path.GetFileName(directory), Contents = ScanForDeviceFiles(directory) });
		//	}

		//	string[] files = Directory.GetFiles(folderPath);
		//	if (files.Any())
		//		deviceFiles.AddRange(files.Select(x => new DeviceFile { Name = Path.GetFileName(x), Path = x }));

		//	return deviceFiles;
		//}

		#endregion

		#region Approach 4

		static void GenerateDeviceFiles_Approach4()
		{
			List<string> zipFilePaths = GetData();

			Console.WriteLine("GenerateDeviceFiles_Approach4");
			Console.WriteLine("--------------------------------");

			allDeviceFiles.Clear();
			//GenerateFolderLevelList(zipFilePaths);
			allDeviceFiles.AddRange(GenerateTree(zipFilePaths));

			foreach (var deviceFile in allDeviceFiles)
			{
				PrintTree(deviceFile, "", true);
			}
		}

		/* Generate Folder Level List */
		static List<Tuple<string, string, int>> folderLevels = new List<Tuple<string, string, int>>();
		static void GenerateFolderLevelList(List<string> folderPaths)
		{
			foreach(string folderPath in folderPaths)
			{
				int folderLevel = 0;
				string parentFolder = "";
				foreach(string path in folderPath.Split('/'))
				{
					folderLevels.Add(new Tuple<string, string, int>(parentFolder, path, folderLevel));
					parentFolder = path;
					folderLevel++;
				}
			}
		}


		static List<DeviceFile> GenerateTree(List<string> folderPaths)
		{
			/* Generate Folder Level List */
			List<Tuple<string, string, int>> folderLevels = new List<Tuple<string, string, int>>();

			foreach (string folderPath in folderPaths)
			{
				int folderLevel = 0;
				string parentFolder = "";
				foreach (string path in folderPath.Split('/'))
				{
					folderLevels.Add(new Tuple<string, string, int>(parentFolder, path, folderLevel));
					parentFolder = path;
					folderLevel++;
				}
			}

			/* Build Tree */
			List<DeviceFile> deviceFiles = new List<DeviceFile>();

			for (int index = 0; index <= folderLevels.Max(x => x.Item3); index++)
			{
				IEnumerable<IGrouping<string, Tuple<string, string, int>>> levelItems = folderLevels.Where(x => x.Item3 == index).GroupBy(x => x.Item2);

				foreach(IGrouping<string, Tuple<string, string, int>> levelItemContents in levelItems)
				{
					DeviceFile deviceFile = new DeviceFile { IsDirectory = string.IsNullOrEmpty(Path.GetExtension(levelItemContents.Key)), Name = levelItemContents.Key };

					foreach (Tuple<string, string, int> levelItem in levelItemContents)
					{
						if (string.IsNullOrEmpty(levelItem.Item1)) //No Parent
						{
							if (!deviceFiles.Any(x => x.Name == levelItem.Item2))
								deviceFiles.Add(deviceFile);
						}
						else // Has Parent
						{
							foreach (DeviceFile folderItem in deviceFiles)
							{
								if (FindParentAndAddNode(folderItem, levelItem.Item1, deviceFile))
									break;
							}
						}
					}
				}
			}

			return deviceFiles;
		}

		static bool FindParentAndAddNode(DeviceFile lookup, string folderName, DeviceFile node)
		{
			if (lookup.Name == folderName && !lookup.Contents.Any(x => x.Name == node.Name))
			{
				lookup.Contents.Add(node);
				return true;
			}
				
			if (lookup.HasContents)
			{
				foreach(DeviceFile deviceFile in lookup.Contents)
				{
					if (deviceFile.IsDirectory)
						return FindParentAndAddNode(deviceFile, folderName, node);
				}
			}

			return false;
		}

		#endregion

		static void PrintTree(DeviceFile tree, String indent, bool last)
		{
			Console.WriteLine(indent + "+- " + tree.Name);
			indent += last ? "   " : "|  ";

			for (int i = 0; i < tree.Contents.Count; i++)
			{
				PrintTree(tree.Contents[i], indent, i == tree.Contents.Count - 1);
			}
		}

		static List<string> GetData()
		{
			var dataList = new List<string>
			{
				"Data/Assigned/Configurations.db",
				"Data/Configurations.db",
				"Data/DeviceHistory.db",
				"Data/InitialSetup.xml",
				"Data/Transactions.trn.db",
				"Data/Assigned/Self/Configurations2.db",
				"Logs/Convey.log",
				"Logs/tools-20221018.log",
				"Data/Assigned/Configurations2.db",
				"ToolsState.txt",
				"Transactions.trn",
				"Data/Assigned/Self/Configurations.db",
				"Data/Assigned/Self/Self2/Configurations.db",
				"Data/Assigned/Self/Self2/Self3/Configurations.db",
				"Data/Assigned/Self/Self2/Self3/Self4/Configurations.db"
			};

			dataList.Sort();
			return dataList;
		}
	}
}
