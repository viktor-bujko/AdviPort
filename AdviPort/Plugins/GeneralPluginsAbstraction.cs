using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AdviPort.Plugins {
	interface IExecutablePlugin {
		int Invoke(object[] args);
	}

	interface IPlugin : IExecutablePlugin {
		string Name { get; }
		string Description { get; }
	}

	class PluginInputReader : IUserInterfaceReader {
		protected virtual TextReader Reader { get; }
		protected virtual TextWriter Writer { get; }

		public PluginInputReader(TextReader reader, TextWriter writer) {
			Reader = reader;
			Writer = writer;
		}

		public virtual string ReadUserInput(string initialPrompt) {
			if (!(initialPrompt is null)) {
				Writer.Write(initialPrompt + ": ");
			}

			var input = Reader.ReadLine();

			if (input is null) throw new ArgumentNullException("The input should not be null.");

			return input.Trim();
		}
	}
}
