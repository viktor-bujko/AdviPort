using System;
using System.Text;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AdviPort {
	class Program {
		static void Main(string[] args) {

			// TODO: Support možnosti kedy správanie aplikácie nebude interaktívne
			// teda vedieť dopísať správanie programu, kedy sa spracuje nejaký
			// cmdline príkaz a podľa neho sa postaví query

			var reader = Console.In;
			var writer = Console.Out;
			bool exit = false;

			while (! exit) {
				var settings = GeneralApplicationSettings.GetAppSettings();

				if (writer.Equals(Console.Out)) {
					Console.WindowWidth = 112;
				}

				IMainPageHandler mainHandler = MainPageHandlerSelector.SelectMainPageHandler(settings, reader, writer);

				mainHandler.PrintMainPageContent(settings);

				var input = mainHandler.ReadUserInput();

				IExecutablePlugin chosenPlugin = mainHandler.HandlePluginChoice(input);

				if (chosenPlugin is null) {
					Console.ReadLine();
					Console.Clear();
					continue;
				}

				Console.Clear();

				int exitCode = chosenPlugin.Invoke(args);

				if (chosenPlugin is ExitAppPlugin) {
					exit = exitCode == 0;
				}

				if (exitCode != 0) {
					Console.Error.WriteLine("An error occured during the execution of chosen plugin.");
				}

				writer.WriteLine("Press any key to continue.");
				Console.ReadKey();
				Console.Clear();
			}
		}
	}

	class GeneralApplicationSettings {
		public string ApiKeyPath { get; set; }
		public string[] SubCommands { get; set; }
		public string[] AvailablePlugins { get; set; }
		public string MainPageStyle { get; set; }
		public string[] Decorations { get; set; }

		public static GeneralApplicationSettings GetAppSettings() {

			var filePaths = SearchFiles(Directory.GetCurrentDirectory(), "*settings.json", 1);

			if (filePaths is null) { throw new FileNotFoundException("A required file has not been found.");}

			using var settingsFileReader = GetTextReader(filePaths);

			if (settingsFileReader is null) throw new ArgumentException();

			var options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };

			string text = settingsFileReader.ReadToEnd();

			var settings = JsonSerializer.Deserialize<GeneralApplicationSettings>(text, options);

			return settings;
		}

		public static string[] SearchFiles(string path, string pattern, int requiredFiles = -1, int depth = 5) {

			string[] filePaths;

			try {
				filePaths = Directory.GetFiles(path, pattern, SearchOption.AllDirectories);
			} catch { 
				return null;
			}

			if (requiredFiles > -1 && filePaths.Length != requiredFiles) {
				if (depth > 0) {
					return SearchFiles(Directory.GetParent(path).FullName, pattern, requiredFiles, depth - 1);
				} else {
					return null;
				}
			}

			return filePaths;
		}

		public static string[] SearchDir(string startDirPath, string targetDirName, int depth = 5) {
			List<string> targetDirPaths = new List<string>();

			var options = new EnumerationOptions() {
				IgnoreInaccessible = true
			};
			var matchingDirs = Directory.EnumerateDirectories(startDirPath, targetDirName, options);

			foreach(string dirPath in matchingDirs) {
				targetDirPaths.Add(dirPath);
			}
			
			if (targetDirPaths.Count == 0) {
				if (depth > 0) {
					return SearchDir(Directory.GetParent(startDirPath).FullName, targetDirName, depth - 1);
				} else {
					return null;
				}
			}

			return targetDirPaths.ToArray();
		}

		public static TextReader GetTextReader(string[] paths, int index = 0) {
			TextReader textReader;
			try {
				textReader = new StreamReader(paths[index]);
			} catch {
				textReader = null;
			}

			return textReader;
		}
	}
}
