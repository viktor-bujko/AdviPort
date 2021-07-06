using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using AdviPort.UI;

namespace AdviPort.Plugins {

	/// <summary>
	/// Provides login control method.
	/// </summary>
	interface ILoginHandler {
		public UserProfile LogIn();
	}

	/// <summary>
	/// Provides logout control method.
	/// </summary>
	interface ILogoutHandler {
		void LogOut();
	}

	/// <summary>
	/// Plugin which controls the login process.
	/// </summary>
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

		/// <summary>
		/// <inheritdoc cref="LoginPlugin"/>
		/// </summary>
		/// <returns>The instance of user profile of user who logged-in.</returns>
		UserProfile ILoginHandler.LogIn() {

			var userLogin = InputReader.ReadUserInput("Enter your username");
			if (!UserChecker.UserExists(userLogin)) {
				Console.Error.WriteLine($"User with login \"{userLogin}\" does not exists. Please register first.");
				return null;
			}

			var user = UserChecker.GetProfile(userLogin);
			int passwordAttempts = 5;	// initial value of maximum attempts
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

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		/// <returns><inheritdoc/></returns>
		public int Invoke() {

			var loggedUser = ((ILoginHandler)this).LogIn();

			if (loggedUser == null) { return 1; }

			return 0;
		}
	}

	/// <summary>
	/// Plugin which controls logout process.
	/// </summary>
	class LogoutPlugin : LoggedInOnlyPlugin, ILogoutHandler {

		internal static LogoutPlugin Instance { get; } = new LogoutPlugin();

		public override string Name => "Log out";

		public override string Description => "Logs out the current user.";

		private LogoutPlugin() { }

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		/// <returns><inheritdoc/></returns>
		public override int Invoke() {

			((ILogoutHandler)this).LogOut();
			return 0;
		}

		/// <summary>
		/// Logs out currently logged user.
		/// </summary>
		void ILogoutHandler.LogOut() {
			if (Session.ActiveSession.HasLoggedUser) {
				var username = Session.ActiveSession.LoggedUser.UserName;
				Session.ActiveSession.LoggedUser = null;
				Console.WriteLine($"{username}, you have been logged out successfully.");
			} else {
				Console.WriteLine("Already logged out.");
			}
		}
	}
}
