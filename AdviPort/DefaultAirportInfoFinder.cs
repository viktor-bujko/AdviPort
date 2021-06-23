using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AdviPort.ResponseObjects;

namespace AdviPort {

	interface IAirportProvider {
		Task<TAirportDescriptor> GetAirportByICAOAsync<TAirportDescriptor>(string airportIcaoCode) 
			where TAirportDescriptor : Airport, new();
	}

	interface IAirportScheduleProvider {
		Task<TSchedule> GetAirportScheduleAsync<TSchedule>(string airportIcaoCode) 
			where TSchedule : Schedule;
	}

	interface IRunwayInfoProvider {
		Task<TRunwayInfo[]> GetAirportRunwayInfoAsync<TRunwayInfo>(string airportIcaoCode)
			where TRunwayInfo : Runway;
	}

	interface IFlightInfoProvider {
		Task<TFlightInfo[]> GetFlightInfoAsync<TFlightInfo>(string flightCallSign, DateTime flightDate) 
			where TFlightInfo : Flight;
	}

	class AeroDataBoxProvider : IAirportProvider, IAirportScheduleProvider, IRunwayInfoProvider, IFlightInfoProvider {

		private string RootURI { get; } = @"https://aerodatabox.p.rapidahpi.com";

		private readonly JsonSerializerOptions options = new JsonSerializerOptions() {
			PropertyNameCaseInsensitive = true
		};

		private Task<HttpResponseMessage> GetResponseMessageFromServer(Uri destinationUri, string apiKey) {
			var client = new HttpClient();

			var request = new HttpRequestMessage {
				Method = HttpMethod.Get,
				RequestUri = destinationUri,
				Headers = {
					{ "x-rapidapi-key", apiKey },
					{ "x-rapidapi-host", "aerodatabox.p.rapidapi.com" }
				},
			};

			return client.SendAsync(request);
		}

		public async Task<TAirportDescriptor> GetAirportByICAOAsync<TAirportDescriptor>(string aptIcao) 
			where TAirportDescriptor : Airport, new() {

			if (! Session.ActiveSession.HasLoggedUser)
				throw new ArgumentNullException("A user has to be logged in order to use this method.");

			var loggedUser = Session.ActiveSession.LoggedUser;

			if (loggedUser.FavouriteAirports.ContainsKey(aptIcao.ToLower())) {
				// Avoiding sending a request from already downloaded data.
				return (TAirportDescriptor)loggedUser.FavouriteAirports[aptIcao];
			}

			string apiKey = Encryptor.Decrypt(loggedUser.APIKey);

			//var uri = new Uri($"{ RootURI }/airport?icao={ aptIcao.ToLower() }");
			var uri = new Uri($"{RootURI}/airports/icao/{aptIcao.ToLower()}");

			/*var response = await GetResponseMessageFromServer(uri, apiKey);

			if (!response.IsSuccessStatusCode) {
				Console.Error.WriteLine("Try checking you have entered an existing ICAO airport code.");
				Console.Error.WriteLine(response.ReasonPhrase);
				Console.Error.WriteLine(response.StatusCode);
				return new TAirportDescriptor();
			}

			var contentString = await response.Content.ReadAsStringAsync();*/

			Console.WriteLine($"Got successful response for airport \"{ aptIcao }\"");

			//var body = "{\"id\":1213,\"iata\":\"CDG\",\"icao\":\"LFPG\",\"name\":\"Charles de Gaulle Airport (Roissy Airport)\",\"location\":\"Paris, Île-de-France, France\",\"street_number\":\"\",\"street\":\"\",\"city\":\"Roissy-en-France\",\"county\":\"\",\"state\":\"Île-de-France\",\"country_iso\":\"FR\",\"country\":\"France\",\"postal_code\":\"95700\",\"phone\":\"+33 1 70 36 39 50\",\"latitude\":49.00969,\"longitude\":2.5479245,\"uct\":120,\"website\":\"http://www.parisaeroport.fr/\"}\n";
			var contentString = "{\"icao\": \"EDDF\", \"iata\": \"FRA\", \"shortName\": \"Frankfurt-am-Main\", \"fullName\": \"Frankfurt-am-Main\", \"municipalityName\": \"Frankfurt-am-Main\", \"location\": { \"lat\": 50.0264, \"lon\": 8.543129}, \"country\": { \"code\": \"DE\", \"name\": \"Germany\"}, \"continent\": { \"code\": \"EU\", \"name\": \"Europe\" }, \"timeZone\": \"Europe/Berlin\", \"urls\": { \"WebSite\": \"http://www.frankfurt-airport.de/ \", \"Wikipedia\": \"https://en.wikipedia.org/wiki/Frankfurt_Airport \", \"Twitter\": \"http://twitter.com/Airport_FRA \", \"GoogleMaps\": \"https://www.google.com/maps/@50.026401,8.543129,14z \", \"FlightRadar\": \"https://www.flightradar24.com/50.03,8.54/14 \" } }";

			using (TextWriter wr = new StreamWriter("./airport_response.json")) {
				wr.WriteLine(contentString);
			}

			var airportObject = JsonSerializer.Deserialize<TAirportDescriptor>(contentString, options);

			return airportObject;
		}

