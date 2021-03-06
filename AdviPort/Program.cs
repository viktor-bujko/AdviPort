using System;
using System.Text;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AdviPort {
	class Program {
		static void Main(string[] args) {

			while (true) {
				var settings = GeneralApplicationSettings.GetAppSettings();

				IMainContentPrinter mainPagePrinter = MainPagePrinterSelector.SelectMainPagePrinter(settings);

				mainPagePrinter.PrintMainPageContent(Console.Out, settings);

				Console.ReadLine();

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

			var filePaths = SearchForFiles(Directory.GetCurrentDirectory(), "*settings.json", requiredFiles: 1);

			using var settingsFileReader = GetTextReader(filePaths);

			if (settingsFileReader is null) throw new ArgumentException();

			var options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };

			string text = settingsFileReader.ReadToEnd();

			var settings = JsonSerializer.Deserialize<GeneralApplicationSettings>(text, options);

			return settings;
		}

		public static string[] SearchForFiles(string path, string pattern, int depth = 3, int requiredFiles = -1) {

			string[] filePaths;

			try {
				filePaths = Directory.GetFiles(path, pattern, SearchOption.AllDirectories);
			} catch { 
				return null;
			}

			if (requiredFiles > -1 && filePaths.Length != requiredFiles) {
				if (depth > 0) {
					return SearchForFiles(Directory.GetParent(path).FullName, pattern, depth - 1, requiredFiles);
				} else {
					throw new ArgumentException("Make sure EXACTLY one \"settings.json\" file exists in the project directory and its subdirectories.");
				}
			}

			return filePaths;
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
