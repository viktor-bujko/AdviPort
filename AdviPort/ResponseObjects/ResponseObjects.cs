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
		
		internal virtual string[] Labels { get; } = { "a", "b", "c" };

		public Aircraft Aircraft { get; set; }
		public Airline Airline { get; set; }
		public Arrival Arrival { get; set; }
		public string CallSign { get; set; }
		public string CodeshareStatus { get; set; }
		public Departure Departure { get; set; }
		public bool IsCargo { get; set; }
		public string Number { get; set; }
		public string Status { get; set; }

		public string ToString(byte[] cursorPositions, bool isArrival) {
			const string unknown = "------";
			StringBuilder sb = new StringBuilder();
			DateTime localTime, scheduledTime;
			if (isArrival) {
				localTime = DateTime.Parse(Arrival.ActualTimeLocal);
				scheduledTime = DateTime.Parse(Arrival.ScheduledTimeLocal);
			} else {
				localTime = DateTime.Parse(Departure.ActualTimeLocal);
				scheduledTime = DateTime.Parse(Departure.ScheduledTimeLocal);
			}

			string[] fields = {
				CallSign ?? unknown,
				Departure?.Airport?.Name ?? unknown,
				localTime.ToShortTimeString(),
				scheduledTime.ToShortTimeString(),
				Arrival?.Terminal ?? "--",
				Arrival?.BaggageBelt ?? "--",
				Status ?? unknown
			};

			if (cursorPositions == null) {
				Array.Fill<byte>(cursorPositions, 10);
			}

			int totalLength = 0;

			for (int i = 0; i < fields.Length; i++) {
				sb.Append(fields[i]);
				totalLength += fields[i].Length;
				string padding = new string(' ', Math.Max(0, cursorPositions[i] - totalLength));
				sb.Append(padding);
				totalLength += padding.Length;
			}
			return sb.ToString();
		}
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

	public class Schedule {
		public Flight[] Arrivals { get; set; } = new Flight[0];
		public Flight[] Departures { get; set; } = new Flight[0];

		private string GetTitleCenterLine(string title) => new string(' ', (80 - title.Length) / 2) + title;

		public override string ToString() {
			var sb = new StringBuilder();
			byte[] firstPositions = {
				12, 36, 45, 56, 66, 73, 80
			};

			sb.AppendLine(GetTitleCenterLine("ARRIVALS"));
			sb.AppendLine("Flight      Arriving From         Scheduled  ETA     Terminal    Belt    Status");
			sb.AppendLine("-------------------------------------------------------------------------------");
			sb.AppendLine(Arrivals.FlightsTableToString(firstPositions, true));
			sb.Append("\n\n");

			sb.AppendLine(GetTitleCenterLine("DEPARTURES"));
			sb.AppendLine("Flight      Departing To          Scheduled  ETD     Terminal    Gate    Status");
			sb.AppendLine("-------------------------------------------------------------------------------");
			sb.AppendLine(Departures.FlightsTableToString(firstPositions, false));
			return sb.ToString();
		}
	}

	static class FlightArrayExtensions {
		public static string FlightsTableToString(this Flight[] flights, byte[] firstPositions, bool isArrival, int startIndex = 0, int length = 20) {
			var sb = new StringBuilder();
			int endIndex = Math.Min(startIndex + length, flights.Length);

			for (int i = startIndex; i < endIndex; i++) {
				sb.AppendLine(flights[i].ToString(firstPositions, isArrival));
			}

			return sb.ToString();
		}
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
