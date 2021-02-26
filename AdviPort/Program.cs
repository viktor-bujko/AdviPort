using System;
using System.Text;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AdviPort {
	class Program {
		static void Main(string[] args) {

			var settings = GetAppSettings();

			MainPagePrinter mainPagePrinter = MainPagePrinter.SelectPrinter(settings.MainPageStyle);

			mainPagePrinter.Print(Console.Out, settings);

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

		private static GeneralApplicationSettings GetAppSettings() {

			using var settingsFileReader = GetSettingsFileReader(Directory.GetCurrentDirectory());
			
			if (settingsFileReader is null) throw new ArgumentException();

			var options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };

			string text = settingsFileReader.ReadToEnd();

			var settings = JsonSerializer.Deserialize<GeneralApplicationSettings>(text, options);

			return settings;
		}

		private static TextReader GetSettingsFileReader(string currentDirectoryPath, int depth = 3) {

			string[] settingsFilesPaths = Directory.GetFiles(currentDirectoryPath, "*settings.json", SearchOption.AllDirectories);

			if (settingsFilesPaths.Length != 1) {
				if (depth > 0) {
					return GetSettingsFileReader(Directory.GetParent(currentDirectoryPath).FullName, depth - 1);
				} else {
					throw new ArgumentException("Make sure EXACTLY one \"settings.json\" file exists in the project directory and its subdirectories.");
				}
			}

			TextReader textReader;
			try {
				textReader = new StreamReader(settingsFilesPaths[0]);
			} catch {
				textReader = null;
			}

			return textReader;
		}
	}

	class GeneralApplicationSettings {
		public string ApiKeyPath { get; set; }
		public string[] SubCommands { get; set; }
		public string[] Modules { get; set; }
		public string MainPageStyle { get; set; }
	}
}
