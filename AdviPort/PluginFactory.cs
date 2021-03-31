using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using AdviPort.Plugins;

namespace AdviPort {

	internal static class PluginSelector {
		internal static IPlugin SearchPluginByName(string pluginName) {

			FileSystemProfileDB appDatabase = new FileSystemProfileDB();
			FileSystemProfileDBWriter profileWriter = new FileSystemProfileDBWriter();
			PluginInputReader inputReader = new PluginInputReader();

			IPlugin plugin = pluginName switch {
				"register" => new RegisterAPIKeyPlugin(
					inputReader,
					appDatabase,
					DefaultUserInputPasswordCreator.GetInstance(inputReader),
					appDatabase
				),
				"login" => LoginPlugin.GetInstance(
					inputReader, 
					appDatabase
				),
				"logout" => new LogoutPlugin(),
				"add_favourite" => new AddFavouriteAirportPlugin(
					inputReader, 
					new RapidAPIAirportInfoFinder(),
					appDatabase,
					profileWriter
				),
				"remove_favourite" => new RemoveFavouriteAirportPlugin(
					inputReader,
					appDatabase,
					profileWriter
				),
				"print_schedule" => new PrintScheduleAirport(),
				"about" => new AboutAppPlugin(),
				"exit" => new ExitAppPlugin(),
				"search_by_flight" => new SearchByFlightPlugin(),
				"save_flight_info" => new SaveFlightInfoPlugin(),
				"airport_info" => new AirportInfoPlugin(inputReader, appDatabase),
				"aircraft_info" => new AircraftInfoPlugin(),
				_ => null
			};

			return plugin;
		}

		public static IList<IPlugin> GetAvailablePlugins(GeneralApplicationSettings settings) {

			List<IPlugin> plugins = new List<IPlugin>(settings.AvailablePlugins.Length);

			foreach (string pluginName in settings.AvailablePlugins) {
				var plugin = SearchPluginByName(pluginName);
				if (plugin is null) continue;

				if (! (plugin is ILoggedInOnlyPlugin) || ! (plugin is ILoggedOffPlugin)) {
					if (!Session.ActiveSession.HasLoggedUser && plugin is ILoggedInOnlyPlugin) continue;
					if (Session.ActiveSession.HasLoggedUser && plugin is ILoggedOffPlugin) continue;
				}

				plugins.Add(plugin);
			}

			return plugins;
		}

		internal static bool TryGetPluginFromInputString(string input, IList<IPlugin> plugins, out List<IPlugin> filteredPlugins) {
			filteredPlugins = new List<IPlugin>();
			input = input.ToLower();

			if (string.IsNullOrEmpty(input)) return false;

			if (input == "exit" || input == "quit") {
				filteredPlugins.Add(new ExitAppPlugin());
				return true;
			}

			for (int i = 0; i < plugins.Count; i++) {

				string pluginNameFirstWord = plugins[i].Name.Split()[0].ToLower();
				bool matchesFirstWord = pluginNameFirstWord == input;

				if (matchesFirstWord) filteredPlugins.Add(plugins[i]);
			}

			return filteredPlugins.Count == 1;
		}
	}
}
