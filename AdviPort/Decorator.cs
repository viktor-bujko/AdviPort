using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AdviPort {
	class PageDecoration {

		private string[] Decoration { get; }
		public int RowsSize { get; }
		public int MaxWidth { get; }

		public int LastUsedIndex {
			get => lastIndex;
			set {
				if (value <= RowsSize) {
					lastIndex = value;
				} else throw new IndexOutOfRangeException();
			}
		}

		public bool Exists => !(Decoration is null);
		private int lastIndex;

		// Indexer just to make the call to the Decoration array more visible.
		public string this[int i] {
			get {
				LastUsedIndex = i;
				return Decoration[i];
			}
		}

		public PageDecoration(TextReader reader) {
			string line;
			int maxWidth = int.MinValue;
			List<string> decor = new List<string>();

			while (!((line = reader.ReadLine()) is null)) {
				if (line.StartsWith('#') || string.IsNullOrEmpty(line)) continue;

				decor.Add(line);
				if (line.Length > maxWidth) maxWidth = line.Length;
			}

			MaxWidth = maxWidth;
			Decoration = decor.ToArray();
			RowsSize = Decoration.Length;
		}

		public PageDecoration(string fileName) {
			try {
				TextReader reader = new StreamReader(fileName);
				var obj = new PageDecoration(reader);
				RowsSize = obj.RowsSize;
				Decoration = obj.Decoration;
				RowsSize = obj.RowsSize;
				MaxWidth = obj.MaxWidth;
			} catch { /*throw new ArgumentException("The file with given fileName does not exist.");*/ }
		}

		public void Print(int objectCount = 1, int spaces = 15, string title = "") {

			if (spaces < 0 || objectCount < 0) {
				throw new ArgumentException($"Both { nameof(objectCount) } and { nameof(spaces) } parameters can't be negative.");
			}

			int titlePadding = 0;
			if (!string.IsNullOrEmpty(title)) {
				titlePadding = title.Length;
				spaces = 6;
			}

			for (int row = 0; row < RowsSize; row++) {
				int lineCount = objectCount;
				while (lineCount > 0) {
					Console.Write(Decoration[row]);
					Console.Write(new string(' ', MaxWidth - Decoration[row].Length));
					// Condition will place a non-empty title in the center between TWO decoration 
					if (objectCount == 2 && row == RowsSize / 2 && !string.IsNullOrEmpty(title)) {
						Console.Write(new string(' ', spaces / 2));
						Console.Write(title);
						titlePadding = 0;
						Console.Write(new string(' ', spaces / 2));
					}
					if (--lineCount > 0) {
						if (!string.IsNullOrEmpty(title) && titlePadding == 0) {
							titlePadding = title.Length;
							title = "";
							continue;
						}
						Console.Write(new string(' ', spaces + titlePadding));
					}
				}
				Console.WriteLine();
			}
		}

		public static List<PageDecoration> GetPageDecorations(string path) {

			List<PageDecoration> result = new List<PageDecoration>();

			var filePaths = GeneralApplicationSettings.SearchFiles(path, "*.txt");

			if (filePaths is null) return result;

			for (int i = 0; i < filePaths.Length; i++) {
				var reader = GeneralApplicationSettings.GetTextReader(filePaths, i);
				if (reader is null) continue;

				result.Add(new PageDecoration(reader));
			}

			return result;
		}

		public override string ToString() {
			var sb = new StringBuilder(RowsSize);
			for (int i = 0; i < RowsSize; i++) {
				sb.Append(Decoration[i]);
				if (i < RowsSize - 1) sb.Append(Environment.NewLine);
			}
			return sb.ToString();
		}
	}
}
