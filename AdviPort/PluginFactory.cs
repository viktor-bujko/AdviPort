using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace AdviPort {

	internal static class PluginSelector {
		internal static IPlugin SearchPluginByName(string pluginName, TextReader reader, TextWriter writer) {

			FileSystemProfileDB appDatabase = new FileSystemProfileDB();
			PluginInputReader inputReader = new PluginInputReader(reader, writer);

			IPlugin plugin = pluginName switch {
				"register" => new RegisterAPIKeyPlugin(
					inputReader,
					appDatabase,
					DefaultUserInputPasswordCreator.NewInstance(inputReader),
					appDatabase
				),
				"login" => new LoginPlugin(
					inputReader, 
					appDatabase
				),
				"logout" => new LogoutPlugin(),
				"add_favourite" => new AddFavouriteAirportPlugin(
					inputReader, 
					new DefaultAirportInfoFinder(),
					appDatabase
				),
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

		public static IPlugin[] GetAvailablePlugins(GeneralApplicationSettings settings, TextReader reader, TextWriter writer) {

			List<IPlugin> plugins = new List<IPlugin>(settings.AvailablePlugins.Length);

			foreach (string pluginName in settings.AvailablePlugins) {
				var plugin = SearchPluginByName(pluginName, reader, writer);
				if (plugin is null) continue;

				plugins.Add(plugin);
			}

			return plugins.ToArray();
		}

		internal static bool TryGetPluginFromInputString(string input, IPlugin[] plugins, out List<IPlugin> filteredPlugins) {
			filteredPlugins = new List<IPlugin>();
			input = input.ToLower();

			if (string.IsNullOrEmpty(input)) return false;

			if (input == "exit" || input == "quit") {
				filteredPlugins.Add(new ExitAppPlugin());
				return true;
			}

			for (int i = 0; i < plugins.Length; i++) {
				bool inputIsSubstring = plugins[i].Name.ToLower().StartsWith(input);

				if (inputIsSubstring) filteredPlugins.Add(plugins[i]);
			}

			return filteredPlugins.Count == 1;
		}
	}
}
