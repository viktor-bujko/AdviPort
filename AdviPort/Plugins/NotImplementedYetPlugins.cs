using System;
using System.Collections.Generic;
using System.Text;

namespace AdviPort.Plugins {

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

		private PluginInputReader InputReader { get; }
		private IUserChecker UserChecker { get; }

		public AirportInfoPlugin(PluginInputReader inputReader, IUserChecker userChecker) {
			InputReader = inputReader;
			UserChecker = userChecker;
		}

		public int Invoke(object[] args) {
			var loggedUser = Session.ActiveSession.LoggedUser;

			if (loggedUser == null) {
				Console.WriteLine("Please log in to your account first");
				var loginExitCode = new LoginPlugin(InputReader, UserChecker).Invoke(args);

				if (loginExitCode != 0) { return loginExitCode; }
			}

			// login was successful
			loggedUser = Session.ActiveSession.LoggedUser;

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
