﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AdviPort.Plugins;

namespace AdviPort.UI {

	/// <summary>
	/// Describes the ability to print main menu page. Classes implementing 
	/// this interface should be able to print the content which consists of 
	/// different plugins.
	/// </summary>
	interface IMainPagePrinter {
		int PrintMainPageContent(GeneralApplicationSettings settings);

		int PrintMainPagePluginOption(IPlugin plugin, int orderNumber);
	}

	/// <summary>
	/// Extends the <see cref="IMainPagePrinter"/> and <see cref="IUserInterfaceReader"/> 
	/// contracts. Classes implementing this interface should not only be able to print 
	/// out accessible application content, but also interact with user by reading its 
	/// input and by handling it appropriately. MainPageHeader read-only property extends 
	/// the MainPagePrinter by giving the printer a unique main page representation.
	/// </summary>
	interface IMainPageHandler : IMainPagePrinter, IUserInterfaceReader {
		string MainPageHeader { get; }

		IPlugin HandlePluginChoice(string input);
	}

	/// <summary>
	/// Main Page Handler Factory class.
	/// </summary>
	abstract class MainPageHandlerSelector {

		public static IList<string> MainPageDesignNames { get; } = new List<string> {
			"classic",
			"decorative",
			"descriptive",
			"decorative/descriptive"
		};

		/// <summary>
		/// Method responsible for selecting user interface style of application based on the application settings
		/// or logged user's chosen interface style.
		/// </summary>
		/// <param name="settings">Application settings descriptor.</param>
		/// <returns></returns>
		public static IMainPageHandler SelectMainPageHandler(GeneralApplicationSettings settings) {

			string mainPageStyle;

			if (Session.ActiveSession.HasLoggedUser &&
				MainPageDesignNames.Contains(Session.ActiveSession.LoggedUser.MainPageStyle)) {
				mainPageStyle = Session.ActiveSession.LoggedUser.MainPageStyle;
			} else mainPageStyle = settings.MainPageStyle;

			AbstractMainPageHandler handler = mainPageStyle switch {
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

			return Session.ActiveSession.HasLoggedUser 
						? ShowLoggedUserMainPageHandler.GetInstance(handler) 
						: handler;
		}
	}

	/// <summary>
	/// Abstract class which contains default implementations of several
	/// <see cref="IMainPageHandler"/> interface methods.
	/// </summary>
	abstract class AbstractMainPageHandler : IMainPageHandler {
		public abstract string MainPageHeader { get; }
		protected IReadOnlyList<IPlugin> Plugins { get; set; }

		/// <summary>
		/// Prints the content of the main page based on the general application
		/// settings and the style of the implementing class.
		/// </summary>
		/// <param name="settings">General application settings.</param>
		/// <returns>The number of printed actions.</returns>
		public abstract int PrintMainPageContent(GeneralApplicationSettings settings);

		/// <summary>
		/// Prints an overview of plugin action on the main page.
		/// </summary>
		/// <param name="plugin">Plugin to be printed on the main page.</param>
		/// <param name="orderNumber">The number of currently printed plugin.</param>
		/// <returns></returns>
		public abstract int PrintMainPagePluginOption(IPlugin plugin, int orderNumber);

		public abstract void PrintContentFooterSeparator(int maxPrinted);

		/// <summary>
		/// Prints a collection of plugins to the main page.
		/// </summary>
		/// <param name="plugins">A collection of plugins to be printed to the main page.</param>
		/// <returns>The number of printed plugins.</returns>
		public virtual int PrintAvailablePlugins(IEnumerable<IPlugin> plugins) {
			int maxPrinted = int.MinValue;

			int itemOrderNumber = 0;
			foreach(var plugin in plugins) {
				var printedLength = PrintMainPagePluginOption(plugin, ++itemOrderNumber);

				maxPrinted = printedLength > maxPrinted ? printedLength : maxPrinted;
			}

			PrintContentFooterSeparator(maxPrinted);

			return itemOrderNumber;
		}

		public virtual string ReadUserInput(string initialPrompt) {
			if (!(initialPrompt == null)) {
				Console.Write(initialPrompt + ": ");
			}

			var input = Console.ReadLine();

			if (input == null) throw new ArgumentNullException("The input should not be null.");

			return input.Trim();
		}

		/// <summary>
		/// Controls which (if any) plugin will be selected from the available set 
		/// of plugins based on the input of a user.
		/// </summary>
		/// <param name="input">The input string.</param>
		/// <returns>An instance of the chosen plugin.</returns>
		public IPlugin HandlePluginChoice(string input) {

			if (Plugins == null || Plugins.Count == 0) { throw new ArgumentException("No plugins are available."); }

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

		protected AbstractMainPageHandler() { }
	}


	/// <summary>
	/// A Main page controller with default classic design.
	/// </summary>
	class ClassicMainPageHandler : AbstractMainPageHandler {

