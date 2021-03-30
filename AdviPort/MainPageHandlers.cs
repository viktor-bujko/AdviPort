﻿using System;
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

			CommonMainPageHandler handler = settings.MainPageStyle switch {
				"" => ClassicMainPageHandler.NewInstance(reader, writer),
				"classic" => ClassicMainPageHandler.NewInstance(reader, writer),
				"decorative" => DecorativeMainPageHandler.NewInstance(reader, writer),
				"descriptive" => DescriptiveMainPageHandler.NewInstance(ClassicMainPageHandler.NewInstance(reader, writer)),
				"decorative/descriptive" => DescriptiveMainPageHandler.NewInstance(DecorativeMainPageHandler.NewInstance(reader, writer)),
				"descriptive/decorative" => DescriptiveMainPageHandler.NewInstance(DecorativeMainPageHandler.NewInstance(reader, writer)),
				_ => null,
			};

			if (handler == null) {
				Console.Error.WriteLine("Unsupported main page printer.");
				handler = ClassicMainPageHandler.NewInstance(reader, writer);
			}

			if (! Session.ActiveSession.HasLoggedUser)
				return handler;
			else
				return ShowLoggedUserMainPageHandler.NewInstance(handler);
		}
	}

	abstract class CommonMainPageHandler : IMainPageHandler {

		public abstract string MainPageHeader { get; }

		public virtual TextReader Reader { get; }

		public virtual TextWriter Writer { get; }

		protected virtual IList<IPlugin> Plugins { get; set; }

		public abstract void PrintMainPageContent(GeneralApplicationSettings settings);

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
				Writer.Write(initialPrompt + ": ");
			}

			var input = Reader.ReadLine();

			if (input is null) throw new ArgumentNullException("The input should not be null.");

			return input.Trim();
		}

		public IPlugin HandlePluginChoice(string input) {

			if (Plugins is null || Plugins.Count == 0) { throw new ArgumentException("No plugins are available."); }

			if (input is null) { throw new ArgumentException("Incorrect input string."); }

			if (int.TryParse(input, out int pluginOrderNumber)) {
				--pluginOrderNumber;  // Conversion from order number to Plugins array index.
				
				if (pluginOrderNumber >= 0 && pluginOrderNumber < Plugins.Count) {
					// Correct Plugins array index
					return Plugins[pluginOrderNumber];
				}

				// An incorrect number was entered.
				Console.Error.WriteLine($"Please make sure only numbers in correct range (1 - {Plugins.Count - 1}) are entered.");
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

		protected CommonMainPageHandler(TextReader reader, TextWriter writer) {
			Reader = reader;
			Writer = writer;
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

		//protected virtual IPlugin[] Plugins { get; set; }

		private ClassicMainPageHandler(TextReader reader, TextWriter writer) : base(reader, writer) { }

		public static ClassicMainPageHandler NewInstance(TextReader reader, TextWriter writer) {
			if (Instance is null || Instance.Reader != reader || Instance.Writer != writer) {
				// Creating new instance only if it does not exist yet or if the conditions change
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

		//protected override IPlugin[] Plugins { get; set; }

		private DecorativeMainPageHandler(TextReader reader, TextWriter writer) : base(reader, writer) {
			freeSpaces = 70;
			sideDecoration = new PageDecoration(@"..\..\..\decorations\tower_decoration.txt");
			planeDecoration = new PageDecoration(@"..\..\..\decorations\airplane_decoration.txt");
		}

		public static DecorativeMainPageHandler NewInstance(TextReader reader, TextWriter writer) {
			if (Instance is null || Instance.Reader != reader || Instance.Writer != writer) {
				// Creating new instance only if it does not exist yet or if the conditions change
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

			for (int i = 0; i < Plugins.Count; i++) {
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

		protected override IList<IPlugin> Plugins { get; set; }

		public override string MainPageHeader => BaseMainPageHandler.MainPageHeader;

		private DescriptiveMainPageHandler(CommonMainPageHandler baseHandler) : base(baseHandler.Reader, baseHandler.Writer) {
			BaseMainPageHandler = baseHandler;
		}

		public static DescriptiveMainPageHandler NewInstance(CommonMainPageHandler baseHandler) {
			if (Instance == null) {
				Instance = new DescriptiveMainPageHandler(baseHandler);
			}

			return Instance;
		}

		public override void PrintMainPageContent(GeneralApplicationSettings settings) {
			Writer.WriteLine(MainPageHeader);
			Plugins = PluginSelector.GetAvailablePlugins(settings, Reader, Writer);

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
			Writer.WriteLine(description);

			return Math.Max(printedLenght, description.Length);
		}

		public override void PrintContentFooterSeparator(int maxPrinted) {
			BaseMainPageHandler.PrintContentFooterSeparator(maxPrinted);
		}
	}

	class ShowLoggedUserMainPageHandler : CommonMainPageHandler {

		private static ShowLoggedUserMainPageHandler Instance { get; set; }
		private CommonMainPageHandler BaseHandler { get; }

		private ShowLoggedUserMainPageHandler(CommonMainPageHandler baseHandler) : base(baseHandler.Reader, baseHandler.Writer) {
			BaseHandler = baseHandler;
		}

		public override string MainPageHeader => BaseHandler.MainPageHeader;

		public static ShowLoggedUserMainPageHandler NewInstance(CommonMainPageHandler baseHandler) {
			if (Instance == null || Instance.BaseHandler != baseHandler) {
				Instance = new ShowLoggedUserMainPageHandler(baseHandler);
			}

			return Instance;
		}

		public override void PrintMainPageContent(GeneralApplicationSettings settings) {

			if (! Session.ActiveSession.HasLoggedUser) throw new ArgumentException("This main page handler should not be used without logged user.");

			Writer.WriteLine(MainPageHeader);

			var loggedUser = Session.ActiveSession.LoggedUser;
			Writer.WriteLine($"Welcome back, {loggedUser.UserName}\n");

			Plugins = PluginSelector.GetAvailablePlugins(settings, Reader, Writer);

			if (Plugins.Contains(LoginPlugin.Instance)) {
				Plugins.Remove(LoginPlugin.Instance);
			}

			PrintAvailablePlugins();
		}

		public override int PrintMainPagePluginOption(IPlugin plugin, int orderNumber) => BaseHandler.PrintMainPagePluginOption(plugin, orderNumber);

		public override void PrintContentFooterSeparator(int maxPrinted) => BaseHandler.PrintContentFooterSeparator(maxPrinted);
	}
}
