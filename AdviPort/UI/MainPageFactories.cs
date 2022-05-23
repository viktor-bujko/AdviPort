using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdviPort.UI {
	abstract class AbstractMainPageHandlerFactory {
		public abstract AbstractMainPageHandler GetMainPageHandler();
	}

	class ClassicMainPageHandlerFactory : AbstractMainPageHandlerFactory {

		private static ClassicMainPageHandlerFactory instance;
		private ClassicMainPageHandlerFactory() { }
		public static ClassicMainPageHandlerFactory GetFactory() {
			if (instance == null) {
				instance = new ClassicMainPageHandlerFactory();
			}

			return instance;
		}

		public override AbstractMainPageHandler GetMainPageHandler() => new ClassicMainPageHandler();
	}

	class DecorativeMainPageHandlerFactory : AbstractMainPageHandlerFactory {
		private static DecorativeMainPageHandlerFactory instance;
		private DecorativeMainPageHandlerFactory() { }
		public static DecorativeMainPageHandlerFactory GetFactory() {
			if (instance == null) {
				instance = new DecorativeMainPageHandlerFactory();
			}

			return instance;
		}

		public override AbstractMainPageHandler GetMainPageHandler() => new DecorativeMainPageHandler();
	}

	class DescriptiveMainPageHandlerFactory<T> : AbstractMainPageHandlerFactory
		where T : AbstractMainPageHandler, new() {

		private static DescriptiveMainPageHandlerFactory<T> instance;
		private DescriptiveMainPageHandlerFactory() { }

		public static DescriptiveMainPageHandlerFactory<T> GetFactory() {
			if (instance == null) {
				instance = new DescriptiveMainPageHandlerFactory<T>();
			}

			return instance;
		}

		public override AbstractMainPageHandler GetMainPageHandler() => DescriptiveMainPageHandler.GetInstance(new T());
	}

	class LoggedUserMainPageHandlerFactory : AbstractMainPageHandlerFactory {

		private AbstractMainPageHandler BaseHandler { get; }
		internal LoggedUserMainPageHandlerFactory(AbstractMainPageHandler handler) {
			BaseHandler = handler;
		}

		public override AbstractMainPageHandler GetMainPageHandler() => ShowLoggedUserMainPageHandler.GetInstance(BaseHandler);
	}
}
