using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AdviPort.Plugins;

namespace AdviPort {

	/// <summary>
	/// Interface which describes the ability to print main menu page.
	/// Classes implementing this interface should be able to print the content which consists
	/// of different plugins.
	/// </summary>
	interface IMainPagePrinter {
		void PrintMainPageContent(GeneralApplicationSettings settings, out int printedPlugins);

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
		public static IMainPageHandler SelectMainPageHandler(GeneralApplicationSettings settings) {

			CommonMainPageHandler handler = settings.MainPageStyle switch {
				"" => ClassicMainPageHandler.Instance,
				"classic" => ClassicMainPageHandler.Instance,
				"decorative" => DecorativeMainPageHandler.Instance,
				"descriptive" => DescriptiveMainPageHandler.GetInstance(ClassicMainPageHandler.Instance),
				"decorative/descriptive" => DescriptiveMainPageHandler.GetInstance(DecorativeMainPageHandler.Instance),
				"descriptive/decorative" => DescriptiveMainPageHandler.GetInstance(DecorativeMainPageHandler.Instance),
				_ => null,
			};

			if (handler == null) {
				Console.Error.WriteLine("Unsupported main page printer.");
				handler = ClassicMainPageHandler.Instance;
			}

			if (! Session.ActiveSession.HasLoggedUser)
				return handler;
			else
				return ShowLoggedUserMainPageHandler.GetInstance(handler);
		}
	}

	abstract class CommonMainPageHandler : IMainPageHandler {

		public abstract string MainPageHeader { get; }

		protected virtual IList<IPlugin> Plugins { get; set; }

		public abstract void PrintMainPageContent(GeneralApplicationSettings settings, out int printedPlugins);

		public abstract int PrintMainPagePluginOption(IPlugin plugin, int orderNumber);

		public abstract void PrintContentFooterSeparator(int maxPrinted);

		public virtual void PrintAvailablePlugins() {
			int maxPrinted = int.MinValue;

			for (int i = 0; i < Plugins.Count; i++) {
				var printedLength = PrintMainPagePluginOption(Plugins[i], i + 1);

				maxPrinted = printedLength > maxPrinted ? printedLength : maxPrinted;
			}

			PrintContentFooterSeparator(maxPrinted);
		}

		public virtual string ReadUserInput(string initialPrompt) {
			if (!(initialPrompt is null)) {
				Console.Write(initialPrompt + ": ");
			}

			var input = Console.ReadLine();

			if (input is null) throw new ArgumentNullException("The input should not be null.");

			return input.Trim();
		}

		public IPlugin HandlePluginChoice(string input) {

			if (Plugins is null || Plugins.Count == 0) { throw new ArgumentException("No plugins are available."); }

			if (string.IsNullOrWhiteSpace(input)) { throw new ArgumentException("Incorrect input string."); }

			if (int.TryParse(input, out int pluginOrderNumber)) {
				--pluginOrderNumber;  // Conversion from order number to Plugins array index.
				
				if (pluginOrderNumber >= 0 && pluginOrderNumber < Plugins.Count) {
					// Correct Plugins array index
					return Plugins[pluginOrderNumber];
				}

				// An incorrect number was entered.
				Console.Error.WriteLine($"Please make sure only numbers in correct range (1 - { Plugins.Count }) are entered.");
				return null;
			}
			
			// Not a number has been entered as input.
			if (PluginSelector.TryGetPluginFromInputString(input, Plugins, out List<IPlugin> filteredPlugins)) { 
				// Exactly one corresponding plugin has been found.
				return filteredPlugins[0]; 
			}

			if (filteredPlugins.Count == 0) {
				Console.Error.Write("No corresponding plugin has been found. Please type the order number or entire first word of the plugin.");
			} else {
				Console.Error.Write($"{filteredPlugins.Count} matching plugins has been found. Please specify an exact number of the plugin.");
				Plugins = filteredPlugins;
			}

			return null;
		}

		protected CommonMainPageHandler() { }
	}

	class ClassicMainPageHandler : CommonMainPageHandler {

		// Only a single instance of this type is expected to be needed.
		public static ClassicMainPageHandler Instance { get; } = new ClassicMainPageHandler();

		public override string MainPageHeader => @"
 █████╗ ██████╗ ██╗   ██╗██╗██████╗  ██████╗ ██████╗ ████████╗
██╔══██╗██╔══██╗██║   ██║██║██╔══██╗██╔═══██╗██╔══██╗╚══██╔══╝
███████║██║  ██║██║   ██║██║██████╔╝██║   ██║██████╔╝   ██║   
██╔══██║██║  ██║╚██╗ ██╔╝██║██╔═══╝ ██║   ██║██╔══██╗   ██║   
██║  ██║██████╔╝ ╚████╔╝ ██║██║     ╚██████╔╝██║  ██║   ██║   
╚═╝  ╚═╝╚═════╝   ╚═══╝  ╚═╝╚═╝      ╚═════╝ ╚═╝  ╚═╝   ╚═╝    
______________________________________________________________
";

		private ClassicMainPageHandler() { }

		public override void PrintMainPageContent(GeneralApplicationSettings settings, out int printedPlugins) {
			Console.WriteLine(MainPageHeader);

			Plugins = PluginSelector.GetAvailablePlugins(settings);
			printedPlugins = Plugins.Count;
			PrintAvailablePlugins();
		}
		
		public override int PrintMainPagePluginOption(IPlugin plugin, int orderNumber) {
			var pluginToPrint = $"{ orderNumber }) { plugin.Name }";
			Console.WriteLine(pluginToPrint);

			return pluginToPrint.Length;
		}

