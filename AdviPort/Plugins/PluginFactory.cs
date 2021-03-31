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
				"about" => AboutAppPlugin.Instance,
				"exit" => ExitAppPlugin.Instance,
				"register" => RegisterAPIKeyPlugin.GetInstance(
					inputReader,
					appDatabase,
					DefaultUserInputPasswordCreator.GetInstance(inputReader),
					appDatabase
				),
				"login" => LoginPlugin.GetInstance(
					inputReader, 
					appDatabase
				),
				"logout" => LogoutPlugin.Instance,
				"add_favourite" => AddFavouriteAirportPlugin.GetInstance(
					inputReader, 
					new AeroDataBoxProvider(),
					appDatabase,
					profileWriter
				),
				"remove_favourite" => RemoveFavouriteAirportPlugin.GetInstance(
					inputReader,
					appDatabase,
					profileWriter
				),
				"print_schedule" => PrintScheduleAirport.GetInstance(
					inputReader, 
					new AeroDataBoxProvider(),
					appDatabase
				),
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

				if (! (plugin is LoggedInOnlyPlugin) || ! (plugin is ILoggedOffOnlyPlugin)) {
					if (!Session.ActiveSession.HasLoggedUser && plugin is LoggedInOnlyPlugin) continue;
					if (Session.ActiveSession.HasLoggedUser && plugin is ILoggedOffOnlyPlugin) continue;
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
				filteredPlugins.Add(ExitAppPlugin.Instance);
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
