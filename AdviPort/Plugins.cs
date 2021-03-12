using System;
using System.Collections.Generic;
using System.Text;

namespace AdviPort {

	interface IPlugin {
		string Name { get; }
		string Description { get; }

		//TODO: string QueryName { get; } - ak chcem teda dopísať modul kedy sa bude dať volať priamo plugin z cmdline

		void Invoke(object[] args);
	}

	internal static class PluginSelector {
		internal static IPlugin SearchPluginByName(string pluginName) {
			IPlugin plugin = pluginName switch {
				"register" => new RegisterAPIKeyPlugin(),
				"add_favourite" => new AddFavouriteAirportPlugin(),
				"remove_favourite" => new RemoveFavouriteAirportPlugin(),
				"select_airport" => new SelectAirportPlugin(),
				"pinpoint" => new PinpointAirportPlugin(),
				"print_schedule" => new PrintScheduleAirport(),
				"about" => new AboutAppPlugin(),
				"exit" => new ExitAppPlugin(),
				"search_by_flight" => new SearchByFlightPlugin(),
				"save_flight_info" => new SaveFlightInfoPlugin(),
				"airport_info" => new AirportInfoPlugin(),
				"aircraft_info" => new AircraftInfoPlugin(),
				_ => null
			};

			return plugin;
		}

		internal static bool TryGetPluginFromInputString(string input, IPlugin[] plugins, out List<IPlugin> filteredPlugins) {
			filteredPlugins = new List<IPlugin>();

			for (int i = 0; i < plugins.Length; i++) {
				bool inputIsSubstring = plugins[i].Name.ToLower().StartsWith(input.ToLower());

				if (inputIsSubstring) filteredPlugins.Add(plugins[i]);
			}

			return filteredPlugins.Count == 1;
		}
	}

	class AboutAppPlugin : IPlugin {
		public string Name => "About Application";

		public string Description => "Prints information about application.";

		public void Invoke(object[] args) {
			Console.WriteLine("Hello From about!");
		}
	}

	class ExitAppPlugin : IPlugin {
		public string Name => "Exit Application";

		public string Description => "Quits the application";

		public void Invoke(object[] args) {
			Console.WriteLine("Hello From Exit!");
		}
	}

	class RegisterAPIKeyPlugin : IPlugin {
		public string Name => "Register API key";

		public string Description => "Registers a new user and his / her API key";

		public void Invoke(object[] args) {
			Console.WriteLine("Hello From Register!");
		}
	}

	class AddFavouriteAirportPlugin : IPlugin {
		public string Name => "Add a favourite airport";

		public string Description => "Adds an airport into current account's bookmarks";

		public void Invoke(object[] args) {
			Console.WriteLine($"Hello From {Name}!");
		}
	}

	class RemoveFavouriteAirportPlugin : IPlugin {
		public string Name => "Remove a favourite airport";

		public string Description => "Removes an airport from current account's bookmarks";

		public void Invoke(object[] args) {
			Console.WriteLine($"Hello From {Name}!");
		}
	}

	class SelectAirportPlugin : IPlugin {
		public string Name => "Select an airport";

		public string Description => "Selects an airport to work with";

		public void Invoke(object[] args) {
			Console.WriteLine($"Hello From {Name}!");
		}
	}

	class PinpointAirportPlugin : IPlugin {
		public string Name => "Pinpoint an airport";

		public string Description => "Default description";

		public void Invoke(object[] args) {
			Console.WriteLine($"Hello From {Name}!");
		}
	}

	class PrintScheduleAirport : IPlugin {
		public string Name => "Print the flights schedule of a selected airport";

		public string Description => "Prints the flights schedule for selected airport";

		public void Invoke(object[] args) {
			Console.WriteLine($"Hello From {Name}!");
		}
	}

	class SearchByFlightPlugin : IPlugin {
		public string Name => "Search for a flight by the flight number (e.g. AF 1438)";

		public string Description => "Searches for a concrete flight (e.g. AF 1438)";

		public void Invoke(object[] args) {
			Console.WriteLine($"Hello From {Name}!");
		}
	}

	class SaveFlightInfoPlugin : IPlugin {
		public string Name => "Create a bookmark for a given flight";

		public string Description => "Moves a flight into the followed ones";

		public void Invoke(object[] args) {
			Console.WriteLine($"Hello From {Name}!");
		}
	}

	class AirportInfoPlugin : IPlugin {
		public string Name => "Print basic information about a specified airport";

		public string Description => "Prints available information about an airport";

		public void Invoke(object[] args) {
			Console.WriteLine($"Hello From {Name}!");
		}
	}

	class AircraftInfoPlugin : IPlugin {
		public string Name => "Get information about different types of airplanes";

		public string Description => "Prints available information about an aircraft";

		public void Invoke(object[] args) {
			Console.WriteLine($"Hello From {Name}!");
		}
	}

}
