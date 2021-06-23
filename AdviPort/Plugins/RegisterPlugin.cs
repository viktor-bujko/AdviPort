using System;
using System.Collections.Generic;
using System.Text;

namespace AdviPort.Plugins {
	class RegisterAPIKeyPlugin : ILoggedOffOnlyPlugin {

		private static RegisterAPIKeyPlugin Instance { get; set; }

		public string Name => "Register API key";

		public string Description => "Registers a new user and his / her API key";

		private IUserInterfaceReader InputReader { get; }
		private IUserChecker UserChecker { get; }

		private IUserPasswordCreator PasswordCreator { get; }
		private IUserProfileCreator ProfileCreator { get; }

		private RegisterAPIKeyPlugin(IUserInterfaceReader inputReader, IUserChecker userChecker, IUserPasswordCreator passwordCreator, IUserProfileCreator profileCreator) {
			InputReader = inputReader;
			UserChecker = userChecker;
			PasswordCreator = passwordCreator;
			ProfileCreator = profileCreator;
		}

		internal static RegisterAPIKeyPlugin GetInstance(IUserInterfaceReader inputReader, IUserChecker userChecker, IUserPasswordCreator passwordCreator, IUserProfileCreator profileCreator) {
			if (Instance == null) {
				Instance = new RegisterAPIKeyPlugin(inputReader, userChecker, passwordCreator, profileCreator);
			}

			return Instance;
		}

		public RegisterAPIKeyPlugin(IUserInterfaceReader inputReader, IUserDBHandler userDBHandler) {
			InputReader = inputReader;
			UserChecker = userDBHandler;
			PasswordCreator = userDBHandler;
			ProfileCreator = userDBHandler;
		}

		public int Invoke() {

			var userName = InputReader.ReadUserInput("Please enter a name you want to register");

			if (string.IsNullOrWhiteSpace(userName)) {
				Console.Error.WriteLine("User with empty name cannot be registered. Please use at least one non-whitespace character.");
				return 1;
			}

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
