using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Microsoft.Ajax.Utilities;

namespace Ixoxo.BundleCompiler
{
	class Program
	{
		private static CodeSettings _buildSettings = new CodeSettings
		{
			EvalTreatment = EvalTreatment.MakeImmediateSafe,
			TermSemicolons = true,
			PreserveImportantComments = false
		};


		static void Main(string[] args)
		{
			if(args.Length != 1) 
			{
				Console.WriteLine("Bundle path needed.");
				return;
			}

			var bundlePath = PathHelper.GetFullPath(args[0]);
			var path = PathHelper.GetFullPath(bundlePath.Replace(".js.bundle", ".js"));
			var minifiedPath = PathHelper.GetFullPath(bundlePath.Replace(".js.bundle", ".min.js"));

			CompileBundle(bundlePath, path, minifiedPath);
		}


		/// <summary>
		/// Compiles .bundle file
		/// </summary>
		/// <param name="bundlePath"></param>
		/// <param name="path"></param>
		/// <param name="minifiedPath"></param>
		/// <param name="mapPath"></param>
		/// <param name="root"></param>
		public static void CompileBundle(string bundlePath, string path, string minifiedPath)
		{
			Console.WriteLine(Environment.NewLine + "Bundle: " + Path.GetFileName(bundlePath));

			var bundle = XDocument.Load(bundlePath);
			var minify = Convert.ToBoolean(bundle.Element("bundle").Attribute("minify").Value);

			var fileList = bundle.Element("bundle").Elements("file").Select(x => x.Value);
			var builder = new StringBuilder();

			//Read content of each file in bundle
			foreach (var file in fileList)
			{
				Console.WriteLine("Adding: " + file);

				builder.AppendLine("///#source 1 1 " + file);
				builder.AppendLine(File.ReadAllText(PathHelper.GetFullPath(file)));
			}

			//Write concatenated JS to file and minify if needed
			if (!File.Exists(path) || (File.ReadAllText(path) != builder.ToString()))
			{
				using (StreamWriter writer = new StreamWriter(path, false, new UTF8Encoding(true)))
				{
					writer.Write(builder.ToString());	
				}
				Console.WriteLine("Merged into: " + Path.GetFileName(path));

				if (minify)
					MinifyFileWithSourceMap(path, minifiedPath, _buildSettings, true);
			}
			else {
				Console.WriteLine("No changes.");
			}
		}


		/// <summary>
		/// Generate source map then minify file
		/// </summary>
		/// <param name="file"></param>
		/// <param name="minFile"></param>
		/// <param name="settings"></param>
		/// <param name="isBundle"></param>
		private static void MinifyFileWithSourceMap(string file, string minFile, CodeSettings settings, bool isBundle)
		{
			var fileName = minFile + ".map";

			using (var writer = new StreamWriter(fileName, false, new UTF8Encoding(false)))
			{
				using (var map = new V3SourceMap(writer))
				{
					settings.SymbolsMap = map;
					map.StartPackage(Path.GetFileName(minFile), Path.GetFileName(fileName));
					MinifyFile(file, minFile, settings, true, isBundle);
					map.EndPackage();
				}
			}

			Console.WriteLine("Source map generated: " + Path.GetFileName(fileName));
		}


		/// <summary>
		/// Minify file
		/// </summary>
		/// <param name="file"></param>
		/// <param name="minFile"></param>
		/// <param name="settings"></param>
		/// <param name="generateJsSourceMap"></param>
		/// <param name="isBundle"></param>
		private static void MinifyFile(string file, string minFile, CodeSettings settings, bool generateJsSourceMap, bool isBundle)
		{
			var minifier = new Minifier();

			if (!isBundle)
				minifier.FileName = Path.GetFileName(file);

			var minifiedCode = minifier.MinifyJavaScript(File.ReadAllText(file), settings);

			if (generateJsSourceMap)
				minifiedCode = minifiedCode + Environment.NewLine + "//@ sourceMappingURL=" + Path.GetFileName(minFile) + ".map";

			using (StreamWriter writer = new StreamWriter(minFile, false, new UTF8Encoding(true)))
			{
				writer.Write(minifiedCode);
			}

			Console.WriteLine("Minified into: " + Path.GetFileName(minFile));
			Console.WriteLine("Orginal Size: " + new FileInfo(file).Length.ToString());
			Console.WriteLine("Minified Size: " + new FileInfo(minFile).Length.ToString());
		}
	}
}
