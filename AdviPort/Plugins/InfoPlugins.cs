using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AdviPort.ResponseObjects;
using AdviPort.UI;

namespace AdviPort.Plugins {

	/// <summary>
	/// Class representing a plugin responsible for printing current state of a flights schedule 
	/// for given airport.
	/// </summary>
	class PrintScheduleAirport : LoggedInOnlyPlugin {

		private static PrintScheduleAirport Instance { get; set; }

		public override string Name => "Print the flights schedule of a selected airport";

		public override string Description => "Prints the flights schedule for selected airport";

		private IUserInterfaceReader InputReader { get; }
		private IAirportScheduleProvider ScheduleProvider { get; }

		private PrintScheduleAirport(IUserInterfaceReader inputReader, IAirportScheduleProvider scheduleProvider, IUserChecker userChecker) 
			: base(LoginPlugin.GetInstance(inputReader, userChecker)) {
			InputReader = inputReader;
			ScheduleProvider = scheduleProvider;
		}

		public static PrintScheduleAirport GetInstance(IUserInterfaceReader inputReader, IAirportScheduleProvider scheduleProvider, IUserChecker userChecker) {
			if (Instance == null) {
				Instance = new PrintScheduleAirport(inputReader, scheduleProvider, userChecker);
			}

			return Instance;
		}

		/// <summary>
		/// <inheritdoc/>
		/// 
		/// This plugin method asks a logged-in user to enter an airport to get the schedule from, creates and sends
		/// appropriate message to the server and finally shows available information back to the user.
		/// </summary>
		/// <returns><inheritdoc/></returns>
		public override int Invoke() {

			int baseRetVal = base.Invoke();
			if (baseRetVal != 0) { return baseRetVal; }

			var airportIcao = InputReader.ReadUserInput("Please enter the ICAO code of the airport to get the schedule from");

			var scheduleTask = ScheduleProvider.GetAirportScheduleAsync<Schedule>(airportIcao);

			try {
				scheduleTask.Wait(); // Avoiding exception in Result property call
			} catch (AggregateException) {
				return 1;
			}

			if (!scheduleTask.IsCompletedSuccessfully) return 1;

			var schedule = scheduleTask.Result;

			schedule.PrintSchedule();

			InputReader.ReadUserInput("Press Enter to continue");
			return 0;
		}

	}

	/// <summary>
	/// Class representing a plugin responsible for searching any available information about a concrete
	/// flight given by the flight number entered by a logged-in user.
	/// </summary>
	class SearchFlightPlugin : LoggedInOnlyPlugin{
		public override string Name {
			get {
				string result = "Search for a flight by the flight number ";
				if (!string.IsNullOrWhiteSpace(Session.ActiveSession.LoggedUser.LastSearchedFlight)) {
					result += $"(e.g. {Session.ActiveSession.LoggedUser.LastSearchedFlight})";
				}

				return result;
			}
		}

		public override string Description {
			get {
				string result = "Searches for a concrete flight ";
				if (!string.IsNullOrEmpty(Session.ActiveSession.LoggedUser.LastSearchedFlight)) {
					result += $" (e.g. {Session.ActiveSession.LoggedUser.LastSearchedFlight})";
				}

				return result;
			}
		}

		private UserInputReader InputReader { get; }
		private IFlightInfoProvider FlightInfoFinder { get; }

		private IUserProfileWriter ProfileWriter { get; }

		public SearchFlightPlugin(UserInputReader inputReader, IFlightInfoProvider flightInfoProvider, IUserProfileWriter profileWriter) {
			InputReader = inputReader;
			FlightInfoFinder = flightInfoProvider;
			ProfileWriter = profileWriter;
		}

