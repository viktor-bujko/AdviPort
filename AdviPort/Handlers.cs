using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AdviPort {

	interface IMainPagePrinter {
		/* Interface, ktorý musí vedieť vypísať obsah hlavného menu. 
		 * Narozdiel od mainComponent printer može iba vypísať obsah appky bez čarov marov.
		 */

		void PrintMainPageContent(GeneralApplicationSettings settings);

		int PrintMainPagePluginOption(IPlugin plugin, int orderNumber);
	}

	interface IUserInterfaceReader {
		/*
		 * Interface, ktorý musí vedieť :
		 *		- prečítať obsah z readera, najlepšie s nejakým optional promptom
		 */

		string ReadUserInput(string initialPrompt = "Please enter your choice");
	}

	interface IMainPageHandler : IMainPagePrinter, IUserInterfaceReader {
		/* 
		 * Main component je hlavná komponenta menu ktorá obsahuje *názov aplikácie*.
		 * 
		 * - Táto musí vedieť vypísať nejaký úvod ale aj obsah. (vie z MainPagePrintera)
		 * - Taktiež musí vedieť aj spracovať to, čo používateľ zadá (vie z UIReadera)
		 * - zhandlovať že čo asi ten vstup chce povedať
		 */

		string MainPageHeader { get; }  // Táto metóda sa do handlera hodí lebo ho dopĺňa 

		IPlugin HandlePluginChoice(string input);
	}

	/// <summary>
	/// Main Page Handler Factory class.
	/// </summary>
	abstract class MainPageHandlerSelector {
		public static IMainPageHandler SelectMainPageHandler(GeneralApplicationSettings settings, TextReader reader, TextWriter writer) {

			IMainPageHandler handler = settings.MainPageStyle switch {
				"" => ClassicMainPageHandler.NewInstance(reader, writer),
				"classic" => ClassicMainPageHandler.NewInstance(reader, writer),
				"decorative" => DecorativeMainPageHandler.NewInstance(reader, writer),
				"descriptive" => DescriptiveMainPageHandler.NewInstance(reader, writer, false),
				"decorative/descriptive" => DescriptiveMainPageHandler.NewInstance(reader, writer, true),
				"descriptive/decorative" => DescriptiveMainPageHandler.NewInstance(reader, writer, true),
				_ => null,
			};

			if (handler is null) {
				Console.Error.WriteLine("Unsupported main page printer.");
				handler = ClassicMainPageHandler.NewInstance(reader, writer);
			}

			return handler;
		}
	}

	abstract class CommonMainPageHandler : IMainPageHandler {

		public abstract string MainPageHeader { get; }

		public abstract TextReader Reader { get; }

		public abstract TextWriter Writer { get; }

		protected abstract IPlugin[] Plugins { get; set; }

		public abstract void PrintMainPageContent(GeneralApplicationSettings settings);

		public abstract int PrintMainPagePluginOption(IPlugin plugin, int orderNumber);

		public abstract void PrintContentFooterSeparator(int maxPrinted);

		public virtual void PrintAvailablePlugins() {
			int maxPrinted = int.MinValue;

			for (int i = 0; i < Plugins.Length; i++) {
				var printedLength = PrintMainPagePluginOption(Plugins[i], i + 1);

				maxPrinted = printedLength > maxPrinted ? printedLength : maxPrinted;
			}

			PrintContentFooterSeparator(maxPrinted);
		}

		public virtual string ReadUserInput(string initialPrompt) {
			if (!(initialPrompt is null)) {
				Writer.Write(initialPrompt + ": ");
			}

			var input = Reader.ReadLine();

			if (input is null) throw new ArgumentNullException("The input should not be null.");

			return input.Trim();
		}

		public IPlugin HandlePluginChoice(string input) {

			if (Plugins is null || Plugins.Length == 0) { throw new ArgumentException("No plugins are available."); }

			if (input is null) { throw new ArgumentException("Incorrect input string."); }

			if (int.TryParse(input, out int pluginOrderNumber)) {
				--pluginOrderNumber;  // Conversion from order number to Plugins array index.
				
				if (pluginOrderNumber >= 0 && pluginOrderNumber < Plugins.Length) {
					// Correct Plugins array index
					return Plugins[pluginOrderNumber];
				}

				// An incorrect number was entered.
				Console.Error.WriteLine($"Please make sure only numbers in correct range (1 - {Plugins.Length - 1}) are entered.");
				return null;
			}
			
			// Not a number has been entered as input.
			if (PluginSelector.TryGetPluginFromInputString(input, Plugins, out List<IPlugin> filteredPlugins)) { 
				// Exactly one corresponding plugin has been found.
				return filteredPlugins[0]; 
			}

			if (filteredPlugins.Count == 0) {
				Console.Error.Write("No corresponding plugin has been found. Please try again.");
			} else {
				Console.Error.Write($"{filteredPlugins.Count} matching plugins has been found. Please specify an exact number of the plugin.");
				Plugins = filteredPlugins.ToArray();
			}

			return null;
		}
	}

	class ClassicMainPageHandler : CommonMainPageHandler {

		// Only a single instance of this type is expected to be needed.
		private static ClassicMainPageHandler Instance { get; set; }

		public override string MainPageHeader => @"
 █████╗ ██████╗ ██╗   ██╗██╗██████╗  ██████╗ ██████╗ ████████╗
██╔══██╗██╔══██╗██║   ██║██║██╔══██╗██╔═══██╗██╔══██╗╚══██╔══╝
███████║██║  ██║██║   ██║██║██████╔╝██║   ██║██████╔╝   ██║   
██╔══██║██║  ██║╚██╗ ██╔╝██║██╔═══╝ ██║   ██║██╔══██╗   ██║   
██║  ██║██████╔╝ ╚████╔╝ ██║██║     ╚██████╔╝██║  ██║   ██║   
╚═╝  ╚═╝╚═════╝   ╚═══╝  ╚═╝╚═╝      ╚═════╝ ╚═╝  ╚═╝   ╚═╝    
______________________________________________________________
";

		protected override IPlugin[] Plugins { get; set; }

		public override TextReader Reader { get; }

		public override TextWriter Writer { get; }

		private ClassicMainPageHandler(TextReader reader, TextWriter writer) {
			Reader = reader;
			Writer = writer;
		}

		public static ClassicMainPageHandler NewInstance(TextReader reader, TextWriter writer) {
			if (Instance is null) {
				Instance = new ClassicMainPageHandler(reader, writer);
			}

			return Instance;
		}

		public override void PrintMainPageContent(GeneralApplicationSettings settings) {
			Writer.WriteLine(MainPageHeader);

			Plugins = PluginSelector.GetAvailablePlugins(settings, Reader, Writer);
			PrintAvailablePlugins();
		}
		
		public override int PrintMainPagePluginOption(IPlugin plugin, int orderNumber) {
			var pluginToPrint = $"{ orderNumber }) { plugin.Name }";
			Writer.WriteLine(pluginToPrint);

			return pluginToPrint.Length;
		}

		public override void PrintContentFooterSeparator(int maxPrinted) {
			Writer.WriteLine(new string('=', maxPrinted));
			Writer.WriteLine();
		}

	}

	class DecorativeMainPageHandler : CommonMainPageHandler {

		private static DecorativeMainPageHandler Instance { get; set; }

		private readonly int freeSpaces;
		private readonly PageDecoration sideDecoration, planeDecoration;
		//private readonly List<PageDecoration> decorations;

		public override string MainPageHeader => @"
     ___           ___                                                    ___           ___                   
    /  /\         /  /\          ___            ___         ___          /  /\         /  /\          ___     
   /  /::\       /  /::\        /  /\          /__/\       /  /\        /  /::\       /  /::\        /__/\    
  /  /:/\:\     /  /:/\:\      /  /:/          \__\:\     /  /::\      /  /:/\:\     /  /:/\:\       \  \:\   
 /  /::\ \:\   /  /:/  \:\    /  /:/           /  /::\   /  /:/\:\    /  /:/  \:\   /  /::\ \:\       \__\:\  
/__/:/\:\_\:\ /__/:/ \__\:|  /__/:/  ___    __/  /:/\/  /  /::\ \:\  /__/:/ \__\:\ /__/:/\:\_\:\      /  /::\ 
\__\/  \:\/:/ \  \:\ /  /:/  |  |:| /  /\  /__/\/:/    /  /:/\:\_\:\ \  \:\ /  /:/ \__\/ |::\/:/     /  /:/\:\
     \__\::/   \  \:\  /:/   |  |:|/  /:/  \  \::/    /  /:/  \:\/:/  \  \:\  /:/     |  |:|::/     /  /:/__\/
     /  /:/     \  \:\/:/    |__|:|__/:/    \  \:\    \__\/ \  \::/    \  \:\/:/      |  |:|\/     /  /:/     
    /__/:/       \__\::/      \__\::::/      \__\/           \__\/      \  \::/       |__|:|      /  /:/      
    \__\/                                                                \__\/         \__\|      \__\/       
______________________________________________________________________________________________________________
";

		protected override IPlugin[] Plugins { get; set; }

		public override TextReader Reader { get; }
		public override TextWriter Writer { get; }

		private DecorativeMainPageHandler(TextReader reader, TextWriter writer) {
			Reader = reader;
			Writer = writer;
			freeSpaces = 70;
			//decorations = PageDecoration.GetPageDecorations(@"..\..\..\decorations");
			sideDecoration = new PageDecoration(@"..\..\..\decorations\tower_decoration.txt");
			planeDecoration = new PageDecoration(@"..\..\..\decorations\airplane_decoration.txt");
		}

		public static DecorativeMainPageHandler NewInstance(TextReader reader, TextWriter writer) {
			if (Instance is null) {
				Instance = new DecorativeMainPageHandler(reader, writer);
			}

			return Instance;
		}

		public override void PrintMainPageContent(GeneralApplicationSettings settings) {
			Console.BackgroundColor = ConsoleColor.Blue;
			Writer.WriteLine(MainPageHeader);
			Console.BackgroundColor = ConsoleColor.Black;
			planeDecoration.Print(Writer, 2, title: "«« MAIN MENU »»");
			Plugins = PluginSelector.GetAvailablePlugins(settings, Reader, Writer);
			PrintAvailablePlugins();
		}

		public override void PrintAvailablePlugins() {
			//var consoleBackupColor = Console.BackgroundColor;
			var maxPrinted = int.MinValue;

			for (int i = 0; i < Plugins.Length; i++) {
				/*Console.BackgroundColor = (ConsoleColor)new Random().Next(1, 9);
				/if (Console.BackgroundColor == (ConsoleColor)7) { Console.BackgroundColor++; }*/
				 var printedLength = PrintMainPagePluginOption(Plugins[i], i + 1);

				maxPrinted = printedLength > maxPrinted ? printedLength : maxPrinted;
			}

			// Print the rest of the side decoration
			if (sideDecoration.Exists) {
				while (++sideDecoration.LastUsedIndex < sideDecoration.RowsSize) {
					Writer.WriteLine(new string(' ', freeSpaces) + sideDecoration[sideDecoration.LastUsedIndex]);
				}
			}
			//if (writer == Console.Out) { Console.BackgroundColor = consoleBackupColor; }

			PrintContentFooterSeparator(maxPrinted);
		}

		public override int PrintMainPagePluginOption(IPlugin plugin, int orderNumber) {
			string pluginToPrint = $"{ orderNumber } »» { plugin.Name } ««";
			string decorationImg = "";

			if (orderNumber < sideDecoration.RowsSize) {
				decorationImg = new string(' ', freeSpaces - pluginToPrint.Length) + sideDecoration[orderNumber - 1];
			}

			pluginToPrint += decorationImg;
			Writer.WriteLine(pluginToPrint);

			return pluginToPrint.Length;
		}

		public override void PrintContentFooterSeparator(int maxPrinted) {
			Writer.WriteLine(new string('~', maxPrinted));
			Writer.WriteLine();
		}

		public override string ReadUserInput(string initialPrompt) {
			return base.ReadUserInput("»» " + initialPrompt);
		}
	}

	class DescriptiveMainPageHandler : CommonMainPageHandler {

		private static DescriptiveMainPageHandler Instance { get; set; }

		private CommonMainPageHandler BaseMainPageHandler { get; }

		protected override IPlugin[] Plugins { get; set; }

		public override TextReader Reader { get; }

		public override TextWriter Writer { get; }

		public override string MainPageHeader => BaseMainPageHandler.MainPageHeader;

		private DescriptiveMainPageHandler(CommonMainPageHandler baseHandler) {
			BaseMainPageHandler = baseHandler;
			Reader = BaseMainPageHandler.Reader;
			Writer = BaseMainPageHandler.Writer;
		}

		public static DescriptiveMainPageHandler NewInstance(TextReader reader, TextWriter writer, bool isDecorativeHandler) {
			if (Instance is null) {
				if (isDecorativeHandler) {
					Instance = new DescriptiveMainPageHandler(DecorativeMainPageHandler.NewInstance(reader, writer));
				} else {
					Instance = new DescriptiveMainPageHandler(ClassicMainPageHandler.NewInstance(reader, writer));
				}
			}

			return Instance;
		}

		public override void PrintMainPageContent(GeneralApplicationSettings settings) {
			Writer.WriteLine(MainPageHeader);
			Plugins = PluginSelector.GetAvailablePlugins(settings, Reader, Writer);

			var maxPrinted = int.MinValue;

			for (int i = 0; i < Plugins.Length; i++) {
				var printedLength = PrintMainPagePluginOption(Plugins[i], i + 1);

				maxPrinted = printedLength > maxPrinted ? printedLength : maxPrinted;
			}
			PrintContentFooterSeparator(maxPrinted);
		}

		public override int PrintMainPagePluginOption(IPlugin plugin, int orderNumber) {
			int printedLenght = BaseMainPageHandler.PrintMainPagePluginOption(plugin, orderNumber);
			string description = $"   \u2192 { plugin.Description }\n";
			Writer.WriteLine(description);

			return Math.Max(printedLenght, description.Length);
		}

		public override void PrintContentFooterSeparator(int maxPrinted) {
			BaseMainPageHandler.PrintContentFooterSeparator(maxPrinted);
		}
	}

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

		public bool Exists => ! (Decoration is null);
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

			while (! ((line = reader.ReadLine()) is null)) {
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

		public void Print(TextWriter writer, int objectCount = 1, int spaces = 15, string title = "") {
			
			if (spaces < 0 || objectCount < 0) { 
				throw new ArgumentException($"Both { nameof(objectCount) } and { nameof(spaces) } parameters can't be negative."); 
			}

			int titlePadding = 0;
			if (! string.IsNullOrEmpty(title)) {
				titlePadding = title.Length;
				spaces = 6; 
			}

			for (int row = 0; row < RowsSize; row++) {
				int lineCount = objectCount;
				while (lineCount > 0) {
					writer.Write(Decoration[row]);
					writer.Write(new string(' ', MaxWidth - Decoration[row].Length));
					// Condition will place a non-empty title in the center between TWO decoration 
					if (objectCount == 2 && row == RowsSize / 2 && !string.IsNullOrEmpty(title)) {
						writer.Write(new string(' ', spaces / 2));
						writer.Write(title);
						titlePadding = 0;
						writer.Write(new string(' ', spaces / 2));
					}
					if (--lineCount > 0) {
						if (!string.IsNullOrEmpty(title) && titlePadding == 0) {
							titlePadding = title.Length;
							title = "";
							continue;
						}
						writer.Write(new string(' ', spaces + titlePadding));
					}
				}
				writer.WriteLine();
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