		public override void PrintContentFooterSeparator(int maxPrinted) {
			Console.WriteLine(new string('=', maxPrinted));
			Console.WriteLine();
		}

	}

	class DecorativeMainPageHandler : CommonMainPageHandler {

		public static DecorativeMainPageHandler Instance { get; } = new DecorativeMainPageHandler();

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

		private DecorativeMainPageHandler() {
			freeSpaces = 70;
			sideDecoration = new PageDecoration(@"..\..\..\decorations\tower_decoration.txt");
			planeDecoration = new PageDecoration(@"..\..\..\decorations\airplane_decoration.txt");
		}

		public override void PrintMainPageContent(GeneralApplicationSettings settings, out int printedPlugins) {
			Console.WriteLine(MainPageHeader);
			planeDecoration.Print(2, title: "«« MAIN MENU »»");
			Plugins = PluginSelector.GetAvailablePlugins(settings);
			printedPlugins = Plugins.Count;
			PrintAvailablePlugins();
		}

		public override void PrintAvailablePlugins() {
			//var consoleBackupColor = Console.BackgroundColor;
			var maxPrinted = int.MinValue;

			for (int i = 0; i < Plugins.Count; i++) {
				/*Console.BackgroundColor = (ConsoleColor)new Random().Next(1, 9);
				/if (Console.BackgroundColor == (ConsoleColor)7) { Console.BackgroundColor++; }*/
				 var printedLength = PrintMainPagePluginOption(Plugins[i], i + 1);

				maxPrinted = printedLength > maxPrinted ? printedLength : maxPrinted;
			}

			// Print the rest of the side decoration
			if (sideDecoration.Exists) {
				while (++sideDecoration.LastUsedIndex < sideDecoration.RowsSize) {
					Console.WriteLine(new string(' ', freeSpaces) + sideDecoration[sideDecoration.LastUsedIndex]);
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
			Console.WriteLine(pluginToPrint);

			return pluginToPrint.Length;
		}

		public override void PrintContentFooterSeparator(int maxPrinted) {
			Console.WriteLine(new string('~', maxPrinted));
			Console.WriteLine();
		}

		public override string ReadUserInput(string initialPrompt) {
			return base.ReadUserInput("»» " + initialPrompt);
		}
	}

	class DescriptiveMainPageHandler : CommonMainPageHandler {

		private static DescriptiveMainPageHandler Instance { get; set; }

		private CommonMainPageHandler BaseMainPageHandler { get; }

		protected override IList<IPlugin> Plugins { get; set; }

		public override string MainPageHeader => BaseMainPageHandler.MainPageHeader;

		private DescriptiveMainPageHandler(CommonMainPageHandler baseHandler) {
			BaseMainPageHandler = baseHandler;
		}

		public static DescriptiveMainPageHandler GetInstance(CommonMainPageHandler baseHandler) {
			if (Instance == null) {
				Instance = new DescriptiveMainPageHandler(baseHandler);
			}

			return Instance;
		}

		public override void PrintMainPageContent(GeneralApplicationSettings settings, out int printedPlugins) {
			Console.WriteLine(MainPageHeader);
			Plugins = PluginSelector.GetAvailablePlugins(settings);
			printedPlugins = Plugins.Count;

			var maxPrinted = int.MinValue;

			for (int i = 0; i < Plugins.Count; i++) {
				var printedLength = PrintMainPagePluginOption(Plugins[i], i + 1);

				maxPrinted = printedLength > maxPrinted ? printedLength : maxPrinted;
			}
			PrintContentFooterSeparator(maxPrinted);
		}

		public override int PrintMainPagePluginOption(IPlugin plugin, int orderNumber) {
			int printedLenght = BaseMainPageHandler.PrintMainPagePluginOption(plugin, orderNumber);
			string description = $"   \u2192 { plugin.Description }\n";
			Console.WriteLine(description);

			return Math.Max(printedLenght, description.Length);
		}

		public override void PrintContentFooterSeparator(int maxPrinted) {
			BaseMainPageHandler.PrintContentFooterSeparator(maxPrinted);
		}
	}

	class ShowLoggedUserMainPageHandler : CommonMainPageHandler {

		private static ShowLoggedUserMainPageHandler Instance { get; set; }
		private CommonMainPageHandler BaseHandler { get; }

		private ShowLoggedUserMainPageHandler(CommonMainPageHandler baseHandler) {
			BaseHandler = baseHandler;
		}

		public override string MainPageHeader => BaseHandler.MainPageHeader;

		public static ShowLoggedUserMainPageHandler GetInstance(CommonMainPageHandler baseHandler) {
			if (Instance == null || Instance.BaseHandler != baseHandler) {
				Instance = new ShowLoggedUserMainPageHandler(baseHandler);
			}

			return Instance;
		}

		public override void PrintMainPageContent(GeneralApplicationSettings settings, out int printedPlugins) {

			if (! Session.ActiveSession.HasLoggedUser) throw new ArgumentException("This main page handler should not be used without logged user.");

			Console.WriteLine(MainPageHeader);

			var loggedUser = Session.ActiveSession.LoggedUser;
			Console.WriteLine($"Welcome back, {loggedUser.UserName}\n");

			Plugins = PluginSelector.GetAvailablePlugins(settings);
			printedPlugins = Plugins.Count;

			if (Plugins.Contains(LoginPlugin.Instance)) {
				Plugins.Remove(LoginPlugin.Instance);
			}

			PrintAvailablePlugins();
		}

		public override int PrintMainPagePluginOption(IPlugin plugin, int orderNumber) => BaseHandler.PrintMainPagePluginOption(plugin, orderNumber);

		public override void PrintContentFooterSeparator(int maxPrinted) => BaseHandler.PrintContentFooterSeparator(maxPrinted);
	}
}
