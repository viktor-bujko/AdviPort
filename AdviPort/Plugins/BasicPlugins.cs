using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace AdviPort.Plugins {
	class AboutAppPlugin : IPlugin {
		public string Name => "About Application";

		public string Description => "Prints information about application.";

		public int Invoke(object[] args) {
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

	class ExitAppPlugin : IPlugin {
		public string Name => "Exit Application";

		public string Description => "Quits the application";

		public int Invoke(object[] args) {
			Console.WriteLine("Exiting ADVIPORT application.");
			return 0;
		}
	}

	class LoginPlugin : IPlugin {
		public string Name => "Login to the application";

		public string Description => "Logs in as user";

		private PluginInputReader InputReader { get; }

		private IUserChecker UserChecker { get; }

		public LoginPlugin(PluginInputReader inputReader, IUserChecker userChecker) {
			InputReader = inputReader;
			UserChecker = userChecker;
		}

		public int Invoke(object[] args) {
			var userLogin = InputReader.ReadUserInput("Enter your username");
			if (! UserChecker.UserExists(userLogin)) {
				Console.Error.WriteLine($"User with login \"{userLogin}\" does not exists. Please register first.");
				return 1;
			}

			var user = UserChecker.GetProfile(userLogin);
			int passwordAttempts = 5;
			bool loginSuccess;
			bool tryAnotherAttempt;
			string passwordAttemptsWarning = "";

			do {
				int cursorRow = Console.CursorTop;
				var userPasswd = ConsolePasswordReader.Instance.ReadUserInput($"Enter your password{passwordAttemptsWarning}");
				loginSuccess = false;
				tryAnotherAttempt = true;

				if (!  string.IsNullOrWhiteSpace(userPasswd)) {
					userPasswd = Encryptor.Encrypt(userPasswd);
					loginSuccess = user.Password == userPasswd;

					tryAnotherAttempt = --passwordAttempts > 0 && !loginSuccess;
				}

				if (!loginSuccess) {
					if (passwordAttempts <= 3) {
						passwordAttemptsWarning = $" ({passwordAttempts} attempts left)";
					}

					WaitAndClearLine(cursorRow);
				}

			} while (tryAnotherAttempt);

			// Loop breaks after a successful login - if will not succeed
			// or when max attempts are reached without success.
			if (!loginSuccess) {
				Console.Error.WriteLine("Too many incorrect attempts. Please try again.");
				return 1;
			}

			Console.WriteLine("Login successful.");
			Session.ActiveSession.LoggedUser = user;
			return 0;
		}

		private void WaitAndClearLine(int initPosition) {
			Console.WriteLine("Incorrect password");
			Thread.Sleep(350);

			Console.SetCursorPosition(0, initPosition);
			Console.WriteLine(new string(' ', Console.WindowWidth));
			Console.WriteLine(new string(' ', Console.WindowWidth));
			Console.SetCursorPosition(0, initPosition);
		}
	}

	interface ILogoutHandler {
		void LogOut();
	}

	class LogoutPlugin : ILogoutHandler, IPlugin {
		public string Name => "Log out";

		public string Description => "Logs out the current user.";

		public int Invoke(object[] args) {

			((ILogoutHandler)this).LogOut();
			return 0;
		}

		void ILogoutHandler.LogOut() {
			var loggedUser = Session.ActiveSession.LoggedUser;

			if (loggedUser != null) {
				Session.ActiveSession.LoggedUser = null;
				Console.WriteLine("Logged out successfully.");
			} else {
				Console.WriteLine("Already logged out.");
			}

			return 0;
		}
	}
}