		// Only a single instance of this type is expected to be needed.
		public static ClassicMainPageHandler Instance { get; } = new ClassicMainPageHandler();
		public sealed override string MainPageHeader => @"
 █████╗ ██████╗ ██╗   ██╗██╗██████╗  ██████╗ ██████╗ ████████╗
██╔══██╗██╔══██╗██║   ██║██║██╔══██╗██╔═══██╗██╔══██╗╚══██╔══╝
███████║██║  ██║██║   ██║██║██████╔╝██║   ██║██████╔╝   ██║   
██╔══██║██║  ██║╚██╗ ██╔╝██║██╔═══╝ ██║   ██║██╔══██╗   ██║   
██║  ██║██████╔╝ ╚████╔╝ ██║██║     ╚██████╔╝██║  ██║   ██║   
╚═╝  ╚═╝╚═════╝   ╚═══╝  ╚═╝╚═╝      ╚═════╝ ╚═╝  ╚═╝   ╚═╝    
______________________________________________________________
";
		private ClassicMainPageHandler() { }

		/// <summary><inheritdoc/></summary>
		/// <param name="settings"><inheritdoc/></param>
		/// <returns><inheritdoc/></returns>
		public override int PrintMainPageContent(GeneralApplicationSettings settings) {
			Console.WriteLine(MainPageHeader);

			Plugins = PluginSelector.GetAvailablePlugins(settings, PluginSelector.LoginLogoutFilter);
			return PrintAvailablePlugins(Plugins);
		}

		/// <summary><inheritdoc/></summary>
		/// <param name="plugin"><inheritdoc/></param>
		/// <param name="orderNumber"><inheritdoc/></param>
		/// <returns><inheritdoc/></returns>
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

	/// <summary>
	/// A class which provides some kind of decoration to the main page.
	/// </summary>
	class DecorativeMainPageHandler : AbstractMainPageHandler {

		public static DecorativeMainPageHandler Instance { get; } = new DecorativeMainPageHandler();

		private readonly int freeSpaces;
		private readonly PageDecoration sideDecoration, planeDecoration;

		public override string MainPageHeader => @"
      ___           ___                                                    ___           ___                   
     /  /\         /  /\          ___            ___         ___          /  /\         /  /\          ___     
    /  /::\       /  /::\        /  /\          /__/\       /  /\        /  /::\       /  /::\        /__/\    
   /  /:/\:\     /  /:/\:\      /  /:/          \__\:\     /  /::\      /  /:/\:\     /  /:/\:\       \  \:\   
  /  /::\ \:\   /  /:/  \:\    /  /:/           /  /::\   /  /:/\:\    /  /:/  \:\   /  /::\ \:\       \__\:\  
 /__/:/\:\_\:\ /__/:/ \__\:|  /__/:/  ___    __/  /:/\/  /  /::\ \:\  /__/:/ \__\:\ /__/:/\:\_\:\      /  /::\ 
 \__\/  \:\/:/ \  \:\ /  /:/  |  |:| /  /\  /__/\/:/~~  /__/:/\:\_\:\ \  \:\ /  /:/ \__\/~|::\/:/     /  /:/\:\
      \__\::/   \  \:\  /:/   |  |:|/  /:/  \  \::/     \__\/  \:\/:/  \  \:\  /:/     |  |:|::/     /  /:/__\/
      /  /:/     \  \:\/:/    |__|:|__/:/    \  \:\          \  \::/    \  \:\/:/      |  |:|\/     /__/:/     
     /__/:/       \__\::/      \__\::::/      \__\/           \__\/      \  \::/       |__|:|       \__\/      
     \__\/                                                                \__\/         \__\|                  

______________________________________________________________________________________________________________
";

		private DecorativeMainPageHandler() {
			freeSpaces = 70;
			sideDecoration = new PageDecoration(@"..\..\..\decorations\tower_decoration.txt");
			planeDecoration = new PageDecoration(@"..\..\..\decorations\airplane_decoration.txt");
		}

		/// <summary><inheritdoc/></summary>
		/// <param name="settings"><inheritdoc/></param>
		/// <returns><inheritdoc/></returns>
		public override int PrintMainPageContent(GeneralApplicationSettings settings) {
			Console.WriteLine(MainPageHeader);
			planeDecoration.Print(2, title: "«« MAIN MENU »»");
			Plugins = PluginSelector.GetAvailablePlugins(settings, PluginSelector.LoginLogoutFilter);
			return PrintAvailablePlugins(Plugins);
		}

		/// <summary><inheritdoc/></summary>
		/// <param name="plugins"><inheritdoc/></param>
		/// <returns><inheritdoc/></returns>
		public override int PrintAvailablePlugins(IEnumerable<IPlugin> plugins) {
			var maxPrinted = int.MinValue;

			int itemOrderNumber = 0;
			foreach (var plugin in plugins) {
				var printedLength = PrintMainPagePluginOption(plugin, ++itemOrderNumber);

				maxPrinted = printedLength > maxPrinted ? printedLength : maxPrinted;
			}

			// Print the rest of the side decoration
			if (sideDecoration.Exists) {
				while (++sideDecoration.LastUsedIndex < sideDecoration.RowsSize) {
					Console.WriteLine(new string(' ', freeSpaces) + sideDecoration[sideDecoration.LastUsedIndex]);
				}
			}

			PrintContentFooterSeparator(maxPrinted);
			return itemOrderNumber;
		}

