using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace AdviPort.Plugins {
	interface ILoginHandler {
		public UserProfile LogIn();
	}

	interface ILogoutHandler {
		void LogOut();
	}

	class LoginPlugin : ILoginHandler, ILoggedOffOnlyPlugin {
		public string Name => "Login to the application";

		internal static LoginPlugin Instance { get; private set; }

		public string Description => "Logs in as user";

		private IUserInterfaceReader InputReader { get; }

		private IUserChecker UserChecker { get; }

		private LoginPlugin(IUserInterfaceReader inputReader, IUserChecker userChecker) {
			InputReader = inputReader;
			UserChecker = userChecker;
		}

		public static LoginPlugin GetInstance(IUserInterfaceReader inputReader, IUserChecker userChecker) {
			if (Instance == null ||
				Instance.InputReader != inputReader ||
				Instance.UserChecker != userChecker
				) {
				// Creating new instance only if it does not exist yet or if the conditions change
				Instance = new LoginPlugin(inputReader, userChecker);
			}

			return Instance;
		}

		UserProfile ILoginHandler.LogIn() {

			var userLogin = InputReader.ReadUserInput("Enter your username");
			if (!UserChecker.UserExists(userLogin)) {
				Console.Error.WriteLine($"User with login \"{userLogin}\" does not exists. Please register first.");
				return null;
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

				if (!string.IsNullOrWhiteSpace(userPasswd)) {
					userPasswd = Encryptor.Encrypt(userPasswd);
					loginSuccess = user.Password == userPasswd;

					tryAnotherAttempt = --passwordAttempts > 0 && !loginSuccess;
				}

				if (!loginSuccess) {
					if (passwordAttempts <= 3) {
						passwordAttemptsWarning = $" ({passwordAttempts} attempts left)";
					}

					Console.WriteLine("Incorrect password");
					Thread.Sleep(350);
					ConsolePasswordReader.Instance.ConsoleClearLine(cursorRow);
				}

			} while (tryAnotherAttempt);

			// Loop breaks after a successful login - if will not succeed
			// or when max attempts are reached without success.
			if (!loginSuccess) {
				Console.Error.WriteLine("Too many incorrect attempts. Please try again.");
				return null;
			}

			Console.WriteLine("Login successful.");
			Session.ActiveSession.LoggedUser = user;
			return user;
		}

		public int Invoke(object[] args) {

			var loggedUser = ((ILoginHandler)this).LogIn();

			if (loggedUser == null) { return 1; }

			return 0;
		}
	}

	class LogoutPlugin : LoggedInOnlyPlugin, ILogoutHandler {

		internal static LogoutPlugin Instance { get; } = new LogoutPlugin();

		public override string Name => "Log out";

		public override string Description => "Logs out the current user.";

		private LogoutPlugin() { }

		public override int Invoke(object[] args) {

			((ILogoutHandler)this).LogOut();
			return 0;
		}

		void ILogoutHandler.LogOut() {
			if (Session.ActiveSession.HasLoggedUser) {
				Session.ActiveSession.LoggedUser = null;
				Console.WriteLine("Logged out successfully.");
			} else {
				Console.WriteLine("Already logged out.");
			}
		}
	}
}
