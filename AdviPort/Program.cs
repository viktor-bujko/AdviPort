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
			// teda vedieť dopísať správanie programu, kedy sa spracuje nejaký cmdline príkaz a podľa neho sa postaví query

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

				IPlugin chosenPlugin = mainHandler.HandlePluginChoice(input);

				if (!(chosenPlugin is null)) {
					if (chosenPlugin is ExitAppPlugin) {
						exit = true;
					}

					Console.Clear();
					chosenPlugin.Invoke(null);
				} else {
					// Happens when the plugin to invoke could not be determined from the user's choice.
				}

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
