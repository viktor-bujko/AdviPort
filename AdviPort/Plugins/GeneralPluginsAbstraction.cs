using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AdviPort.UI;

namespace AdviPort.Plugins {

	/// <summary>
	/// Represents an application extension, which has an invokable action.
	/// </summary>
	interface IExecutablePlugin {
		/// <summary>
		/// Main action of the plugin. This method represents the main method of
		/// any class which implements <see cref="IExecutablePlugin"/>. 
		/// </summary>
		/// <returns>Exit code of the action. Any non-zero value represents an error.</returns>
		int Invoke();
	}

	/// <summary>
	/// Extension of an <see cref="IExecutablePlugin"/>, which provides its name and description.
	/// </summary>
	interface IPlugin : IExecutablePlugin {
		string Name { get; }
		string Description { get; }
	}

	/// <summary>
	/// Extension of <see cref="IPlugin"/> used to describe a plugin available only 
	/// when no user is logged into the application.
	/// </summary>
	interface ILoggedOffOnlyPlugin : IPlugin { }

	/// <summary>
	/// Extension of <see cref="IPlugin"/> used to describe a plugin available only 
	/// when a user is logged into the application. This class also provides a default
	/// implementation which ensures a user is logged in.
	/// </summary>
	abstract class LoggedInOnlyPlugin : IPlugin {
		public abstract string Name { get; }

		public abstract string Description { get; }

		protected virtual ILoginHandler LoginHandler { get; }

		protected LoggedInOnlyPlugin(ILoginHandler loginHandler) {
			LoginHandler = loginHandler;
		}

		protected LoggedInOnlyPlugin() { }

		public virtual int Invoke() {
			UserProfile loggedUser;
			if (!Session.ActiveSession.HasLoggedUser) {
				Console.WriteLine("Please log in to your account first");
				loggedUser = LoginHandler.LogIn();
			} else {
				loggedUser = Session.ActiveSession.LoggedUser;
			}

			if (loggedUser == null) { return 1; }

			return 0;
		}
	}

	/// <summary>
	/// Provides methods which search, retrieve and select different plugins
	/// based on the input parameters. This class assumes fixed mapping of 
	/// plugin names to their actual implementation.
	/// </summary>
	internal static class PluginSelector {

		/// <summary>
		/// Chooses a plugin implementation based on the plugin mapping name.
		/// </summary>
		/// <param name="pluginName">The name of the plugin used in the mapping.</param>
		/// <returns>An instance of <see cref="IPlugin"/> or <code>null</code> if no 
		/// suitable plugin implementation was found for given <paramref name="pluginName"/></returns>
		internal static IPlugin GetPluginByName(string pluginName) {

			FileSystemProfileDB appDatabase = new FileSystemProfileDB();
			//FileSystemProfileDBWriter profileWriter = new FileSystemProfileDBWriter();
			UserInputReader inputReader = new UserInputReader();
			AeroDataBoxProvider infoProvider = new AeroDataBoxProvider();

			IPlugin plugin = pluginName switch {
				"about" => AboutAppPlugin.Instance,
				"exit" => ExitAppPlugin.Instance,
				"register" => RegisterProfilePlugin.GetInstance(
					inputReader,
					appDatabase,
					DefaultUserPasswordCreator.GetInstance(inputReader),
					appDatabase
				),
				"login" => LoginPlugin.GetInstance(
					inputReader,
					appDatabase
				),
				"logout" => LogoutPlugin.Instance,
				"add_favourite" => AddFavouriteAirportPlugin.GetInstance(
					inputReader,
					infoProvider,
					appDatabase,
					appDatabase.ProfileWriter
				),
				"remove_favourite" => RemoveFavouriteAirportPlugin.GetInstance(
					inputReader,
					appDatabase,
					appDatabase.ProfileWriter
				),
				"print_schedule" => PrintScheduleAirport.GetInstance(
					inputReader,
					infoProvider,
					appDatabase
				),
				"search_by_flight" => new SearchFlightPlugin(
					inputReader,
					infoProvider,
					appDatabase.ProfileWriter
				),
				"change_mainpage" => new ChangeMainPageStylePlugin(
					inputReader,
					appDatabase,
					appDatabase.ProfileWriter
				),
				"airport_info" => new AirportInfoPlugin(
					inputReader,
					infoProvider,
					infoProvider
				),
				_ => null
			};

			return plugin;
		}

		/// <summary>
		/// Retrieves and filters 
		/// </summary>
		/// <param name="settings"></param>
		/// <returns></returns>
		public static IReadOnlyList<IPlugin> GetAvailablePlugins(GeneralApplicationSettings settings, Predicate<IPlugin> pluginFilter) {

			List<IPlugin> result = new List<IPlugin>(settings.AvailablePlugins.Length);

			foreach (string pluginName in settings.AvailablePlugins) {
				var plugin = GetPluginByName(pluginName);
				if (plugin == null) continue;

				if (pluginFilter(plugin)) result.Add(plugin);
			}

			return result;
		}

		/// <summary>
		/// Default filter which decides whether a given plugin is available for both logged-in and logged-off
		/// users.
		/// </summary>
		/// <param name="plugin">The plugin to be considered.</param>
		/// <returns>True if <paramref name="plugin"/> is available for all users.</returns>
		internal static bool LoginLogoutFilter(IPlugin plugin) {
			if (!(plugin is LoggedInOnlyPlugin && plugin is ILoggedOffOnlyPlugin)) {
				if (!Session.ActiveSession.HasLoggedUser && plugin is LoggedInOnlyPlugin) return false;
				if (Session.ActiveSession.HasLoggedUser && plugin is ILoggedOffOnlyPlugin) return false;
			}

			return true;
		}
		

		/// <summary></summary>
		/// <param name="input"></param>
		/// <param name="plugins">Input collection of plugins to choose from.</param>
		/// <param name="filteredPlugins">A list of filtered </param>
		/// <returns>Flag whether a plugin could be found by the <paramref name="input"/> string.</returns>
		internal static bool TryGetPluginFromInputString(string input, IReadOnlyList<IPlugin> plugins, out List<IPlugin> filteredPlugins) {
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