		/// <summary><inheritdoc/></summary>
		/// <param name="plugin"><inheritdoc/></param>
		/// <param name="orderNumber"><inheritdoc/></param>
		/// <returns><inheritdoc/></returns>
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

		public override string ReadUserInput(string initialPrompt) => base.ReadUserInput("»» " + initialPrompt);

		private class PageDecoration {

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

			public bool Exists => !(Decoration == null);
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

				while (!((line = reader.ReadLine()) == null)) {
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

				if (filePaths == null) return result;

				for (int i = 0; i < filePaths.Length; i++) {
					var reader = GeneralApplicationSettings.GetTextReader(filePaths, i);
					if (reader == null) continue;

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

	/// <summary>
	/// A main page handler which, on top of another underlying main page 
	/// handler prints a description of each active plugin.
	/// </summary>
	class DescriptiveMainPageHandler : AbstractMainPageHandler {

		private static DescriptiveMainPageHandler Instance { get; set; }
		private AbstractMainPageHandler BaseHandler { get; set; }
		public override string MainPageHeader => BaseHandler.MainPageHeader;

		private DescriptiveMainPageHandler() { }

		public static DescriptiveMainPageHandler GetInstance(AbstractMainPageHandler baseHandler) {
			if (Instance == null) {
				Instance = new DescriptiveMainPageHandler();
			}

			Instance.BaseHandler = baseHandler;

			return Instance;
		}

		/// <summary><inheritdoc/></summary>
		/// <param name="settings"><inheritdoc/></param>
		/// <returns><inheritdoc/></returns>
		public override int PrintMainPageContent(GeneralApplicationSettings settings) {
			Console.WriteLine(MainPageHeader);
			Plugins = PluginSelector.GetAvailablePlugins(settings, PluginSelector.LoginLogoutFilter);
			return BaseHandler.PrintAvailablePlugins(Plugins);
		}

		/// <summary><inheritdoc/></summary>
		/// <param name="plugin"><inheritdoc/></param>
		/// <param name="orderNumber"><inheritdoc/></param>
		/// <returns><inheritdoc/></returns>
		public override int PrintMainPagePluginOption(IPlugin plugin, int orderNumber) {
			int printedLenght = BaseHandler.PrintMainPagePluginOption(plugin, orderNumber);
			string description = $"   \u2192 { plugin.Description }\n"; // unicode code for an arrow
			Console.WriteLine(description);

			return Math.Max(printedLenght, description.Length);
		}

		public override void PrintContentFooterSeparator(int maxPrinted) {
			BaseHandler.PrintContentFooterSeparator(maxPrinted);
		}
	}

	/// <summary>
	/// A main page handler which, on top of another underlying handler shows
	/// additional information about logged user.
	/// </summary>
	class ShowLoggedUserMainPageHandler : AbstractMainPageHandler {

		private static ShowLoggedUserMainPageHandler Instance { get; set; }
		private AbstractMainPageHandler BaseHandler { get; }
		private ShowLoggedUserMainPageHandler(AbstractMainPageHandler baseHandler) {
			BaseHandler = baseHandler;
		}

		public override string MainPageHeader => BaseHandler.MainPageHeader;

		public static ShowLoggedUserMainPageHandler GetInstance(AbstractMainPageHandler baseHandler) {
			if (Instance == null || Instance.BaseHandler != baseHandler) {
				Instance = new ShowLoggedUserMainPageHandler(baseHandler);
			}

			return Instance;
		}

		/// <summary><inheritdoc/></summary>
		/// <param name="settings"><inheritdoc/></param>
		/// <returns><inheritdoc/></returns>
		public override int PrintMainPageContent(GeneralApplicationSettings settings) {

			if (! Session.ActiveSession.HasLoggedUser) throw new ArgumentException("This main page handler should not be used without logged user.");

			Console.WriteLine(MainPageHeader);

			var loggedUser = Session.ActiveSession.LoggedUser;
			Console.WriteLine($"Welcome back, {loggedUser.UserName}\n");

			Plugins = PluginSelector.GetAvailablePlugins(settings, PluginSelector.LoginLogoutFilter);

			return BaseHandler.PrintAvailablePlugins(Plugins);
		}

		/// <summary><inheritdoc/></summary>
		/// <param name="plugin"><inheritdoc/></param>
		/// <param name="orderNumber"><inheritdoc/></param>
		/// <returns><inheritdoc/></returns>
		public override int PrintMainPagePluginOption(IPlugin plugin, int orderNumber) => BaseHandler.PrintMainPagePluginOption(plugin, orderNumber);

		public override void PrintContentFooterSeparator(int maxPrinted) => BaseHandler.PrintContentFooterSeparator(maxPrinted);
	}
}
