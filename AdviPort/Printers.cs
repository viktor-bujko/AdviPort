using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AdviPort {

	interface IMainContentPrinter {
		/* Interface, ktorý musí vedieť vypísať obsah hlavného menu. 
		 * Narozdiel od mainComponent printer može iba vypísať obsah appky bez čarov marov.
		 */
		void PrintMainPageContent(TextWriter writer, GeneralApplicationSettings settings);
	}

	interface IMainPagePrinter : IMainContentPrinter {
		/* Main component je komponenta menu ktorá obsahuje hlavný názov aplikácie.
		 * Táto musí vedieť vypísať nejaký úvod ale aj obsah. 
		 */
		string GetMainPageHeader();

		IPlugin[] GetAvailablePlugins(GeneralApplicationSettings settings);
	}

	abstract class MainPagePrinterSelector {

		public static IMainContentPrinter SelectMainPagePrinter(GeneralApplicationSettings settings) {

			IMainContentPrinter printer = settings.MainPageStyle switch {
				"" => SimpleMainPagePrinter.Instance,
				"simple" => SimpleMainPagePrinter.Instance,
				"decorative" => DecorativeMainPagePrinter.Instance,
				"descriptive" => DescriptiveMainPagePrinter.Instance,
				/*
				"decorative/descriptive" => new DecorativeMainPagePrinter(new DescriptiveMainPagePrinter()),
				"descriptive/decorative" => new DecorativeMainPagePrinter(new DescriptiveMainPagePrinter()),
				*/
				_ => throw new ArgumentException("Unsupported main page printer."),
			};

			return printer;
		}
	}

	abstract class CommonMainComponentPrinter : IMainPagePrinter {

		public abstract string GetMainPageHeader();
		public abstract void PrintPlugin(TextWriter writer, IPlugin plugin, int orderNumber);
		public abstract void PrintMainPageContent(TextWriter writer, GeneralApplicationSettings settings);

		public virtual IPlugin[] GetAvailablePlugins(GeneralApplicationSettings settings) {

			List<IPlugin> plugins = new List<IPlugin>(settings.AvailablePlugins.Length);

			foreach (string pluginName in settings.AvailablePlugins) {
				var plugin = PluginSelector.SearchPluginByName(pluginName);
				if (plugin is null) continue;

				plugins.Add(plugin);
			}

			return plugins.ToArray();
		}

		public virtual void PrintAvailablePlugins(IPlugin[] plugins, TextWriter writer) {
			for (int i = 0; i < plugins.Length; i++) {
				PrintPlugin(writer, plugins[i], i + 1);
			}
		}
	}

	class SimpleMainPagePrinter : CommonMainComponentPrinter {

		private static readonly SimpleMainPagePrinter instance = new SimpleMainPagePrinter();
		// Only a single instance of this type is expected to be needed.
		public static SimpleMainPagePrinter Instance => instance;

		private SimpleMainPagePrinter() { }

		public override void PrintMainPageContent(TextWriter writer, GeneralApplicationSettings settings) {
			writer.WriteLine(GetMainPageHeader());

			var plugins = GetAvailablePlugins(settings);
			PrintAvailablePlugins(plugins, writer);
		}

		public override string GetMainPageHeader() {
			return @"
 █████╗ ██████╗ ██╗   ██╗██╗██████╗  ██████╗ ██████╗ ████████╗
██╔══██╗██╔══██╗██║   ██║██║██╔══██╗██╔═══██╗██╔══██╗╚══██╔══╝
███████║██║  ██║██║   ██║██║██████╔╝██║   ██║██████╔╝   ██║   
██╔══██║██║  ██║╚██╗ ██╔╝██║██╔═══╝ ██║   ██║██╔══██╗   ██║   
██║  ██║██████╔╝ ╚████╔╝ ██║██║     ╚██████╔╝██║  ██║   ██║   
╚═╝  ╚═╝╚═════╝   ╚═══╝  ╚═╝╚═╝      ╚═════╝ ╚═╝  ╚═╝   ╚═╝    
______________________________________________________________
";
		}

		public override void PrintPlugin(TextWriter writer, IPlugin plugin, int orderNumber) {
			writer.WriteLine($"{ orderNumber }) { plugin.Name }");
		}
	}

	class DecorativeMainPagePrinter : CommonMainComponentPrinter {

		private static readonly DecorativeMainPagePrinter instance = new DecorativeMainPagePrinter();
		public static DecorativeMainPagePrinter Instance => instance;

		private readonly int freeSpaces;
		private readonly PageDecoration sideDecoration, planeDecoration;
		//private readonly List<PageDecoration> decorations;

		private DecorativeMainPagePrinter() {
			freeSpaces = 70;
			//decorations = PageDecoration.GetPageDecorations(@"..\..\..\decorations");
			sideDecoration = new PageDecoration(@"..\..\..\decorations\tower_decoration.txt");
			planeDecoration = new PageDecoration(@"..\..\..\decorations\airplane_decoration.txt");
		}

		public override void PrintMainPageContent(TextWriter writer, GeneralApplicationSettings settings) {
			Console.BackgroundColor = ConsoleColor.Blue;
			writer.WriteLine(GetMainPageHeader());
			Console.BackgroundColor = ConsoleColor.Black;
			planeDecoration.Print(writer, 2, title: "«« MAIN MENU »»");
			var plugins = GetAvailablePlugins(settings);
			PrintAvailablePlugins(plugins, writer);
			planeDecoration.Print(writer);
		}

		public override void PrintAvailablePlugins(IPlugin[] plugins, TextWriter writer) {
			//var consoleBackupColor = Console.BackgroundColor;

			for (int i = 0; i < plugins.Length; i++) {
				/*Console.BackgroundColor = (ConsoleColor)new Random().Next(1, 9);
				/if (Console.BackgroundColor == (ConsoleColor)7) { Console.BackgroundColor++; }*/
				PrintPlugin(writer, plugins[i], i + 1);
			}

			// Print the rest of the side decoration
			if (sideDecoration.Exists) {
				while (++sideDecoration.LastUsedIndex < sideDecoration.RowsSize) {
					writer.WriteLine(new string(' ', freeSpaces) + sideDecoration[sideDecoration.LastUsedIndex]);
				}
			}
			//if (writer == Console.Out) { Console.BackgroundColor = consoleBackupColor; }
		}

		public override string GetMainPageHeader() {
			return @"
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
		}

		public override void PrintPlugin(TextWriter writer, IPlugin plugin, int orderNumber) {
			string pluginToPrint = $"{ orderNumber } »» { plugin.Name } ««";
			string decorationImg = "";

			if (orderNumber < sideDecoration.RowsSize) {
				decorationImg = new string(' ', freeSpaces - pluginToPrint.Length) + sideDecoration[orderNumber - 1];
			}

			writer.WriteLine(pluginToPrint + decorationImg);
		}
	}
	class DescriptiveMainPagePrinter : CommonMainComponentPrinter {

		private static readonly DescriptiveMainPagePrinter instance = new DescriptiveMainPagePrinter(SimpleMainPagePrinter.Instance);

		public static DescriptiveMainPagePrinter Instance {
			get => instance;
		}

		private CommonMainComponentPrinter BaseMainPagePrinter { get; }

		private DescriptiveMainPagePrinter(CommonMainComponentPrinter basePrinter) {
			BaseMainPagePrinter = basePrinter;
		}

		public override void PrintMainPageContent(TextWriter writer, GeneralApplicationSettings settings) {
			writer.WriteLine(GetMainPageHeader());

			var plugins = BaseMainPagePrinter.GetAvailablePlugins(settings);

			for (int i = 0; i < plugins.Length; i++) {
				PrintPlugin(writer, plugins[i], i + 1);
			}
		}

		public override string GetMainPageHeader() {
			return BaseMainPagePrinter.GetMainPageHeader();
		}

		public override void PrintPlugin(TextWriter writer, IPlugin plugin, int orderNumber) {
			BaseMainPagePrinter.PrintPlugin(writer, plugin, orderNumber);
			writer.WriteLine($"   \u2192 { plugin.Description }\n");
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

			var filePaths = GeneralApplicationSettings.SearchForFiles(path, "*.txt");

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
