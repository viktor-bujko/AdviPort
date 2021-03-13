using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Security;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace AdviPort {

	interface IExecutablePlugin {
		int Invoke(object[] args);
	}

	interface IPlugin : IExecutablePlugin {
		string Name { get; }
		string Description { get; }

		//TODO: string QueryName { get; } - ak chcem teda dopísať modul kedy sa bude dať volať priamo plugin z cmdline
	}

	internal static class PluginSelector {
		internal static IPlugin SearchPluginByName(string pluginName, TextReader reader, TextWriter writer) {
			IPlugin plugin = pluginName switch {
				"register" => new RegisterAPIKeyPlugin(reader, writer),
				"add_favourite" => new AddFavouriteAirportPlugin(),
				"remove_favourite" => new RemoveFavouriteAirportPlugin(),
				"select_airport" => new SelectAirportPlugin(),
				"pinpoint" => new PinpointAirportPlugin(),
				"print_schedule" => new PrintScheduleAirport(),
				"about" => new AboutAppPlugin(),
				"exit" => new ExitAppPlugin(),
				"search_by_flight" => new SearchByFlightPlugin(),
				"save_flight_info" => new SaveFlightInfoPlugin(),
				"airport_info" => new AirportInfoPlugin(),
				"aircraft_info" => new AircraftInfoPlugin(),
				_ => null
			};

			return plugin;
		}

		public static IPlugin[] GetAvailablePlugins(GeneralApplicationSettings settings, TextReader reader, TextWriter writer) {

			List<IPlugin> plugins = new List<IPlugin>(settings.AvailablePlugins.Length);

			foreach (string pluginName in settings.AvailablePlugins) {
				var plugin = SearchPluginByName(pluginName, reader, writer);
				if (plugin is null) continue;

				plugins.Add(plugin);
			}

			return plugins.ToArray();
		}

		internal static bool TryGetPluginFromInputString(string input, IPlugin[] plugins, out List<IPlugin> filteredPlugins) {
			filteredPlugins = new List<IPlugin>();
			input = input.ToLower();

			if (string.IsNullOrEmpty(input)) return false;

			if (input == "exit" || input == "quit") {
				filteredPlugins.Add(new ExitAppPlugin());
				return true;
			}

			for (int i = 0; i < plugins.Length; i++) {
				bool inputIsSubstring = plugins[i].Name.ToLower().StartsWith(input);

				if (inputIsSubstring) filteredPlugins.Add(plugins[i]);
			}

			return filteredPlugins.Count == 1;
		}
	}

	abstract class InputReadablePlugin : IUserInterfaceReader {
		protected abstract TextReader Reader { get; }
		protected abstract TextWriter Writer { get; }

		public virtual string ReadUserInput(string initialPrompt) {
			if (!(initialPrompt is null)) {
				Writer.Write(initialPrompt + ": ");
			}

			var input = Reader.ReadLine();

			if (input is null) throw new ArgumentNullException("The input should not be null.");

			return input.Trim();
		}
	}

	class AboutAppPlugin : IPlugin {
		public string Name => "About Application";

		public string Description => "Prints information about application.";

		public int Invoke(object[] args) {
			string[] paths = GeneralApplicationSettings.SearchFiles(Directory.GetCurrentDirectory(), "about.txt", 1);

			if (paths is null) throw new FileNotFoundException("A required file has not been found.");

			TextReader aboutReader = GeneralApplicationSettings.GetTextReader(paths);

			if (aboutReader is null) { 
				Console.Error.WriteLine("Specification file could not be found.");
				return 1;
			}

			Console.WriteLine(aboutReader.ReadToEnd());

			return 0;
		}
	}

	class ExitAppPlugin : IPlugin {
		public string Name => "Exit Application";

		public string Description => "Quits the application";

		public int Invoke(object[] args) {
			Console.WriteLine("Exiting ADVIPORT application.");
			return 0;
		}
	}

	class RegisterAPIKeyPlugin : InputReadablePlugin, IPlugin {
		public string Name => "Register API key";

		public string Description => "Registers a new user and his / her API key";

		protected override TextReader Reader { get; }

		protected override TextWriter Writer { get; }

		public RegisterAPIKeyPlugin(TextReader reader, TextWriter writer) {
			Reader = reader;
			Writer = writer;
		}

		public int Invoke(object[] args) {

			var userName = ReadUserInput("Please enter a name you want to register");
			var profileFileName = $"{userName}_userprofile.apt";

			string profilesDir = GetProfilesDirectory();

			if (profilesDir is null) { return 1; }

			FileStream proFileStream = CreateNewProfile(profilesDir, profileFileName, out string profileFilePath);

			if (proFileStream is null) {
				Console.Error.WriteLine("An error ocurred while creating a new profile file for this user.");
				return 1;
			}

			using (proFileStream) {
				var apiKey = ReadUserInput("Please enter the API key you want to use in the application");

				try {
					apiKey = Encryptor.Encrypt(apiKey);

					var profile = new UserProfile(apiKey);

					string serializedProfile = JsonSerializer.Serialize<UserProfile>(profile);

					proFileStream.Write(Encoding.UTF8.GetBytes(serializedProfile));
				} catch {
					// Log the error 
					// User profile should be deleted if anything goes wrong 
					File.Delete(profileFilePath);
					return 1;
				}

				Console.WriteLine("Registration of a new user is successful.");
			}

			return 0;
		}

		private FileStream CreateNewProfile(string profilesDir, string fileName, out string filePath) {
			filePath = profilesDir + Path.DirectorySeparatorChar + fileName;

			if (!UsernameIsFree(profilesDir, fileName)) {
				Console.Error.WriteLine("A user with given username already exists. Please choose another name.");
				return null;
			}

			try {
				var newProfileStream = File.Create(filePath);
				return newProfileStream;
			} catch {
				return null;
			}
		}

		private bool UsernameIsFree(string profileDir, string profileFileName) {
			string[] profilePaths = GeneralApplicationSettings.SearchFiles(profileDir, profileFileName);

			// If true, userprofile file with such username can be created.
			return profilePaths is null || profilePaths.Length == 0;
		}

		private string GetProfilesDirectory() {

			string[] foundProfileDirs = GeneralApplicationSettings.SearchDir(Directory.GetCurrentDirectory(), "profiles");
			string profilesDir = foundProfileDirs[0];

			if (foundProfileDirs is null) {
				Console.Error.WriteLine("Directory with application profile could not be found. Please move \"profiles/\" directory into the project root directory.");
				return null;
			}

			return profilesDir;
		}
	}

	class AddFavouriteAirportPlugin : IPlugin {
		public string Name => "Add a favourite airport";

		public string Description => "Adds an airport into current account's bookmarks";

		public int Invoke(object[] args) {

			// Require the user to be registered
			Console.WriteLine($"Hello From {Name}!");
			return 0;
		}
	}

	class RemoveFavouriteAirportPlugin : IPlugin {
		public string Name => "Remove a favourite airport";

		public string Description => "Removes an airport from current account's bookmarks";

		public int Invoke(object[] args) {

			// Require the user to be registered
			Console.WriteLine($"Hello From {Name}!");
			return 0;
		}
	}

	class SelectAirportPlugin : IPlugin {
		public string Name => "Select an airport";

		public string Description => "Selects an airport to work with";

		public int Invoke(object[] args) {
			Console.WriteLine($"Hello From {Name}!");
			return 0;
		}
	}

	class PinpointAirportPlugin : IPlugin {
		public string Name => "Pinpoint an airport";

		public string Description => "Default description";

		public int Invoke(object[] args) {
			Console.WriteLine($"Hello From {Name}!");
			return 0;
		}
	}

	class PrintScheduleAirport : IPlugin {
		public string Name => "Print the flights schedule of a selected airport";

		public string Description => "Prints the flights schedule for selected airport";

		public int Invoke(object[] args) {
			Console.WriteLine($"Hello From {Name}!");
			return 0;
		}
	}

	class SearchByFlightPlugin : IPlugin {
		public string Name => "Search for a flight by the flight number (e.g. AF 1438)";

		public string Description => "Searches for a concrete flight (e.g. AF 1438)";

		public int Invoke(object[] args) {
			Console.WriteLine($"Hello From {Name}!");
			return 0;
		}
	}

	class SaveFlightInfoPlugin : IPlugin {
		public string Name => "Create a bookmark for a given flight";

		public string Description => "Moves a flight into the followed ones";

		public int Invoke(object[] args) {
			Console.WriteLine($"Hello From {Name}!");
			return 0;
		}
	}

	class AirportInfoPlugin : IPlugin {
		public string Name => "Print basic information about a specified airport";

		public string Description => "Prints available information about an airport";

		public int Invoke(object[] args) {
			Console.WriteLine($"Hello From {Name}!");
			return 0;
		}
	}

	class AircraftInfoPlugin : IPlugin {
		public string Name => "Get information about different types of airplanes";

		public string Description => "Prints available information about an aircraft";

		public int Invoke(object[] args) {
			Console.WriteLine($"Hello From {Name}!");
			return 0;
		}
	}

}
