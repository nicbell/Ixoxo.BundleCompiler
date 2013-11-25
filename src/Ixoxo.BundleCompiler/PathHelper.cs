using System;
using System.IO;
using System.Linq;

namespace Ixoxo.BundleCompiler
{
	public static class PathHelper
	{
		/// <summary>
		/// Returns full path for a partial path, this is a bit of a hack
		/// </summary>
		/// <param name="partialPath"></param>
		/// <returns></returns>
		public static string GetFullPath(string partialPath)
		{
			var folder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location).ToLower();
			var fullPath = partialPath.ToLower().Replace("/", "\\");

			if (fullPath.StartsWith("\\"))
				fullPath = fullPath.Substring(1);

			fullPath = Path.Combine(folder, fullPath);

			if (!File.Exists(fullPath))
				fullPath = String.Join("\\", fullPath.Split('\\').Distinct());

			if (!File.Exists(fullPath))
				Console.WriteLine("Path does not exist yet: " + fullPath);

			return fullPath;
		}
	}
}
