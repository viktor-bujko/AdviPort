using System;
using System.Collections.Generic;
using System.Text;

namespace AdviPort.Plugins {

	class AddFavouriteAirportPlugin : ILoggedInOnlyPlugin {
		public string Name => "Add a favourite airport";

		public string Description => "Adds an airport into current account's bookmarks";
		protected PluginInputReader InputReader { get; }
		protected IUserChecker UserChecker { get; }
		protected IUserProfileWriter ProfileWriter { get; }
		private IAirportFinder AirportFinder { get; }
		private ILoginHandler LoginHandler { get; }

		public AddFavouriteAirportPlugin(PluginInputReader inputReader, IAirportFinder airportFinder, IUserChecker userChecker, IUserProfileWriter profileWriter) {
			AirportFinder = airportFinder;
			InputReader = inputReader;
			UserChecker = userChecker;
			ProfileWriter = profileWriter;
			LoginHandler = LoginPlugin.GetInstance(InputReader, UserChecker);
		}

		public int Invoke(object[] args) {

			var loggedUser = LoginHandler.LogIn();

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

	class RemoveFavouriteAirportPlugin : ILoggedInOnlyPlugin {
		public string Name => "Remove a favourite airport";

		public string Description => "Removes an airport from current account's bookmarks";
		protected PluginInputReader InputReader { get; }
		protected IUserChecker UserChecker { get; }
		protected IUserProfileWriter ProfileWriter { get; }
		private ILoginHandler LoginHandler { get; }

		public RemoveFavouriteAirportPlugin(PluginInputReader inputReader, IUserChecker userChecker, IUserProfileWriter profileWriter) {
			InputReader = inputReader;
			UserChecker = userChecker;
			ProfileWriter = profileWriter;
			LoginHandler = LoginPlugin.GetInstance(InputReader, UserChecker);
		}

		public int Invoke(object[] args) {

			var loggedUser = LoginHandler.LogIn();

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
