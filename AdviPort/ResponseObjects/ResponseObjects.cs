using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AdviPort.ResponseObjects {

	// https://rapidapi.com/aerodatabox/api/aerodatabox/

	/// <summary>Aircraft object descriptor. Contains JSON (de)serializable properties used to describe an aircraft.</summary>
	interface IAircraft {
		string Model { get; set; }
		string Reg { get; set; }
		string AirlineID { get; set; }
		string IATACodeLong { get; set; }
		int NumSeats { get; set; }
		string DeliveryDate { get; set; }
		string RegistrationDate { get; set; }
		double AgeYears { get; set; }
	}

	/// <summary>Airline object descriptor used for (de)serialization.</summary>
	interface IAirLine {
		string Name { get; set; }
	}

	/// <summary>Airport object descriptor. Used to (de)serialize an instance of an airport.</summary>
	interface IAirport {
		string ICAO { get; set; }
		string IATA { get; set; }
		string Name { get; set; }
		string ShortName { get; set; }
		string FullName { get; set; }
		string MunicipalityName { get; set; }
		string CountryCode { get; set; }
		Location Location { get; set; }
		GeoEntity Country { get; set; }
		GeoEntity Continent { get; set; }
		string TimeZone { get; set; }
		Urls Urls { get; set; }
	}

	interface IDimension {
		double Meter { get; set; }
		double Km { get; set; }
		double Mile { get; set; }
		double NM { get; set; }
		double Feet { get; set; }
	}

	interface IFlight {
		Aircraft Aircraft { get; set; }
		Airline Airline { get; set; }
		DepartureArrival Movement { get; set; }
		DepartureArrival Departure { get; set; }
		DepartureArrival Arrival { get; set; }
		Dimension GreatCircleDistance { get; set; }
		string CallSign { get; set; }
		bool IsCargo { get; set; }
		string Number { get; set; }
		string Status { get; set; }
		void PrintColoredFlightScheduleEntry(byte[] cursorPositions, bool isArrival);
		}

	/// <summary>Runway object descriptor.</summary>
	interface IRunway {
		public string Name { get; set; }
		public float TrueHDG { get; set; }
		public string Surface { get; set; }
		public Dimension Length { get; set; }
		public Dimension Width { get; set; }
	}

	interface ILocation {
		float Lat { get; set; }
		float Lon { get; set; }
	}

	interface IGeoEntity {
		string Code { get; set; }
		string Name { get; set; }
	}

	interface ISchedule {
	}

	/// <summary><inheritdoc/></summary>
	public struct Aircraft : IAircraft {
		public string Model { get; set; }
		public string Reg { get; set; }
		public string AirlineID { get; set; }
		public string IATACodeLong { get; set; }
		public int NumSeats { get; set; }
		public string DeliveryDate { get; set; }
		public string RegistrationDate { get; set; }
		public double AgeYears { get; set; }
	}

	/// <summary><inheritdoc/></summary>
	public struct Airline : IAirLine {
		public string Name { get; set; }
	}

	/// <summary>
	/// Flight object descriptor. Contains JSON (de)serializable properties used to describe a flight. 
	/// </summary>
	public class Flight : IFlight {

		public Aircraft Aircraft { get; set; }
		public Airline Airline { get; set; }
		public DepartureArrival Movement { get; set; }
		public DepartureArrival Departure { get; set; }
		public DepartureArrival Arrival { get; set; }
		public Dimension GreatCircleDistance { get; set; }
		public string CallSign { get; set; }
		public string CodeshareStatus { get; set; }
		public bool IsCargo { get; set; }
		public string Number { get; set; }
		public string Status { get; set; }

		public void PrintColoredFlightScheduleEntry(byte[] cursorPositions, bool isArrival) {
			const string unknown = "------";
			DateTime.TryParse(Movement.ScheduledTimeLocal, out DateTime scheduledTime);
			DateTime.TryParse(Movement.ActualTimeLocal, out DateTime localTime);

			Action<string, Flight> defaultPrintAction = (value, _) => Console.Write(value);

			var fieldsToPrint = new List<ValueTuple<string, Action<string, Flight>>> {
				( Number ?? unknown, defaultPrintAction ),
				( Movement.Airport?.Name ?? unknown, defaultPrintAction ),
				( scheduledTime.ToShortTimeString(),  defaultPrintAction ),
				( Status.Contains("Canceled") || localTime == DateTime.MinValue ? "--:--" : localTime.ToShortTimeString(), (time, _) => {
						DateTime now = DateTime.Now;

						if (!isArrival && DateTime.TryParse(time, out DateTime dateTime) && now.AddMinutes(15).CompareTo(dateTime) > 0) {
							Console.ForegroundColor = ConsoleColor.DarkRed;
							Console.Write(time);
							Console.ForegroundColor = ConsoleColor.White;
						} else Console.Write(time);
					}
				),
				( Movement.Terminal ?? "--", defaultPrintAction ),
				( isArrival ? Movement.BaggageBelt ?? "--" : Movement.Gate ?? "--", defaultPrintAction ),
				( Status ?? unknown, (status, flight) => {
						Console.ForegroundColor = flight.SelectColorFromFlightStatus();
						Console.Write(status);
						Console.ForegroundColor = ConsoleColor.White;
					}
				),
				( !isArrival ? Movement.CheckInDesk ?? "--" : "        ", defaultPrintAction )
			};


			if (cursorPositions == null) {
				cursorPositions = new byte[fieldsToPrint.Count];
				Array.Fill<byte>(cursorPositions, 15);
			}

			PrintFlightScheduleEntryFields(fieldsToPrint, cursorPositions);
		}

		/// <summary>
		/// Printing a collection of 
		/// </summary>
		/// <param name="valuesToPrint"></param>
		/// <param name="cursorPositions"></param>
		private void PrintFlightScheduleEntryFields(IEnumerable<(string, Action<string, Flight>)> valuesToPrint, byte[] cursorPositions) {
			int totalLength = 0;
			int idx = 0;

			foreach (var (value, printMethod) in valuesToPrint) {
				printMethod(value, this);
				totalLength += value.Length;

				var padding = new string(' ', Math.Max(0, cursorPositions[idx] - totalLength));
				Console.Write(padding);
				totalLength += padding.Length;
				idx++;
			}
			Console.WriteLine();
		}

		internal ConsoleColor SelectColorFromFlightStatus() {
			ConsoleColor color = Status switch {
				"Departed" => ConsoleColor.Green,
				"Arrived" => ConsoleColor.Green,
				"Delayed" => ConsoleColor.Red,
				"Canceled" => ConsoleColor.DarkRed,
				"GateClosed" => ConsoleColor.Blue,
				"Boarding" => ConsoleColor.DarkYellow,
				_ => Console.ForegroundColor
			};

			if (Status.Contains("Canceled")) return ConsoleColor.DarkRed;

			return color;
		}
	}

	/// <summary>Type of flight object descriptor. Contains properties used to (de)serialize either an arrival or a departure flight.</summary>
	public struct DepartureArrival {
		public string ActualTimeLocal { get; set; }
		public string ActualTimeUtc { get; set; }
		public Airport Airport { get; set; }
		public string BaggageBelt { get; set; }
		public string CheckInDesk { get; set; }
		public string Gate { get; set; }
		public string RunwayTimeLocal { get; set; }
		public string RunwayTimeUtc { get; set; }
		public string ScheduledTimeLocal { get; set; }
		public string ScheduledTimeUtc { get; set; }
		public string Terminal { get; set; }
	}

	/// <summary>Flight schedule descriptor. Provides the description of the list of flights contained in the schedule.</summary>
	public struct Schedule : ISchedule {

		private int ScheduleWidth { get => 95; }
		public Flight[] Arrivals { get; set; }
		public Flight[] Departures { get; set; }

		private string GetTitleCenterLine(string title) {
			return new string(' ', (ScheduleWidth - title.Length) / 2) + title;
		}

		public void PrintSchedule() {
			// experimentally chosen constants for schedule entry indentation
			byte[] firstPositions = { 12, 36, 45, 56, 66, 73, 85, 95};
			var hLine = new string('-', ScheduleWidth);
			var dateTimeNow = DateTime.Now;

			Console.WriteLine();
			Console.WriteLine(GetTitleCenterLine($"Time now is: {dateTimeNow.ToShortDateString()} {dateTimeNow.ToShortTimeString()}"));
			Console.WriteLine(GetTitleCenterLine("ARRIVALS"));
			Console.WriteLine("Flight      Arriving From         Scheduled   ETA    Terminal    Belt    Status");
			Console.WriteLine(hLine);
			Arrivals?.PrintFlightsSchedule(firstPositions, true);
			Console.WriteLine("\n");

			Console.WriteLine(GetTitleCenterLine("DEPARTURES"));
			Console.WriteLine("Flight      Departing To          Scheduled   ETD    Terminal    Gate     Status    Check-In");
			Console.WriteLine(hLine);
			Departures?.PrintFlightsSchedule(firstPositions, false);
			Console.WriteLine();
		}
	}

	/// <summary>Static class which provides extension methods for printing flights in a flight array.</summary>
	static class IFlightListExtensions {
		public static void PrintFlightsSchedule(this IList<IFlight> flights, byte[] firstPositions, bool isArrival, int startIndex = 0, int length = 20) {
			int endIndex = Math.Min(startIndex + length, flights.Count);

			for (int i = startIndex; i < endIndex; i++) {
				flights[i].PrintColoredFlightScheduleEntry(firstPositions, isArrival);
			}
		}

		public static string FlightInfoToString(this IList<IFlight> flights) {
			var sb = new StringBuilder();
			IFlight operatorFlight = flights[0];
			var flightNumber = operatorFlight.Number;

			Console.WriteLine("Empty / non filled values are unknown.");

			var hLine = new string('-', 20);

			sb.AppendLine(hLine + flightNumber + hLine);
			sb.Append("Callsigns (flight codes with codeshares):");
			foreach (var flight in flights) { sb.Append($" {flight.CallSign} /"); }
			sb.AppendLine();
			sb.AppendLine($"Flight from: {operatorFlight.Departure.Airport?.Name} ({operatorFlight.Departure.Airport?.CountryCode})");
			sb.AppendLine($"\tTerminal: {operatorFlight.Departure.Terminal}");
			sb.AppendLine($"\t    Gate: {operatorFlight.Departure.Gate}");
			sb.AppendLine($"Flight to: {operatorFlight.Arrival.Airport?.Name} ({operatorFlight.Arrival.Airport?.CountryCode})");
			sb.AppendLine($"\tTerminal: {operatorFlight.Arrival.Terminal}");
			sb.AppendLine($"\t    Gate: {operatorFlight.Arrival.Gate}");
			sb.Append($"Great Circle Distance ({operatorFlight.Departure.Airport?.IATA} -> {operatorFlight.Arrival.Airport?.IATA}): ");
			sb.AppendLine($"{operatorFlight.GreatCircleDistance.Km} km ({operatorFlight.GreatCircleDistance.Mile} miles / {operatorFlight.GreatCircleDistance.NM} NM)");
			sb.AppendLine("Operated by:");
			sb.AppendLine($"\t  Airline: {operatorFlight.Airline.Name}");
			sb.AppendLine($"\t Aircraft: {operatorFlight.Aircraft.Model} ({operatorFlight.Aircraft.Reg})");
			sb.AppendLine($"Status: {operatorFlight.Status}");
			string flightType = operatorFlight.IsCargo ? "cargo" : "passenger";
			sb.AppendLine($"Flight is a {flightType} flight.");
			sb.AppendLine(hLine + hLine + new string('-', flightNumber.Length));

			return sb.ToString();
		}
	}

	/// <summary><inheritdoc/></summary>
	public class Airport : IAirport {
		public string ICAO { get; set; }
		public string IATA { get; set; }
		public string Name { get; set; }
		public string ShortName { get; set; }
		public string FullName { get; set; }
		public string MunicipalityName { get; set; }
		public string CountryCode { get; set; }
		public Location Location { get; set; }
		public GeoEntity Country { get; set; }
		public GeoEntity Continent { get; set; }
		public string TimeZone { get; set; }
		public Urls Urls { get; set; }

		public string BuildAirportInfoTable(Runway[] runways, bool includeRWYs) {
			var sb = new StringBuilder();
			string lat = Location.Lat > 0 ? "N " : "S ",
					lon = Location.Lon > 0 ? "E " : "W ";

			string[] content = {
				$"ICAO / IATA: {ICAO} / {IATA}",
				$"Airport Name: {FullName} / {ShortName}",
				$"Country: {Country.Name} ({Country.Code})",
				$"Location:",
				'\t' + lat + Math.Abs(Location.Lat).ToString(),
				'\t' + lon + Math.Abs(Location.Lon).ToString(),
				$"Airport urls:",
				'\t' + Urls.WebSite,
				'\t' + Urls.Wikipedia,
				'\t' + Urls.GoogleMaps,
				'\t' + Urls.Twitter
			};

			var maxLength = content.Max(info => info.Length);
			string vLine = "| ";
			string hLine = new string('_', maxLength + vLine.Length);

			sb.AppendLine(hLine);
			foreach (var info in content) {
				sb.AppendLine(vLine + info);
			}

			if (!includeRWYs) {
				sb.AppendLine(hLine);
				return sb.ToString();
			}

			sb.AppendLine(vLine + "Runways:");
			foreach (var runway in runways) {
				sb.AppendLine($"{vLine}	Runway: {runway.Name}");
				sb.AppendLine($"{vLine}		True Heading: {runway.TrueHDG}");
				sb.AppendLine($"{vLine}		Runway Surface: {runway.Surface}");
				sb.AppendLine($"{vLine}		Length: {runway.Length.Meter} meters ({runway.Length.Feet} feet)");
				sb.AppendLine($"{vLine}		Width: {runway.Width.Meter} meters ({runway.Width.Feet} feet)");
				sb.AppendLine($"{vLine}	_______________");
			}
			sb.AppendLine(hLine);

			return sb.ToString();
		}
	}

	/// <summary><inheritdoc/></summary>
	public struct Runway : IRunway {
		public string Name { get; set; }
		public float TrueHDG { get; set; }
		public string Surface { get; set; }
		public Dimension Length { get; set; }
		public Dimension Width { get; set; }
	}

	public struct Location : ILocation {
		public float Lat { get; set; }
		public float Lon { get; set; }
	}

	public struct GeoEntity : IGeoEntity {
		public string Code { get; set; }
		public string Name { get; set; }
	}

	public struct Urls {
		public string WebSite { get; set; }
		public string Wikipedia { get; set; }
		public string Twitter { get; set; }
		public string GoogleMaps { get; set; }
		public string FlightRadar { get; set; }
	}

	public struct Dimension : IDimension {
		public double Meter { get; set; }
		public double Km { get; set; }
		public double Mile { get; set; }
		public double NM { get; set; }
		public double Feet { get; set; }
	}
}
