using System;
using System.Collections.Generic;
using System.Text;

namespace AdviPort.Plugins {

	class AddFavouriteAirportPlugin : LoggedInOnlyPlugin {

		private static AddFavouriteAirportPlugin Instance { get; set; }
		public override string Name => "Add a favourite airport";
		public override string Description => "Adds an airport into current account's bookmarks";
		protected IUserInterfaceReader InputReader { get; }
		protected IUserProfileWriter ProfileWriter { get; }
		private IAirportProvider AirportFinder { get; }

		private AddFavouriteAirportPlugin(IUserInterfaceReader inputReader, IAirportProvider airportFinder, IUserChecker userChecker, IUserProfileWriter profileWriter) : base(LoginPlugin.GetInstance(inputReader, userChecker)) {
			AirportFinder = airportFinder;
			InputReader = inputReader;
			ProfileWriter = profileWriter;
		}

		public static AddFavouriteAirportPlugin GetInstance(IUserInterfaceReader inputReader, IAirportProvider airportFinder, IUserChecker userChecker, IUserProfileWriter profileWriter) {
			if (Instance == null) {
				Instance = new AddFavouriteAirportPlugin(inputReader, airportFinder, userChecker, profileWriter);
			}

			return Instance;
		}

		public override int Invoke(object[] args) {

			UserProfile loggedUser;

			if (!Session.ActiveSession.HasLoggedUser) {
				Console.WriteLine("Please log in to your account first");
				loggedUser = LoginHandler.LogIn();
			} else {
				loggedUser = Session.ActiveSession.LoggedUser;
			}

			if (loggedUser == null) { return 1; }

			var airportIcaoCode = InputReader.ReadUserInput($"{loggedUser.UserName}, please enter the ICAO code of your favourite airport");

			if (string.IsNullOrWhiteSpace(airportIcaoCode)) { return 0; }

			if (loggedUser.FavouriteAirports.ContainsKey(airportIcaoCode)) {
				Console.WriteLine("This airport is already marked as favourite.");
				return 0;
			}

			var airport = AirportFinder.GetAirportByICAO(airportIcaoCode);

			loggedUser.FavouriteAirports.Add(airport.ICAO.ToLower(), airport);

			ProfileWriter.WriteUserProfile(loggedUser);

			return 0;
		}
	}

	class RemoveFavouriteAirportPlugin : LoggedInOnlyPlugin {

		private static RemoveFavouriteAirportPlugin Instance { get; set; }
		public override string Name => "Remove a favourite airport";
		public override string Description => "Removes an airport from current account's bookmarks";
		protected IUserInterfaceReader InputReader { get; }
		protected IUserProfileWriter ProfileWriter { get; }

		private RemoveFavouriteAirportPlugin(IUserInterfaceReader inputReader, IUserChecker userChecker, IUserProfileWriter profileWriter) : base(LoginPlugin.GetInstance(inputReader, userChecker)) {
			InputReader = inputReader;
			ProfileWriter = profileWriter;
		}

		public static RemoveFavouriteAirportPlugin GetInstance(IUserInterfaceReader inputReader, IUserChecker userChecker, IUserProfileWriter profileWriter) {
			if (Instance == null) {
				Instance = new RemoveFavouriteAirportPlugin(inputReader, userChecker, profileWriter);
			}

			return Instance;
		}

		public override int Invoke(object[] args) {

			int baseRetVal = base.Invoke(args);

			if (baseRetVal != 0) { return baseRetVal; }

			var loggedUser = Session.ActiveSession.LoggedUser;

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
