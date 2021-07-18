using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using AdviPort.Plugins;
using AdviPort.UI;
using System.Text;

namespace AdviPort {

	class Program {

		/// <summary>
		/// Main application method which describes application lifecycle.
		/// </summary>
		/// <param name="args">Command-line arguments.</param>
		static void Main(string[] args) {

			bool exit = false;
			var mainPageNavigator = new DefaultMainPageNavigator();

			// Setting the console window to suitable size.
			Console.WindowWidth = 130;
			Console.WindowHeight = 50;

			while (! exit) {
				var settings = GeneralApplicationSettings.GetAppSettings();

				if (settings == null) { break; }

				IMainPageHandler mainHandler = MainPageHandlerSelector.SelectMainPageHandler(settings);
				IMainPageNavigator navigator = mainPageNavigator;

				int printedCount = mainHandler.PrintMainPageContent(settings);

				var input = navigator.NavigateOrReadInput(printedCount);

				IExecutablePlugin chosenPlugin = mainHandler.HandlePluginChoice(input);

				if (chosenPlugin == null) {
					Console.ReadLine();
					Console.Clear();
					continue;
				}

				Console.Clear();

				int exitCode = chosenPlugin.Invoke();

				if (chosenPlugin is ExitAppPlugin) exit = exitCode == 0;	// exit only if the exitCode is without any error.

				if (exitCode != 0) Console.Error.WriteLine("An error occured during the execution of chosen plugin.");

				Console.WriteLine("Press any key to continue.");
				Console.ReadKey();
				Console.Clear();
			}
		}
	}

	/// <summary>
	/// A static class which represents application settings descriptor. This class contains properties 
	/// which can be set in settings.json file.
	/// </summary>
	class GeneralApplicationSettings {

		private static readonly string[] defaultStringArr = { };
		public string ApiKeyPath { get; set; }
		public string[] SubCommands { get; set; } = defaultStringArr;
		public string[] AvailablePlugins { get; set; } = defaultStringArr;
		public string MainPageStyle { get; set; }
		public string[] Decorations { get; set; } = defaultStringArr;

		/// <summary>
		/// Fetches application settings from the settings.json file. After getting correct settings.json
		/// file, this file is read and application settings descriptor instance is created.
		/// This method assumes settings.json file is reasonably small in size that it can be read using
		/// <see cref="TextReader.ReadToEnd"/> method.
		/// </summary>
		/// <returns>An instance of application settings descriptor.</returns>
		public static GeneralApplicationSettings GetAppSettings() {

			var filePaths = SearchFiles(AppDomain.CurrentDomain.BaseDirectory, "*settings.json", requiredFiles: 1);
			GeneralApplicationSettings settings;

			if (filePaths == null) {
				Console.Error.WriteLine("A required settings file has not been found.");
				return null;
			}

			using var settingsFileReader = GetTextReader(filePaths);

			if (settingsFileReader == null) {
				Console.Error.WriteLine("File reader instance could not be created.");
				return null;
			}

			var options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };

			try {
				string text = settingsFileReader.ReadToEnd();

				settings = JsonSerializer.Deserialize<GeneralApplicationSettings>(text, options);
			} catch {
				throw new ApplicationException("Check whether settings.json file could be read and that it is not corrupted");
			}

			return settings;
		}

