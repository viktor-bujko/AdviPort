using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AdviPort.Plugins {

	class AboutAppPlugin : LoggedInOnlyPlugin, ILoggedOffOnlyPlugin {

		public static AboutAppPlugin Instance { get; } = new AboutAppPlugin();
		public override string Name => "About Application";

		public override string Description => "Prints information about application.";

		private AboutAppPlugin() { }

		public override int Invoke(object[] args) {
			string[] paths = GeneralApplicationSettings.SearchFiles(Directory.GetCurrentDirectory(), "about.txt", requiredFiles: 1);

			if (paths is null) throw new FileNotFoundException("A required file has not been found.");

			TextReader aboutReader = GeneralApplicationSettings.GetTextReader(paths);

			if (aboutReader is null) {
				Console.Error.WriteLine("Specification file could not be found.");
				return 1;
			}

			Console.WriteLine(aboutReader.ReadToEnd());

			return 0;
		}
	}

	class ExitAppPlugin : LoggedInOnlyPlugin, ILoggedOffOnlyPlugin {

		public static ExitAppPlugin Instance { get; } = new ExitAppPlugin();

		private ExitAppPlugin() { }

		public override string Name => "Exit Application";

		public override string Description => "Quits the application";

		public override int Invoke(object[] args) {
			Console.WriteLine("Exiting ADVIPORT application.");
			return 0;
		}
	}
}