		public async Task<TSchedule> GetAirportScheduleAsync<TSchedule>(string airportIcaoCode) 
			where TSchedule : Schedule {

			if (! Session.ActiveSession.HasLoggedUser) {
				throw new ArgumentNullException("A user has to be logged in order to use this method.");
			}

			var loggedUser = Session.ActiveSession.LoggedUser;
			string apiKey = Encryptor.Decrypt(loggedUser.APIKey);
			DateTime utcNow = DateTime.UtcNow;

			var uri = new Uri($"{ RootURI }/flights/airports/icao/{ airportIcaoCode }/{ utcNow.ToString("yyyy-MM-ddTHH:mm") }/{ utcNow.AddHours(3).ToString("yyyy-MM-ddTHH:mm") }?withLeg=false&direction=Both&withCancelled=true&withCodeshared=true");

			/*var response = await GetResponseMessageFromServer(uri, apiKey);

			if (!response.IsSuccessStatusCode) {
				Console.Error.WriteLine("Try checking you have entered an existing ICAO airport code.");
				Console.Error.WriteLine(response.ReasonPhrase);
				return null;
			}

			var contentString = await response.Content.ReadAsStringAsync();*/

			var contentString = "{\"arrivals\": [{\"aircraft\": {\"model\": \"Airbus A320\", \"reg\": \"VP-BJW\" }, \"airline\": { \"name\": \"Aeroflot\" }, \"arrival\": { \"actualTimeLocal\": \"2019-12-26 12:06+03:00\", \"actualTimeUtc\": \"2019-12-26 09:06Z\",\"baggageBelt\": \"6\", \"runwayTimeLocal\": \"2019-12-26 11:54+03:00\",\"runwayTimeUtc\": \"2019-12-26 08:54Z\",\"scheduledTimeLocal\": \"2019-12-26 12:00+03:00\",\"scheduledTimeUtc\": \"2019-12-26 09:00Z\",\"terminal\": \"B\"},\"callSign\": \"AFL011\",\"codeshareStatus\": \"IsOperator\", \"departure\": {\"actualTimeLocal\":\"2019-12-26 10:34+03:00\", \"actualTimeUtc\":\"2019-12-26 07:34Z\", \"airport\": {\"iata\":\"LED\", \"icao\":\"ULLI\", \"name\":\"Saint-Petersburg\" }, \"checkInDesk\":\"201-206\", \"gate\":\"D02\", \"runwayTimeLocal\":\"2019-12-26 10:54 +03:00\", \"runwayTimeUtc\":\"2019-12-26 07:54Z\", \"scheduledTimeLocal\":\"2019-12-26 10:40 +03:00\",  \"scheduledTimeUtc\":\"2019-12-26 07:40Z\", \"terminal\":\"1\" },  \"isCargo\": false,\"number\": \"SU 11\",\"status\": \"Arrived\"}, {\"aircraft\": {\"model\": \"Airbus A320\", \"reg\": \"VP-BJW\" }, \"airline\": { \"name\": \"Aeroflot\" }, \"arrival\": { \"actualTimeLocal\": \"2019-12-26 12:06+03:00\", \"actualTimeUtc\": \"2019-12-26 09:06Z\",\"baggageBelt\": \"6\", \"runwayTimeLocal\": \"2019-12-26 11:54+03:00\",\"runwayTimeUtc\": \"2019-12-26 08:54Z\",\"scheduledTimeLocal\": \"2019-12-26 12:00+03:00\",\"scheduledTimeUtc\": \"2019-12-26 09:00Z\",\"terminal\": \"B\"},\"callSign\": \"AFL011\",\"codeshareStatus\": \"IsOperator\", \"departure\": {\"actualTimeLocal\":\"2019-12-26 10:34+03:00\", \"actualTimeUtc\":\"2019-12-26 07:34Z\", \"airport\": {\"iata\":\"LED\", \"icao\":\"ULLI\", \"name\":\"Saint-Petersburg\" }, \"checkInDesk\":\"201-206\", \"gate\":\"D02\", \"runwayTimeLocal\":\"2019-12-26 10:54 +03:00\", \"runwayTimeUtc\":\"2019-12-26 07:54Z\", \"scheduledTimeLocal\":\"2019-12-26 10:40 +03:00\",  \"scheduledTimeUtc\":\"2019-12-26 07:40Z\", \"terminal\":\"1\" },  \"isCargo\": false,\"number\": \"SU 11\",\"status\": \"Arrived\"}]}";

			using (TextWriter wr = new StreamWriter("./schedule_response.json")) {
				wr.WriteLine(contentString);
			}

			var schedule = JsonSerializer.Deserialize<TSchedule>(contentString, options);

			return schedule;
		}

