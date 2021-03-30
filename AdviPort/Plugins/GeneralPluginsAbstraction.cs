using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AdviPort.Plugins {
	interface IExecutablePlugin {
		int Invoke(object[] args);
	}

	interface IPlugin : IExecutablePlugin {
		string Name { get; }
		string Description { get; }
	}

	interface ILoggedInOnlyPlugin : IPlugin { }

	interface ILoggedOffPlugin : IPlugin { }

	class PluginInputReader : IUserInterfaceReader {
		protected virtual TextReader Reader { get; }
		protected virtual TextWriter Writer { get; }

		public PluginInputReader(TextReader reader, TextWriter writer) {
			Reader = reader;
			Writer = writer;
		}

		public virtual string ReadUserInput(string initialPrompt) {
			if (!(initialPrompt is null)) {
				Writer.Write(initialPrompt + ": ");
			}

			var input = Reader.ReadLine();

			if (input is null) throw new ArgumentNullException("The input should not be null.");

			return input.Trim();
		}
	}

	class ConsolePasswordReader : IUserInterfaceReader {

		public static ConsolePasswordReader Instance { get; } = new ConsolePasswordReader();
		protected virtual TextReader Reader { get; }
		protected virtual TextWriter Writer { get; }

		private ConsolePasswordReader() {
			Reader = Console.In;
			Writer = Console.Out;
		}

		public string ReadUserInput(string initialPrompt) {
			if (! (initialPrompt == null)) {
				Writer.Write(initialPrompt + ": ");
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
	}
}
