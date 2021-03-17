using System;
using System.Collections.Generic;
using System.Text;

namespace AdviPort {

	interface IAirportFinder {
		void FindAirportByCode(string airportIcaoCode);
	}

	class AirportInfoHandling : IAirportFinder {
		public void FindAirportByCode(string airportIcaoCode) {
			return;
		}
	}
}