		public async Task<TRunwayInfo[]> GetAirportRunwayInfoAsync<TRunwayInfo>(string airportIcaoCode) 
			where TRunwayInfo : Runway {

			if (!Session.ActiveSession.HasLoggedUser) {
				throw new ArgumentNullException("A user has to be logged in order to use this method.");
			}

			var loggedUser = Session.ActiveSession.LoggedUser;
			string apiKey = Encryptor.Decrypt(loggedUser.APIKey);

			var uri = new Uri($"{RootURI}/airports/icao/{airportIcaoCode}/runways");

			/*var response = await GetResponseMessageFromServer(uri, apiKey);

			if (!response.IsSuccessStatusCode) {
				Console.Error.WriteLine("An error occured while trying to get runway information.");
				return null;
			}

			var contentString = await response.Content.ReadAsStringAsync();*/

			var contentString = "[{\"name\":\"04\",\"trueHdg\":41.1,\"length\":{\"meter\":2021,\"km\":2.021,\"mile\":1.256,\"nm\":1.091,\"feet\":663},\"width\":{\"meter\":44,\"km\":0.044,\"mile\":0.027,\"nm\":0.024,\"feet\":144},\"isClosed\":false,\"location\":{\"lat\":52.3003578,\"lon\":4.783485},\"surface\":\"Asphalt\",\"displacedThreshold\":{\"meter\":0,\"km\":0,\"mile\":0,\"nm\":0,\"feet\":0},\"hasLighting\":true},{\"name\":\"06\",\"trueHdg\":57.9,\"length\":{\"meter\":3438,\"km\":3.438,\"mile\":2.136,\"nm\":1.856,\"feet\":11279},\"width\":{\"meter\":44,\"km\":0.044,\"mile\":0.027,\"nm\":0.024,\"feet\":144},\"isClosed\":false,\"location\":{\"lat\":52.28792,\"lon\":4.73415327},\"surface\":\"Asphalt\",\"displacedThreshold\":{\"meter\":249,\"km\":0.249,\"mile\":0.155,\"nm\":0.134,\"feet\":816},\"hasLighting\":true},{\"name\":\"09\",\"trueHdg\":86.8,\"length\":{\"meter\":3445,\"km\":3.445,\"mile\":2.141,\"nm\":1.86,\"feet\":11302},\"width\":{\"meter\":44,\"km\":0.044,\"mile\":0.027,\"nm\":0.024,\"feet\":144},\"isClosed\":false,\"location\":{\"lat\":52.3166275,\"lon\":4.74632835},\"surface\":\"Asphalt\",\"displacedThreshold\":{\"meter\":88,\"km\":0.088,\"mile\":0.055,\"nm\":0.048,\"feet\":288},\"hasLighting\":true},{\"name\":\"18C\",\"trueHdg\":183.2,\"length\":{\"meter\":3301,\"km\":3.301,\"mile\":2.051,\"nm\":1.782,\"feet\":10830},\"width\":{\"meter\":44,\"km\":0.044,\"mile\":0.027,\"nm\":0.024,\"feet\":144},\"isClosed\":false,\"location\":{\"lat\":52.3313942,\"lon\":4.740041},\"surface\":\"Asphalt\",\"displacedThreshold\":{\"meter\":0,\"km\":0,\"mile\":0,\"nm\":0,\"feet\":0},\"hasLighting\":true},{\"name\":\"18L\",\"trueHdg\":183.2,\"length\":{\"meter\":3397,\"km\":3.397,\"mile\":2.111,\"nm\":1.834,\"feet\":11145},\"width\":{\"meter\":44,\"km\":0.044,\"mile\":0.027,\"nm\":0.024,\"feet\":144},\"isClosed\":false,\"location\":{\"lat\":52.3212929,\"lon\":4.78016043},\"surface\":\"Asphalt\",\"displacedThreshold\":{\"meter\":573,\"km\":0.573,\"mile\":0.356,\"nm\":0.309,\"feet\":1879},\"hasLighting\":true},{\"name\":\"18R\",\"trueHdg\":183.2,\"length\":{\"meter\":3799,\"km\":3.799,\"mile\":2.361,\"nm\":2.051,\"feet\":12463},\"width\":{\"meter\":59,\"km\":0.059,\"mile\":0.037,\"nm\":0.032,\"feet\":193},\"isClosed\":false,\"location\":{\"lat\":52.3626671,\"lon\":4.71195555},\"surface\":\"Asphalt\",\"displacedThreshold\":{\"meter\":269,\"km\":0.269,\"mile\":0.167,\"nm\":0.145,\"feet\":882},\"hasLighting\":true},{\"name\":\"22\",\"trueHdg\":221.1,\"length\":{\"meter\":2021,\"km\":2.021,\"mile\":1.256,\"nm\":1.091,\"feet\":6630},\"width\":{\"meter\":44,\"km\":0.044,\"mile\":0.027,\"nm\":0.024,\"feet\":144},\"isClosed\":false,\"location\":{\"lat\":52.31404,\"lon\":4.8030405},\"surface\":\"Asphalt\",\"displacedThreshold\":{\"meter\":0,\"km\":0,\"mile\":0,\"nm\":0,\"feet\":0},\"hasLighting\":true},{\"name\":\"24\",\"trueHdg\":237.9,\"length\":{\"meter\":3438,\"km\":3.438,\"mile\":2.136,\"nm\":1.856,\"feet\":11279},\"width\":{\"meter\":44,\"km\":0.044,\"mile\":0.027,\"nm\":0.024,\"feet\":144},\"isClosed\":false,\"location\":{\"lat\":52.3043556,\"lon\":4.776933},\"surface\":\"Asphalt\",\"displacedThreshold\":{\"meter\":0,\"km\":0,\"mile\":0,\"nm\":0,\"feet\":0},\"hasLighting\":true},{\"name\":\"27\",\"trueHdg\":266.8,\"length\":{\"meter\":3445,\"km\":3.445,\"mile\":2.141,\"nm\":1.86,\"feet\":11302},\"width\":{\"meter\":44,\"km\":0.044,\"mile\":0.027,\"nm\":0.024,\"feet\":144},\"isClosed\":false,\"location\":{\"lat\":52.31837,\"lon\":4.79690361},\"surface\":\"Asphalt\",\"displacedThreshold\":{\"meter\":0,\"km\":0,\"mile\":0,\"nm\":0,\"feet\":0},\"hasLighting\":true},{\"name\":\"36C\",\"trueHdg\":3.2,\"length\":{\"meter\":3301,\"km\":3.301,\"mile\":2.051,\"nm\":1.782,\"feet\":10830},\"width\":{\"meter\":44,\"km\":0.044,\"mile\":0.027,\"nm\":0.024,\"feet\":144},\"isClosed\":false,\"location\":{\"lat\":52.3017769,\"lon\":4.737321},\"surface\":\"Asphalt\",\"displacedThreshold\":{\"meter\":449,\"km\":0.449,\"mile\":0.279,\"nm\":0.242,\"feet\":1473},\"hasLighting\":true},{\"name\":\"36L\",\"trueHdg\":3.2,\"length\":{\"meter\":3799,\"km\":3.799,\"mile\":2.361,\"nm\":2.051,\"feet\":12463},\"width\":{\"meter\":59,\"km\":0.059,\"mile\":0.037,\"nm\":0.032,\"feet\":193},\"isClosed\":false,\"location\":{\"lat\":52.3285751,\"lon\":4.70885},\"surface\":\"Asphalt\",\"displacedThreshold\":{\"meter\":0,\"km\":0,\"mile\":0,\"nm\":0,\"feet\":0},\"hasLighting\":true},{\"name\":\"36R\",\"trueHdg\":3.2,\"length\":{\"meter\":3397,\"km\":3.397,\"mile\":2.111,\"nm\":1.834,\"feet\":11145},\"width\":{\"meter\":44,\"km\":0.044,\"mile\":0.027,\"nm\":0.024,\"feet\":144},\"isClosed\":false,\"location\":{\"lat\":52.29081,\"lon\":4.77734375},\"surface\":\"Asphalt\",\"displacedThreshold\":{\"meter\":0,\"km\":0,\"mile\":0,\"nm\":0,\"feet\":0},\"hasLighting\":true}]";


			var runways = JsonSerializer.Deserialize<TRunwayInfo[]>(contentString, options);

			return runways;
		}

