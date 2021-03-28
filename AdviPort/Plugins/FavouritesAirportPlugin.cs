using System;
using System.Collections.Generic;
using System.Text;

namespace AdviPort.Plugins {
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
}
