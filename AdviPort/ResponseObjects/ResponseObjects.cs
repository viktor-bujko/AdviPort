using System;
using System.Collections.Generic;
using System.Text;

namespace AdviPort.ResponseObjects {

	public class Aircraft {
		public string Model { get; set; }
		public string Reg { get; set; }
		public bool Active { get; set; }
		public string AirlineID { get; set; }
		public string IATACodeLong { get; set; }
		public int NumSeats { get; set; }
		public string DeliveryDate { get; set; }
		public string RegistrationDate { get; set; }
		public double AgeYears { get; set; }
	}

	public class Airline {
		public string Name { get; set; }
	}

	public class Flight {
		public Aircraft Aircraft { get; set; }
		public Airline Airline { get; set; }
		public Arrival Arrival { get; set; }
		public string CallSign { get; set; }
		public string CodeshareStatus { get; set; }
		public Departure Departure { get; set; }
		public bool IsCargo { get; set; }
		public string Number { get; set; }
		public string Status { get; set; }
	}

	public class Arrival {
		public string ActualTimeLocal { get; set; }
		public string ActualTimeUtc { get; set; }
		public string BaggageBelt { get; set; }
		public string RunwayTimeLocal { get; set; }
		public string RunwayTimeUtc { get; set; }
		public string ScheduledTimeLocal { get; set; }
		public string ScheduledTimeUtc { get; set; }
		public string Terminal { get; set; }
	}

	public class Departure {
		public string ActualTimeLocal { get; set; }
		public string ActualTimeUtc { get; set; }
		public Airport Airport { get; set; }
		public string CheckInDesk { get; set; }
		public string Gate { get; set; }
		public string RunwayTimeLocal { get; set; }
		public string RunwayTimeUtc { get; set; }
		public string ScheduledTimeLocal { get; set; }
		public string ScheduledTimeUtc { get; set; }
		public string Terminal { get; set; }
	}

	public class Airport {
		public string ICAO { get; set; }
		public string IATA { get; set; }
		public string Name { get; set; }
		public string ShortName { get; set; }
		public string FullName { get; set; }
		public string MunicipalityName { get; set; }
		public Location Location { get; set; }
		public Country Country { get; set; }
		public Continent Continent { get; set; }
		public string TimeZone { get; set; }
		public Urls Urls { get; set; }
	}

	class RapidAPIAirportREST {
		public string IATA { get; set; }
		public string ICAO { get; set; }
		public string Name { get; set; }
		public string Location { get; set; }
		public string City { get; set; }
		public string State { get; set; }
		public string Country_ISO { get; set; }
		public string Country { get; set; }
		public string Phone { get; set; }
		public double Latitude { get; set; }
		public double Longitude { get; set; }
		public string Website { get; set; }
	}

	public class Location {
		public float Lat { get; set; }
		public float Lon { get; set; }
	}

	public class Country {
		public string Code { get; set; }
		public string Name { get; set; }
	}

	public class Continent {
		public string Code { get; set; }
		public string Name { get; set; }
	}

	public class Urls {
		public string WebSite { get; set; }
		public string Wikipedia { get; set; }
		public string Twitter { get; set; }
		public string GoogleMaps { get; set; }
		public string FlightRadar { get; set; }
	}

	public class FlightTimeInfo {
		public Airport From { get; set; }
		public Airport To { get; set; }
		public GreatCircleDistance GreatCircleDistance { get; set; }
		public string ApproxFlightTime { get; set; }
	}

	public class GreatCircleDistance {
		public int Meter { get; set; }
		public float Km { get; set; }
		public float Mile { get; set; }
		public float NM { get; set; }
		public float Feet { get; set; }
	}
}
