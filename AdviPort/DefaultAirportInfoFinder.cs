using System;
using System.Collections.Generic;
using System.Text;

namespace AdviPort {

	interface IAirportFinder {
		void FindAirportByCode(string airportIcaoCode);
	}

	abstract class AviationStackInformation {
		protected Uri BasePartURI { get; } = new Uri(@"https://api.aviationstack.com/v1/");
	}

	class DefaultAirportInfoFinder : AviationStackInformation, IAirportFinder {

		private string AirportSpecificPartURI { get; } = "airports";
		public void FindAirportByCode(string airportIcaoCode) {
			var loggedUser = Session.ActiveSession.LoggedUser;

			if (loggedUser == null) {
				Console.Error.WriteLine("You have to log in first before using application services.");
				throw new ArgumentNullException();
			}


		}
	}
}
