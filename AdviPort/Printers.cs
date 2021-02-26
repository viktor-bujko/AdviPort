using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AdviPort {

	interface IPrinter {
		void Print(TextWriter writer, GeneralApplicationSettings settings);
	}

	abstract class MainPagePrinter : IPrinter {
		protected IPrinter Decorator { get; }

		public MainPagePrinter(IPrinter printer) {
			Decorator = printer;
		}

		internal static MainPagePrinter SelectPrinter(string mainPageStyle) {

			var basic = new BaseMainPagePrinter();

			MainPagePrinter printer = mainPageStyle switch {
				"" => new SimpleMainPagePrinter(basic),
				"simple" => new SimpleMainPagePrinter(basic),
				"decorative" => new DecorativeMainPagePrinter(basic),
				"descriptive" => new DescriptiveMainPagePrinter(basic),
				_ => throw new ArgumentException("Unsupported main page printer."),
			};
			return printer;
		}

		public virtual void Print(TextWriter writer, GeneralApplicationSettings settings) {
			Decorator.Print(writer, settings);
		}
	}

	class BaseMainPagePrinter : IPrinter {

		public void Print(TextWriter writer, GeneralApplicationSettings settings) {
			writer.WriteLine(@"
 █████╗ ██████╗ ██╗   ██╗██╗██████╗  ██████╗ ██████╗ ████████╗
██╔══██╗██╔══██╗██║   ██║██║██╔══██╗██╔═══██╗██╔══██╗╚══██╔══╝
███████║██║  ██║██║   ██║██║██████╔╝██║   ██║██████╔╝   ██║   
██╔══██║██║  ██║╚██╗ ██╔╝██║██╔═══╝ ██║   ██║██╔══██╗   ██║   
██║  ██║██████╔╝ ╚████╔╝ ██║██║     ╚██████╔╝██║  ██║   ██║   
╚═╝  ╚═╝╚═════╝   ╚═══╝  ╚═╝╚═╝      ╚═════╝ ╚═╝  ╚═╝   ╚═╝                                                                 
			");
		}
	}

	class SimpleMainPagePrinter : MainPagePrinter {

		public SimpleMainPagePrinter(IPrinter printer) : base(printer) { }
	}

	class DecorativeMainPagePrinter : MainPagePrinter { 

		public DecorativeMainPagePrinter(IPrinter printer) : base(printer) { }

		public override void Print(TextWriter writer, GeneralApplicationSettings settings) {
			writer.WriteLine(@"
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
");
		}
	}

	class DescriptiveMainPagePrinter : MainPagePrinter {

		public DescriptiveMainPagePrinter(IPrinter printer) : base(printer) { }
	}

}
