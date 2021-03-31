using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace AdviPort {

	interface IAirportProvider {
		ResponseObjects.Airport GetAirportByICAO(string airportIcaoCode);
	}

	interface IAirportScheduleProvider {
		ResponseObjects.Schedule GetAirportSchedule(string airportIcaoCode);
	}

	interface IProvider : IAirportProvider, IAirportScheduleProvider {
		string RootURI { get; }
	}

	#region RapidAPI

	class AeroDataBoxProvider : IProvider {

		public string RootURI { get; } = @"https://aerodatabox.p.rapidapi.com";

		private readonly JsonSerializerOptions options = new JsonSerializerOptions() {
			PropertyNameCaseInsensitive = true
		};

		public async Task<string> GetResponseFromServer(Uri destinationUri, string apiKey) {
			var client = new HttpClient();

			var request = new HttpRequestMessage {
				Method = HttpMethod.Get,
				RequestUri = destinationUri,
				Headers = {
					{ "x-rapidapi-key", apiKey },
					{ "x-rapidapi-host", "aerodatabox.p.rapidapi.com" }
				},
			};

			using var rawResponse = await client.SendAsync(request);

			if (! rawResponse.IsSuccessStatusCode) {
				Console.Error.WriteLine("Selected airport could not be added to favourite airports. Try checking you have entered an existing ICAO airport code.");
				Console.Error.WriteLine(rawResponse.ReasonPhrase);
				return null;
			}
			return rawResponse.Content.ReadAsStringAsync().Result;	
		}

		public ResponseObjects.Airport GetAirportByICAO(string airportIcaoCode) {
			if (! Session.ActiveSession.HasLoggedUser) {
				throw new ArgumentNullException("A user has to be logged in order to use this method.");
			}

			var loggedUser = Session.ActiveSession.LoggedUser;
			string apiKey = Encryptor.Decrypt(loggedUser.APIKey);

			var uri = new Uri($"{ RootURI }/airport?icao={ airportIcaoCode.ToLower() }");

			//var response = GetResponseFromServer(uri, apiKey);
			Console.WriteLine($"Got successful response for airport \"{ airportIcaoCode }\"");

			//var body = "{\"id\":1213,\"iata\":\"CDG\",\"icao\":\"LFPG\",\"name\":\"Charles de Gaulle Airport (Roissy Airport)\",\"location\":\"Paris, Île-de-France, France\",\"street_number\":\"\",\"street\":\"\",\"city\":\"Roissy-en-France\",\"county\":\"\",\"state\":\"Île-de-France\",\"country_iso\":\"FR\",\"country\":\"France\",\"postal_code\":\"95700\",\"phone\":\"+33 1 70 36 39 50\",\"latitude\":49.00969,\"longitude\":2.5479245,\"uct\":120,\"website\":\"http://www.parisaeroport.fr/\"}\n";
			var response = "{\"icao\": \"EDDF\", \"iata\": \"FRA\", \"shortName\": \"Frankfurt-am-Main\", \"fullName\": \"Frankfurt-am-Main\", \"municipalityName\": \"Frankfurt-am-Main\", \"location\": { \"lat\": 50.0264, \"lon\": 8.543129}, \"country\": { \"code\": \"DE\", \"name\": \"Germany\"}, \"continent\": { \"code\": \"EU\", \"name\": \"Europe\" }, \"timeZone\": \"Europe/Berlin\", \"urls\": { \"WebSite\": \"http://www.frankfurt-airport.de/ \", \"Wikipedia\": \"https://en.wikipedia.org/wiki/Frankfurt_Airport \", \"Twitter\": \"http://twitter.com/Airport_FRA \", \"GoogleMaps\": \"https://www.google.com/maps/@50.026401,8.543129,14z \", \"FlightRadar\": \"https://www.flightradar24.com/50.03,8.54/14 \" } }";

			using (TextWriter wr = new StreamWriter("./schedule_response.json")) {
				wr.WriteLine(response);
			}
			var airportObject = JsonSerializer.Deserialize<ResponseObjects.Airport>(response, options);

			return airportObject;
		}

		public ResponseObjects.Schedule GetAirportSchedule(string airportIcaoCode) {

			if (! Session.ActiveSession.HasLoggedUser) {
				throw new ArgumentNullException("A user has to be logged in order to use this method.");
			}

			var loggedUser = Session.ActiveSession.LoggedUser;
			string apiKey = loggedUser.APIKey;
			DateTime utcNow = DateTime.UtcNow;

			var uri = new Uri($"{ RootURI }/flights/airports/icao/{ airportIcaoCode }/{ utcNow.ToString("yyyy-MM-ddTHH:mm") }/{ utcNow.AddHours(3).ToString("yyyy-MM-ddTHH:mm") }?withLeg=false&direction=Both&withCancelled=true&withCodeshared=true");

			//var response = GetResponseFromServer(uri, apiKey);
			var response = "{\"arrivals\": [{\"aircraft\": {\"model\": \"Airbus A320\", \"reg\": \"VP-BJW\" }, \"airline\": { \"name\": \"Aeroflot\" }, \"arrival\": { \"actualTimeLocal\": \"2019-12-26 12:06+03:00\", \"actualTimeUtc\": \"2019-12-26 09:06Z\",\"baggageBelt\": \"6\", \"runwayTimeLocal\": \"2019-12-26 11:54+03:00\",\"runwayTimeUtc\": \"2019-12-26 08:54Z\",\"scheduledTimeLocal\": \"2019-12-26 12:00+03:00\",\"scheduledTimeUtc\": \"2019-12-26 09:00Z\",\"terminal\": \"B\"},\"callSign\": \"AFL011\",\"codeshareStatus\": \"IsOperator\", \"departure\": {\"actualTimeLocal\":\"2019-12-26 10:34+03:00\", \"actualTimeUtc\":\"2019-12-26 07:34Z\", \"airport\": {\"iata\":\"LED\", \"icao\":\"ULLI\", \"name\":\"Saint-Petersburg\" }, \"checkInDesk\":\"201-206\", \"gate\":\"D02\", \"runwayTimeLocal\":\"2019-12-26 10:54 +03:00\", \"runwayTimeUtc\":\"2019-12-26 07:54Z\", \"scheduledTimeLocal\":\"2019-12-26 10:40 +03:00\",  \"scheduledTimeUtc\":\"2019-12-26 07:40Z\", \"terminal\":\"1\" },  \"isCargo\": false,\"number\": \"SU 11\",\"status\": \"Arrived\"}, {\"aircraft\": {\"model\": \"Airbus A320\", \"reg\": \"VP-BJW\" }, \"airline\": { \"name\": \"Aeroflot\" }, \"arrival\": { \"actualTimeLocal\": \"2019-12-26 12:06+03:00\", \"actualTimeUtc\": \"2019-12-26 09:06Z\",\"baggageBelt\": \"6\", \"runwayTimeLocal\": \"2019-12-26 11:54+03:00\",\"runwayTimeUtc\": \"2019-12-26 08:54Z\",\"scheduledTimeLocal\": \"2019-12-26 12:00+03:00\",\"scheduledTimeUtc\": \"2019-12-26 09:00Z\",\"terminal\": \"B\"},\"callSign\": \"AFL011\",\"codeshareStatus\": \"IsOperator\", \"departure\": {\"actualTimeLocal\":\"2019-12-26 10:34+03:00\", \"actualTimeUtc\":\"2019-12-26 07:34Z\", \"airport\": {\"iata\":\"LED\", \"icao\":\"ULLI\", \"name\":\"Saint-Petersburg\" }, \"checkInDesk\":\"201-206\", \"gate\":\"D02\", \"runwayTimeLocal\":\"2019-12-26 10:54 +03:00\", \"runwayTimeUtc\":\"2019-12-26 07:54Z\", \"scheduledTimeLocal\":\"2019-12-26 10:40 +03:00\",  \"scheduledTimeUtc\":\"2019-12-26 07:40Z\", \"terminal\":\"1\" },  \"isCargo\": false,\"number\": \"SU 11\",\"status\": \"Arrived\"}]}";

			using (TextWriter wr = new StreamWriter("./schedule_response.json")) {
				wr.WriteLine(response);
			}

			var schedule = JsonSerializer.Deserialize<ResponseObjects.Schedule>(response, options);

			return schedule;
		}
	}

	#endregion
}
