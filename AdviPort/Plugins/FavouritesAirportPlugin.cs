using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace AdviPort.Plugins {

	abstract class LoggedAirportPlugin {
		protected PluginInputReader InputReader { get; }
		protected IUserChecker UserChecker { get; }
		protected IUserProfileWriter ProfileWriter { get; }

		protected LoggedAirportPlugin(PluginInputReader inputReader, IUserChecker userChecker, IUserProfileWriter profileWriter) {
			InputReader = inputReader;
			UserChecker = userChecker;
			ProfileWriter = profileWriter;
		}

		protected UserProfile LogInAUser() {
			var loggedUser = Session.ActiveSession.LoggedUser;

			if (loggedUser == null) {
				Console.WriteLine("Please log in to your account first");
				var loginExitCode = new LoginPlugin(InputReader, UserChecker)
					.Invoke(null);

				if (loginExitCode != 0) { return null; }
				Thread.Sleep(500);
			}

			// login was successful
			Console.Clear();
			loggedUser = Session.ActiveSession.LoggedUser;

			return loggedUser;
		}
	}

	class AddFavouriteAirportPlugin : LoggedAirportPlugin, IPlugin {
		public string Name => "Add a favourite airport";

		public string Description => "Adds an airport into current account's bookmarks";

		private IAirportFinder AirportFinder { get; }

		public AddFavouriteAirportPlugin(PluginInputReader inputReader, IAirportFinder airportFinder, IUserChecker userChecker, IUserProfileWriter profileWriter) : base(inputReader, userChecker, profileWriter) {
			AirportFinder = airportFinder;
		}

		public int Invoke(object[] args) {

			var loggedUser = LogInAUser();

			if (loggedUser == null) { return 1; }

			var airportIcaoCode = InputReader.ReadUserInput($"{loggedUser.UserName}, please enter the ICAO code of your favourite airport");

			if (string.IsNullOrWhiteSpace(airportIcaoCode)) { return 0; }

			if (loggedUser.FavouriteAirports.ContainsKey(airportIcaoCode)) {
				Console.WriteLine("This airport is already marked as favourite.");
				return 0;
			}

			AirportFinder.SetFavouriteAirportByICAO(airportIcaoCode, ProfileWriter);

			return 0;
		}
	}

	class RemoveFavouriteAirportPlugin : LoggedAirportPlugin, IPlugin {
		public string Name => "Remove a favourite airport";

		public string Description => "Removes an airport from current account's bookmarks";

		public RemoveFavouriteAirportPlugin(PluginInputReader inputReader, IUserChecker userChecker, IUserProfileWriter profileWriter) : base(inputReader, userChecker, profileWriter) {
		}

		public int Invoke(object[] args) {

			var loggedUser = LogInAUser();

			if (loggedUser == null) { return 1; }

			var icaoToRemove = InputReader.ReadUserInput($"{loggedUser.UserName}, please enter the ICAO code of the airport you want to remove from favourites");

			var airportToRemove = icaoToRemove.ToLower();

			if (! loggedUser.FavouriteAirports.Remove(airportToRemove)) {
				Console.Error.WriteLine($"Airport with ICAO code \"{icaoToRemove}\" could not be removed successfully. Please make sure you entered a correct ICAO code.");
				return 1;
			} 
			
			return ProfileWriter.WriteUserProfile(loggedUser);
		}
	}
}
