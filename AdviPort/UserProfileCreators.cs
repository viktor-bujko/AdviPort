using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using AdviPort.Plugins;

namespace AdviPort {
	interface IUserProfileCreator {
		int CreateProfile(string userName, string password, string apiKey);
	}

	interface IUserProfileWriter {
		int WriteUserProfile(UserProfile profile);
	}

	interface IUserPasswordCreator {
		string CreateUserPassword();
	}

	class DefaultUserInputPasswordCreator : IUserPasswordCreator {

		private static DefaultUserInputPasswordCreator Instance { get; set; }
		private PluginInputReader Reader { get; }
		private DefaultUserInputPasswordCreator(PluginInputReader inputReader) {
			Reader = inputReader;
		}

		public static DefaultUserInputPasswordCreator GetInstance(PluginInputReader inputReader) {
			if (Instance == null || Instance.Reader != inputReader) {
				// Creating new instance only if it does not exist yet or if the conditions change
				Instance = new DefaultUserInputPasswordCreator(inputReader);
			}

			return Instance;
		}

		public string CreateUserPassword() {

			bool incorrectPassword;
			Regex regex = new Regex("^[a-zA-Z0-9]{8,}$");
			string passwd1;

			do {
				incorrectPassword = true;
				int cursor = Console.CursorTop;
				passwd1 = ConsolePasswordReader.Instance.ReadUserInput("Please enter password you want to use (at least 8 characters - letters and numbers only)");

				if (string.IsNullOrWhiteSpace(passwd1)) {
					Console.Error.WriteLine("The password cannot be empty. Please try again.");
					Thread.Sleep(350);
					ConsolePasswordReader.Instance.ConsoleClearLine(cursor);
					continue; 
				}

				if (!regex.IsMatch(passwd1)) {
					Console.Error.WriteLine("Please make sure your password contains at least 8 characters (letters and numbers only)");
					incorrectPassword = true;
					continue;
				}

				var passwd2 = ConsolePasswordReader.Instance.ReadUserInput("Please type your password again");

				if (passwd1 != passwd2) {
					Console.Error.WriteLine("Passwords do not match. Please try again.");
					incorrectPassword = true;
					continue;
				}

				incorrectPassword = false;

			} while (incorrectPassword);

			return passwd1;
		}
	}


	interface IUserChecker {
		bool UserExists(string userName);
		UserProfile GetProfile(string userName);
	}

	interface IUserDBHandler : IUserChecker, IUserPasswordCreator, IUserProfileCreator { }
}
