using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AdviPort.Plugins {
	interface IExecutablePlugin {
		int Invoke();
	}

	interface IPlugin : IExecutablePlugin {
		string Name { get; }
		string Description { get; }
	}

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

	interface ILoggedOffOnlyPlugin : IPlugin { }

	class PluginInputReader : IUserInterfaceReader {

		public PluginInputReader() { }

		public virtual string ReadUserInput(string initialPrompt) {
			if (!(initialPrompt is null)) {
				Console.Write(initialPrompt + ": ");
			}

			var input = Console.ReadLine();

			if (input is null) throw new ArgumentNullException("The input should not be null.");

			return input.Trim();
		}
	}

	class ConsolePasswordReader : IUserInterfaceReader {

		public static ConsolePasswordReader Instance { get; } = new ConsolePasswordReader();

		private ConsolePasswordReader() { }

		public string ReadUserInput(string initialPrompt) {
			if (! (initialPrompt == null)) {
				Console.Write(initialPrompt + ": ");
			}

			StringBuilder sb = new StringBuilder();
			ConsoleKeyInfo key;

			do {
				key = Console.ReadKey(true);
				switch (key.Key) {
					case ConsoleKey.Escape:
					case ConsoleKey.Backspace:
					case ConsoleKey.Home:
					case ConsoleKey.End:
						continue;
					case ConsoleKey.Enter:
						Console.SetCursorPosition(0, Console.CursorTop + 1);
						break;
					default:
						sb.Append(key.KeyChar);
						break;
				}

			} while (key.Key != ConsoleKey.Enter);

			return sb.ToString();
		}

		public void ConsoleClearLine(int initPosition, int rowsToClear = 2) {
			Console.SetCursorPosition(0, initPosition);
			for (int i = 0; i < rowsToClear; i++) {
				Console.WriteLine(new string(' ', Console.WindowWidth));
			}
			Console.SetCursorPosition(0, initPosition);
		}
	}
}
