using System;
using System.Text;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AdviPort {
	class Program {
		static void Main(string[] args) {

			var settings = GeneralApplicationSettings.GetAppSettings();

			IMainContentPrinter mainPagePrinter = MainPagePrinterSelector.SelectMainPagePrinter(settings);

			mainPagePrinter.PrintMainPageContent(Console.Out, settings);

			Console.ReadLine();

			Console.Clear();


			/*
			 * Keď načítam nastavenia aplikácie, tak si z nich vyberiem to, ktorý MainPagePrinter sa má vybrať
			 * (napr nejaký simple, classic, decorative), a budem chcieť aby sa ten mainpage obnovoval a používal stále keď 
			 * sa do menu vrátim -> asi zakaždým čítať tie nastavenia a meniť podľa nich správanie programu
			 * 
			 * simple - len vypíše nejaké veci a ponuku
			 * decorative - nájdi adviport ascii symbol
			 * descriptive - pridá nejaké vysvetlivky ku simple alebo také niečo
			 */
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
