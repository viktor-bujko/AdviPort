using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AdviPort {

	interface IMainPageNavigator {
		string NavigateOrReadInput(int maxPlugins);
	}

	class DefaultMainPageNavigator : IMainPageNavigator {

		private int cursorTop;
		private int maxPlugins;
		private string message = "Please enter your choice: ";

		public string NavigateOrReadInput(int maxPlugins) {

			bool textReadState = true;
			cursorTop = Console.CursorTop;
			this.maxPlugins = maxPlugins;
			string resultInput;

			do {
				Console.Write(message);

				if (textReadState) {
					resultInput = ReadInput();
				} else {
					resultInput = Navigate(0);
				}
			} while (resultInput == null);

			return resultInput;
		}

		private string ReadInput() {
			var key = Console.ReadKey();

			if (key.Key == ConsoleKey.UpArrow || key.Key == ConsoleKey.DownArrow) { return Navigate(1); }
			if (key.Key == ConsoleKey.Spacebar || key.Key == ConsoleKey.Backspace) {
				Console.CursorLeft = message.Length;
				return ReadInput();
			}

			string stringInput = Console.ReadLine();

			return key.KeyChar + stringInput;
		}

		private string Navigate(int initValue) {
			ConsoleKeyInfo key;
			int position = initValue;

			do {
				Console.Write(position);
				key = Console.ReadKey();
				Console.CursorLeft = message.Length; 
				
				if (key.Key == ConsoleKey.UpArrow) { --position; }
				if (key.Key == ConsoleKey.DownArrow && position + 1 <= maxPlugins) { ++position; }

				if (position == 0) {
					Console.CursorLeft = message.Length;
					Console.Write(new string(' ', Console.WindowWidth));
					Console.CursorLeft = message.Length;
					Console.CursorTop = cursorTop;
					return ReadInput();
				}

			} while (key.Key != ConsoleKey.Enter);

			return position.ToString();
		}
	}
}
