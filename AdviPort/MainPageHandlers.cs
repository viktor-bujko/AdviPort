using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AdviPort {

	/// <summary>
	/// Interface which describes the ability to print main menu page.
	/// Classes implementing this interface should be able to print the content which consists
	/// of different plugins.
	/// </summary>
	interface IMainPagePrinter {
		void PrintMainPageContent(GeneralApplicationSettings settings);

		int PrintMainPagePluginOption(IPlugin plugin, int orderNumber);
	}

	/// <summary>
	/// Interface which describes input reading contract. The contract can be fulfilled using
	/// any available reader object.
	/// </summary>
	interface IUserInterfaceReader {
		string ReadUserInput(string initialPrompt = "Please enter your choice");
	}

	/// <summary>
	/// Interface which extends the contracts of <code>IMainPagePrinter</code> and <code>IUserInterfaceReader</code>.
	/// Classes implementing this interface should not only be able to print out accessible application content, but 
	/// also interact with user by reading its input and by handling it appropriately. MainPageHeader read-only property 
	/// extends the MainPagePrinter by giving the printer a unique main page representation.
	/// <seealso cref="IMainPagePrinter"/>
	/// <seealso cref="IUserInterfaceReader"/>
	/// </summary>
	interface IMainPageHandler : IMainPagePrinter, IUserInterfaceReader {
		string MainPageHeader { get; }

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
}
