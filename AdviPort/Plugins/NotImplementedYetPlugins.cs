using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AdviPort.ResponseObjects;

namespace AdviPort.Plugins {

	class SearchByFlightPlugin : LoggedInOnlyPlugin {
		public override string Name => "Search for a flight by the flight number (e.g. AF 1438)";

		public override string Description => "Searches for a concrete flight (e.g. AF 1438)";

		private PluginInputReader InputReader { get; }
		private IFlightInfoProvider FlightInfoFinder { get; }

		public SearchByFlightPlugin(PluginInputReader inputReader, IFlightInfoProvider flightInfoProvider) {
			InputReader = inputReader;
			FlightInfoFinder = flightInfoProvider;
		}

		public override int Invoke() {
			int baseRetVal = base.Invoke();

			if (baseRetVal != 0) return baseRetVal;

			var loggedUser = Session.ActiveSession.LoggedUser;

			string flightNumber = InputReader.ReadUserInput($"{loggedUser.UserName}, please enter the flight number of flight you want to get info for");
			string flightDate = InputReader.ReadUserInput($"{loggedUser.UserName}, specify the date to search for info or press enter to use default date ({DateTime.Now.ToShortDateString()})");

			if (! DateTime.TryParse(flightDate, out DateTime date)) {
				date = DateTime.Today;
			}

			if (string.IsNullOrWhiteSpace(flightNumber)) return 0;

			var flightInfoTask = FlightInfoFinder.GetFlightInfoAsync<Flight>(flightNumber, date);

			var flightInfos = flightInfoTask.Result;

			if (flightInfos.Length == 0) {
				Console.Error.WriteLine($"No flight with given flight number ({flightNumber}) was found.");
				Console.Error.WriteLine("Please check for potential errors in the given callsign.");
				return 1;
			}

			bool isArrival = flightInfos[0].Arrival.ActualTimeLocal != null;

			Console.WriteLine(flightInfos[0].BuildFlightString(null, isArrival));
			return 0;
		}
	}

	class SaveFlightInfoPlugin : LoggedInOnlyPlugin {
		public override string Name => "Create a bookmark for a given flight";

		public override string Description => "Moves a flight into the followed ones";

		public override int Invoke() {
			Console.WriteLine($"Hello From {Name}! This is not finished yet");
			return 0;
		}
	}

	class AirportInfoPlugin : LoggedInOnlyPlugin {
		public override string Name => "Print basic information about a specified airport";

		public override string Description => "Prints available information about an airport";

		private PluginInputReader InputReader { get; }
		private IAirportProvider AirportFinder { get; }
		private IRunwayInfoProvider RunwayFinder { get; }


		public AirportInfoPlugin(PluginInputReader inputReader, IAirportProvider airportProvider, IRunwayInfoProvider runwayInfoProvider) {
			InputReader = inputReader;
			AirportFinder = airportProvider;
			RunwayFinder = runwayInfoProvider;
		}

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

			var airport = airportTask.Result;
			var runways = runwayTask.Result;

			Console.WriteLine(airport.BuildAirportInfoTable(runways, includeRWYs));
			return 0;
		}
	}

	class AircraftInfoPlugin : LoggedInOnlyPlugin {
		public override string Name => "Get information about different types of airplanes";

		public override string Description => "Prints available information about an aircraft";

		public override int Invoke() {
			Console.WriteLine($"Hello From {Name}!  This is not finished yet");
			return 0;
		}
	}
}