		public async Task<TFlightInfo[]> GetFlightInfoAsync<TFlightInfo>(string flightCallSign, DateTime flightDate) 
			where TFlightInfo : Flight {

			if (!Session.ActiveSession.HasLoggedUser) {
				throw new ArgumentNullException("A user has to be logged in order to use this method.");
			}

			var loggedUser = Session.ActiveSession.LoggedUser;
			string apiKey = Encryptor.Decrypt(loggedUser.APIKey);

			var uri = new Uri($"{RootURI}/flights/callsign/{flightCallSign}/{flightDate.Year}-{flightDate.Month}-{flightDate.Date}");

			/*var response = await GetResponseMessageFromServer(uri, apiKey);

			if (!response.IsSuccessStatusCode) {
				Console.Error.WriteLine("An error occured while trying to get runway information.");
				return null;
			}

			var contentString = await response.Content.ReadAsStringAsync();*/
			var contentString = "[{\"aircraft\": {\"model\": \"Airbus A320\", \"reg\": \"VP-BJW\" }, \"airline\": { \"name\": \"Aeroflot\" }, \"arrival\": { \"actualTimeLocal\": \"2019-12-26 12:06+03:00\", \"actualTimeUtc\": \"2019-12-26 09:06Z\",\"baggageBelt\": \"6\", \"runwayTimeLocal\": \"2019-12-26 11:54+03:00\",\"runwayTimeUtc\": \"2019-12-26 08:54Z\",\"scheduledTimeLocal\": \"2019-12-26 12:00+03:00\",\"scheduledTimeUtc\": \"2019-12-26 09:00Z\",\"terminal\": \"B\"},\"callSign\": \"AFL011\",\"codeshareStatus\": \"IsOperator\", \"departure\": {\"actualTimeLocal\":\"2019-12-26 10:34+03:00\", \"actualTimeUtc\":\"2019-12-26 07:34Z\", \"airport\": {\"iata\":\"LED\", \"icao\":\"ULLI\", \"name\":\"Saint-Petersburg\" }, \"checkInDesk\":\"201-206\", \"gate\":\"D02\", \"runwayTimeLocal\":\"2019-12-26 10:54 +03:00\", \"runwayTimeUtc\":\"2019-12-26 07:54Z\", \"scheduledTimeLocal\":\"2019-12-26 10:40 +03:00\",  \"scheduledTimeUtc\":\"2019-12-26 07:40Z\", \"terminal\":\"1\" },  \"isCargo\": false,\"number\": \"SU 11\",\"status\": \"Arrived\"}]";

			var flightInfos = JsonSerializer.Deserialize<TFlightInfo[]>(contentString, options);

			return flightInfos;
		}
	}
}
