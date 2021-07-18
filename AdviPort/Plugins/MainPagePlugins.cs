using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AdviPort.UI;

namespace AdviPort.Plugins {

	/// <summary>
	/// Provides an 'About' page. 
	/// </summary>
	class AboutAppPlugin : LoggedInOnlyPlugin, ILoggedOffOnlyPlugin {

		public static AboutAppPlugin Instance { get; } = new AboutAppPlugin();
		public override string Name => "About Application";

		public override string Description => "Prints information about application.";

		private AboutAppPlugin() { }

		/// <summary>
		/// <inheritdoc/>
		/// Shows 'About' page.
		/// </summary>
		/// <returns><inheritdoc/></returns>
		public override int Invoke() {
			string[] paths = GeneralApplicationSettings.SearchFiles(AppDomain.CurrentDomain.BaseDirectory, "about.txt");

			if (paths == null) {
				Console.Error.WriteLine("A required file has not been found.");
				return 1;
			}

			TextReader aboutReader = GeneralApplicationSettings.GetTextReader(paths);

			if (aboutReader == null) {
				Console.Error.WriteLine("Specification file could not be found.");
				return 1;
			}

			Console.WriteLine(aboutReader.ReadToEnd());

			return 0;
		}
	}

	/// <summary>
	/// Represents the plugin which handles proper application termination.
	/// </summary>
	class ExitAppPlugin : LoggedInOnlyPlugin, ILoggedOffOnlyPlugin {

		public static ExitAppPlugin Instance { get; } = new ExitAppPlugin();

		private ExitAppPlugin() { }

		public sealed override string Name => "Exit Application";

		public sealed override string Description => "Quits the application";

		/// <summary>
		/// <inheritdoc/>
		/// Handles application termination.
		/// </summary>
		/// <returns><inheritdoc/></returns>
		public override int Invoke() {
			if (Session.ActiveSession.HasLoggedUser) LogoutPlugin.Instance.Invoke();

			Console.WriteLine("Exiting ADVIPORT application.");
			return 0;
		}
	}

	/// <summary>
	/// Changes the design of the main application page.
	/// </summary>
	class ChangeMainPageStylePlugin : LoggedInOnlyPlugin {
		public override string Name => "Change the design of the main page.";

		public override string Description => "Changes the style of the main page to one of the predefined styles.";

		private IUserProfileWriter ProfileWriter { get; set; }

		public ChangeMainPageStylePlugin(IUserInterfaceReader inputReader, IUserChecker userChecker, IUserProfileWriter profileWriter) :
			base(LoginPlugin.GetInstance(inputReader, userChecker)) {
			ProfileWriter = profileWriter;
		}

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		/// <returns><inheritdoc/></returns>
		public override int Invoke() {
			int baseRetVal = base.Invoke();

			if (baseRetVal != 0) return baseRetVal;

			Session.ActiveSession.LoggedUser.SetMainPageStyle(ProfileWriter);
			return 0;
		}
	}
}