		/// <summary>
		/// Method responsible for searching for files matching given <paramref name="pattern"/>.
		/// Also, a number of required files matching the <paramref name="pattern"/> can be specified,
		/// which creates another constraint for file matching. In case <paramref name="requiredFiles"/>
		/// parameter and the number of matched files do not match (and the number of required matching 
		/// files is specified), searching for files continues in the parent directory until 
		/// <paramref name="depth"/> is zero or correct number of files is found. Therefore, at most 
		/// <paramref name="depth"/> recursive calls will be performed.
		/// </summary>
		/// <param name="path">Path representing where the search should start.</param>
		/// <param name="pattern">Pattern to which the file names should be matched.</param>
		/// <param name="requiredFiles">Optional parameter for the number of files which should match given pattern. Default is -1 for all matching files.</param>
		/// <param name="depth">Optional parameter for the number of recursive method calls. Default value is 5.</param>
		/// <returns>Array of files which match the <paramref name="pattern"/> in the given <paramref name="path"/>.</returns>
		public static string[] SearchFiles(string path, string pattern, int requiredFiles = -1, int depth = 5) {

			if (string.IsNullOrEmpty(path)) throw new ArgumentNullException($"Cannot search for files if {nameof(path)} is null or empty string.");

			if (string.IsNullOrEmpty(pattern)) throw new ArgumentNullException($"Cannot search for files if {nameof(pattern)} is null or empty string.");

			string[] filePaths;

			try {
				filePaths = Directory.GetFiles(path, pattern, SearchOption.AllDirectories);
			} catch {
				return null;
			}

			if (requiredFiles > -1 && filePaths.Length != requiredFiles) {
				if (depth > 0) { return SearchFiles(Directory.GetParent(path).FullName, pattern, requiredFiles, depth - 1); } 
				else { return null; }
			}

			return filePaths;
		}

		/// <summary>
		/// Method reponsible for searching directories with given <paramref name="targetDirectoryName"/>
		/// starting at <paramref name="startDirectoryPath"/>. If more directories correspond, only the 
		/// first one is returned.
		/// <seealso cref="SearchFiles(string, string, int, int)"/>
		/// </summary>
		/// <param name="startDirectoryPath">Path representing where the search should start.</param>
		/// <param name="targetDirectoryName">Desired directory name to be searched for.</param>
		/// <param name="depth">Optional parameter for the number of recursive method calls. Default value is 5.</param>
		/// <returns>The file path of the directory with <paramref name="targetDirectoryName"/>.</returns>
		public static string SearchDir(string startDirectoryPath, string targetDirectoryName, int depth = 5) {
			IList<string> targetDirPaths = new List<string>();

			var options = new EnumerationOptions() { IgnoreInaccessible = true };
			var matchingDirs = Directory.EnumerateDirectories(startDirectoryPath, targetDirectoryName, options);

			foreach (string dirPath in matchingDirs) {
				targetDirPaths.Add(dirPath);
				break;
			}

			if (targetDirPaths.Count > 0) { return targetDirPaths[0]; }

			if (depth > 0) {
				// Trying to search in the parent directory
				return SearchDir(Directory.GetParent(startDirectoryPath).FullName, targetDirectoryName, depth - 1);
			} else {
				// Still nothing found - giving up on searching
				return null;
			}
		}

		internal static string GetProfilesDirectoryPath() {
			string profilesPath = SearchDir(AppDomain.CurrentDomain.BaseDirectory, "profiles");

			if (profilesPath == null) {
				string current = Directory.GetCurrentDirectory();
				const int parents = 3;
				for (int i = 0; i < parents; i++) {
					// Backing up from current directory "parents" times 
					current = Directory.GetParent(current).FullName;
				}

				profilesPath = current + Path.DirectorySeparatorChar + "profiles";
				Directory.CreateDirectory(profilesPath);
				Console.Error.WriteLine($"Created a new directory: {profilesPath}");
			}

			return profilesPath;
		}

		public static TextReader GetTextReader(string[] paths, int index = 0) {
			TextReader textReader; 
			try {
				textReader = new StreamReader(paths[index], Encoding.UTF8);
			} catch {
				textReader = null;
			}

			return textReader;
		}
	}

	/// <summary>
	/// Class which represents current application session, especially information about a logged user.
	/// Only one instance of this class can be created, which means that the information about user is 
	/// globally shared within the application.
	/// </summary>
	class Session {

		private static Session active;
		private UserProfile loggedUser;

		/// <summary>
		/// Active session instance.
		/// </summary>
		public static Session ActiveSession {
			get {
				if (active == null) { active = new Session(); }

				return active;
			}
		}

		/// <summary>
		/// Flag representing whether a user is logged in current active session.
		/// </summary>
		public bool HasLoggedUser => ActiveSession.LoggedUser != null;

		/// <summary>
		/// Provides instance of currently logged user.
		/// </summary>
		public UserProfile LoggedUser {
			get => loggedUser;
			set => loggedUser = value;
		}

		private Session() {
			LoggedUser = null;
		}
	}
}
