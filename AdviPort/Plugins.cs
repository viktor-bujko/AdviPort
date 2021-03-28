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

		private IUserPasswordCreator PasswordCreator { get; }
		private IUserProfileCreator ProfileCreator { get; }

		public RegisterAPIKeyPlugin(PluginInputReader inputReader, IUserChecker userChecker, IUserPasswordCreator passwordCreator, IUserProfileCreator profileCreator) {
			InputReader = inputReader;
			UserChecker = userChecker;
			PasswordCreator = passwordCreator;
			ProfileCreator = profileCreator;
		}

		public RegisterAPIKeyPlugin(PluginInputReader inputReader, IUserDBHandler userDBHandler) {
			InputReader = inputReader;
			UserChecker = userDBHandler;
			PasswordCreator = userDBHandler;
			ProfileCreator = userDBHandler;
		}

		public int Invoke(object[] args) {

			var userName = InputReader.ReadUserInput("Please enter a name you want to register");

			if (UserChecker.UserExists(userName)) {
				Console.Error.WriteLine($"A user with name {userName} already exists. Please choose another name.");
				return 1;
			}

			var passwd = PasswordCreator.CreateUserPassword();

			var apiKey = InputReader.ReadUserInput("Please enter the API key you want to use in the application");

			return ProfileCreator.CreateProfile(userName, passwd, apiKey);
		}
	}

	class LoginPlugin : IPlugin {
		public string Name => "Login to the application";

		public string Description => "Logs in as user";

		private PluginInputReader InputReader { get; }

		private IUserChecker UserChecker { get; }

		public LoginPlugin(PluginInputReader inputReader, IUserChecker userChecker) {
			InputReader = inputReader;
			UserChecker = userChecker;
		}

		public int Invoke(object[] args) {
			var userLogin = InputReader.ReadUserInput("Enter your username");
			if (! UserChecker.UserExists(userLogin)) {
				Console.Error.WriteLine($"User with login \"{userLogin}\" does not exists. Please register first.");
				return 1;
			}

			var user = UserChecker.GetProfile(userLogin);
			int passwordAttempts = 5;
			bool loginSuccess;
			bool tryAnotherAttempt;

			do {
				var userPasswd = InputReader.ReadUserInput($"Enter your password ({passwordAttempts} attempts left)");
				userPasswd = Encryptor.Encrypt(userPasswd);

				loginSuccess = user.Password == userPasswd;

				tryAnotherAttempt = passwordAttempts-- > 0 && ! loginSuccess;

			} while (tryAnotherAttempt);

			if (! loginSuccess) {
				Console.Error.WriteLine("Too many incorrect attempts. Please try again.");
				return 1;
			}

			Session.ActiveSession.LoggedUser = user;
			return 0;
		}
	}

	class LogoutPlugin : IPlugin {
		public string Name => "Log out";

		public string Description => "Logs out the current user.";

		public int Invoke(object[] args) {
			var loggedUser = Session.ActiveSession.LoggedUser;

			if (loggedUser != null) {
				Session.ActiveSession.LoggedUser = null;
				Console.WriteLine("Logged out successfully.");
			} else {
				Console.WriteLine("Already logged out.");
			}

			return 0;
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

			var loggedUser = Session.ActiveSession.LoggedUser;

			if (loggedUser == null) {
				Console.WriteLine("Please log in to your account first");
				var loginExitCode = new LoginPlugin(InputReader, UserChecker)
					.Invoke(args);

				if (loginExitCode != 0) { return loginExitCode; }
			}

			// login was successful
			loggedUser = Session.ActiveSession.LoggedUser;

			if (loggedUser == null) { throw new ArgumentNullException("Logged user cannot be null"); }

			var airportIcaoCode = InputReader.ReadUserInput("Please enter the ICAO code of your favourite airport");

			if (loggedUser.FavouriteAirports.Contains(airportIcaoCode)) {
				Console.WriteLine("This airport is already marked as favourite.");
				return 0;
			}

			AirportFinder.FindAirportByCode(airportIcaoCode);

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
