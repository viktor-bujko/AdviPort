using System;
using System.Collections.Generic;
using System.Text;

namespace AdviPort.Plugins {

	class SearchByFlightPlugin : LoggedInOnlyPlugin {
		public override string Name => "Search for a flight by the flight number (e.g. AF 1438)";

		public override string Description => "Searches for a concrete flight (e.g. AF 1438)";

		public override int Invoke(object[] args) {
			Console.WriteLine($"Hello From {Name}!");
			return 0;
		}
	}

	class SaveFlightInfoPlugin : LoggedInOnlyPlugin {
		public override string Name => "Create a bookmark for a given flight";

		public override string Description => "Moves a flight into the followed ones";

		public override int Invoke(object[] args) {
			Console.WriteLine($"Hello From {Name}!");
			return 0;
		}
	}

	class AirportInfoPlugin : LoggedInOnlyPlugin {
		public override string Name => "Print basic information about a specified airport";

		public override string Description => "Prints available information about an airport";

		private PluginInputReader InputReader { get; }
		private IUserChecker UserChecker { get; }

		public AirportInfoPlugin(PluginInputReader inputReader, IUserChecker userChecker) {
			InputReader = inputReader;
			UserChecker = userChecker;
		}

		public override int Invoke(object[] args) {

			int baseRetVal = base.Invoke(args);

			return 0;
		}
	}

	class AircraftInfoPlugin : LoggedInOnlyPlugin {
		public override string Name => "Get information about different types of airplanes";

		public override string Description => "Prints available information about an aircraft";

		public override int Invoke(object[] args) {
			Console.WriteLine($"Hello From {Name}!");
			return 0;
		}
	}
}
