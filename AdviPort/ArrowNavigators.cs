using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AdviPort {

	interface IMainPageNavigator {
		string NavigateOrReadInput();
	}

	class DefaultMainPageNavigator : IMainPageNavigator {

	
		public string NavigateOrReadInput() {

			int cursor = Console.CursorTop;
			Console.Write("Please enter your choice: ");
			int arrowNav = 0;
			StringBuilder sb = new StringBuilder();

			while (true) {
				var key = Console.ReadKey();

				if (key.Key == ConsoleKey.Enter) { break; }
				if (key.Key == ConsoleKey.DownArrow) {
					ClearAndWritePosition(++arrowNav, cursor);
				}
				if (key.Key == ConsoleKey.UpArrow) {
					ClearAndWritePosition(--arrowNav, cursor);
				}

				sb.Append(key.KeyChar);
			}

			if (arrowNav > 0) {
				Console.WriteLine(arrowNav);
				return arrowNav.ToString();
			}
	
			Console.WriteLine(sb.ToString());
			return sb.ToString();
		}

		private void ClearAndWritePosition(int position, int initCursorPosition) {
			// TODO: Change this
			Plugins.ConsolePasswordReader.Instance.ConsoleClearLine(initCursorPosition);
			Console.Write($"Currently chosen plugin: {position}");
		}
	}
}