		/// <summary>
		/// <inheritdoc/>
		/// 
		/// This plugin action asks the user to enter the flight number of a flight for which the information should 
		/// be retrieved. Request is then sent to the server and its response is finally shown back to the user.
		/// </summary>
		/// <returns><inheritdoc/></returns>
		public override int Invoke() {
			int baseRetVal = base.Invoke();

			if (baseRetVal != 0) return baseRetVal;

			var loggedUser = Session.ActiveSession.LoggedUser;

			string flightNumber = InputReader.ReadUserInput($"{loggedUser.UserName}, please enter the flight number of flight you want to get info for");
			string flightDate = InputReader.ReadUserInput($"{loggedUser.UserName}, specify the date (DD/MM/YYYY) to search for info or press enter to use default date ({DateTime.Now.ToShortDateString()})");

			if (string.IsNullOrWhiteSpace(flightNumber)) return 0;

			if (!DateTime.TryParse(flightDate, out DateTime date)) {
				date = DateTime.Today;
			}

			var flightInfoTask = FlightInfoFinder.GetFlightInfoAsync<Flight>(flightNumber, date);

			try {
				flightInfoTask.Wait(); // Avoiding exception in Result property call
			} catch (AggregateException) {
				return 1;
			}

			if (!flightInfoTask.IsCompletedSuccessfully) return 1;

			IList<IFlight> flightInfos = flightInfoTask.Result;

			if (flightInfos.Count == 0) {
				Console.Error.WriteLine($"No flight with given flight number ({flightNumber}) was found.");
				Console.Error.WriteLine("Please check for potential errors in the given flight number.");
				return 1;
			}

			Console.WriteLine(flightInfos.FlightInfoToString());

			Session.ActiveSession.LoggedUser.LastSearchedFlight = flightNumber.ToUpper();
			Session.ActiveSession.LoggedUser.SchedulesHistory.Add(flightNumber.ToUpper());

			Session.ActiveSession.LoggedUser.TrimFlightsHistory(ProfileWriter);
			return 0;
		}
	}

	/// <summary>
	/// Class representing a plugin responsible for getting any available information about user-entered 
	/// airport.
	/// </summary>
	class AirportInfoPlugin : LoggedInOnlyPlugin {
		public override string Name => "Print basic information about a specified airport";

		public override string Description => "Prints available information about an airport";

		private UserInputReader InputReader { get; }
		private IAirportProvider AirportFinder { get; }
		private IRunwayInfoProvider RunwayFinder { get; }


		public AirportInfoPlugin(UserInputReader inputReader, IAirportProvider airportProvider, IRunwayInfoProvider runwayInfoProvider) {
			InputReader = inputReader;
			AirportFinder = airportProvider;
			RunwayFinder = runwayInfoProvider;
		}

		/// <summary>
		/// <inheritdoc/>
		/// 
		/// This action method asks the logged-in user to enter the code of an airport for which information 
		/// should be retrieved. A suitable request is created and sent to the server. The response is finally 
		/// shown to the user.
		/// </summary>
		/// <returns><inheritdoc/></returns>
		public override int Invoke() {
			int baseRetVal = base.Invoke();

			if (baseRetVal != 0) return baseRetVal;

			var loggedUser = Session.ActiveSession.LoggedUser;

			var airportIcaoCode = InputReader.ReadUserInput($"{loggedUser.UserName}, please enter the ICAO code of the airport to get information");
			bool includeRWYs = InputReader.ReadUserInput($"{loggedUser.UserName}, should runways information be included in the output? (y/N)")
								.Trim().ToLower().StartsWith('y');

			if (string.IsNullOrWhiteSpace(airportIcaoCode)) { return 0; }

			var airportTask = AirportFinder.GetAirportByICAOAsync<Airport>(airportIcaoCode);
			var runwayTask = RunwayFinder.GetAirportRunwayInfoAsync<Runway>(airportIcaoCode);

			Task.WaitAll(airportTask, runwayTask);
			// Avoiding exception in Result property call
			if (!airportTask.IsCompletedSuccessfully || !runwayTask.IsCompletedSuccessfully) return 1;

			var airport = airportTask.Result;
			var runways = runwayTask.Result;

			Console.WriteLine(airport.BuildAirportInfoTable(runways, includeRWYs));
			return 0;
		}
	}
}
