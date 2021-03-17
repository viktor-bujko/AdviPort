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

	class PluginInputReader : IUserInterfaceReader {
		protected virtual TextReader Reader { get; }
		protected virtual TextWriter Writer { get; }

		public PluginInputReader(TextReader reader, TextWriter writer) {
			Reader = reader;
			Writer = writer;
		}

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
			string[] paths = GeneralApplicationSettings.SearchFiles(Directory.GetCurrentDirectory(), "about.txt", requiredFiles: 1);

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

	class RegisterAPIKeyPlugin : IPlugin {
		public string Name => "Register API key";

		public string Description => "Registers a new user and his / her API key";

		private PluginInputReader InputReader { get; }
		private IUserChecker UserChecker { get; }
		private IUserProfileCreator ProfileCreator { get; }

		public RegisterAPIKeyPlugin(PluginInputReader inputReader, IUserChecker userChecker, IUserProfileCreator profileCreator) {
			InputReader = inputReader;
			UserChecker = userChecker;
			ProfileCreator = profileCreator;
		}

		public RegisterAPIKeyPlugin(PluginInputReader inputReader, IUserDBHandler userHandler) {
			InputReader = inputReader;
			UserChecker = userHandler;
			ProfileCreator = userHandler;
		}

		public int Invoke(object[] args) {

			var userName = InputReader.ReadUserInput("Please enter a name you want to register");

			if (UserChecker.UserExists(userName)) {
				Console.Error.WriteLine($"A user with name {userName} already exists. Please choose another name.");
				return 1;
			}

			var apiKey = InputReader.ReadUserInput("Please enter the API key you want to use in the application");

			return ProfileCreator.CreateProfile(userName, apiKey);
		}
	}

	class AddFavouriteAirportPlugin : IPlugin {
		public string Name => "Add a favourite airport";

		public string Description => "Adds an airport into current account's bookmarks";

		private PluginInputReader InputReader { get; }

		private IAirportFinder AirportFinder { get; }

		private IUserChecker UserChecker { get; }

		public AddFavouriteAirportPlugin(PluginInputReader inputReader, IAirportFinder airportFinder, IUserChecker userChecker) {
			InputReader = inputReader;
			AirportFinder = airportFinder;
			UserChecker = userChecker;
		}

		public int Invoke(object[] args) {

			// TODO: Check current session - whether the user exists, is logged in.

			var airportIcaoCode = InputReader.ReadUserInput("Please enter the ICAO code of your favourite airport");

			AirportFinder.FindAirportByCode(airportIcaoCode);

			/* if foundAirports == null -> error; can't be added as favourite airport. 
			   check also if the airport is not already in the favourites airports. */

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
