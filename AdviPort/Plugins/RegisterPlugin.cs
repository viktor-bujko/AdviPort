using System;
using System.Collections.Generic;
using System.Text;

namespace AdviPort.Plugins {
	class RegisterAPIKeyPlugin : IPlugin {
		public string Name => "Register API key";

		public string Description => "Registers a new user and his / her API key";

		private PluginInputReader InputReader { get; }
		private IUserChecker UserChecker { get; }

		private IUserPasswordCreator PasswordCreator { get; }
		private IUserProfileCreator ProfileCreator { get; }

		public RegisterAPIKeyPlugin(PluginInputReader inputReader, IUserChecker userChecker, IUserPasswordCreator passwordCreator, IUserProfileCreator profileCreator) {
			InputReader = inputReader;
			UserChecker = userChecker;
			PasswordCreator = passwordCreator;
			ProfileCreator = profileCreator;
		}

		public RegisterAPIKeyPlugin(PluginInputReader inputReader, IUserDBHandler userDBHandler) {
			InputReader = inputReader;
			UserChecker = userDBHandler;
			PasswordCreator = userDBHandler;
			ProfileCreator = userDBHandler;
		}

		public int Invoke(object[] args) {

			var userName = InputReader.ReadUserInput("Please enter a name you want to register");

			if (UserChecker.UserExists(userName)) {
				Console.Error.WriteLine($"A user with name {userName} already exists. Please choose another name.");
				return 1;
			}

			var passwd = PasswordCreator.CreateUserPassword();

			var apiKey = InputReader.ReadUserInput("Please enter the API key you want to use in the application");

			return ProfileCreator.CreateProfile(userName, passwd, apiKey);
		}
	}
}
