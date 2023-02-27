using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpConsole.Models
{
	public class DeviceFile
	{
		public bool IsDirectory { get; set; }

		public string Name { get; set; } = string.Empty;

		public string Path { get; set; } = string.Empty;

		/// <summary>
		/// Contains file path for displaying in WebBrowser
		/// .log, .csv and.xml are restrict file extensions and doesn't launch within WebBrowser 
		/// A copy of file is created with .txt extension and its file path will be saved in the property
		/// </summary>
		public string FilePath { get; set; } = string.Empty;

		public List<DeviceFile> Contents { get; set; } = new List<DeviceFile>();

		public bool HasContents { get { return Contents?.Any() ?? false; } }

		public bool CanView { get { return !string.IsNullOrEmpty(Name) ? viewFileExtensions.Contains(System.IO.Path.GetExtension(Name).ToLower()) : false; } }

		public bool CanViewInGrok { get { return string.Equals(Name, "DeviceHistory.db", StringComparison.OrdinalIgnoreCase); } }

		private string[] viewFileExtensions => new string[] { ".txt", ".csv", ".xml", ".log", ".ivd", ".trn" };
	}
}
