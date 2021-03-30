using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;

namespace AdviPort {

	interface IAirportFinder {
		void SetFavouriteAirportByICAO(string airportIcaoCode, IUserProfileWriter profileWriter);
	}

	interface IProvider {
		string RootURI { get;}
	}

	#region AviationStack
	abstract class AviationStack : IProvider {
		public string RootURI { get; } = @"https://api.aviationstack.com/v1/";
	}

	class AviationStackAirportInfoFinder : IAirportFinder {
		protected string APIEndpoint { get; } = "airports";
		public void SetFavouriteAirportByICAO(string airportIcaoCode, IUserProfileWriter profileWriter) {
			var loggedUser = Session.ActiveSession.LoggedUser;

			if (loggedUser == null) {
				Console.Error.WriteLine("You have to log in first before using application services.");
				throw new ArgumentNullException();
			}
		}
	}

	class AviationStackAirportREST {
		public string Airport_Name { get; }
		public string IATA_Code { get; }
		public string ICAO_Code { get; }
		public double Latitude { get; }
		public double Longitude { get; }
		public string Geoname_ID { get; }
		public string Timezone { get; }
		public int GMT { get; }
		public string Phone_number { get; }
		public string Country_Name { get; }
		public string Country_ISO2 { get; }
		public string City_IATA_Code { get; }

		public AviationStackAirportREST() { }

		public AviationStackAirportREST(string airport_name, string iata_code, string icao_code, double latitude, double longitude, string geoname_id, string timezone, int gmt, string phone_number, string country_name, string country_iso2, string city_iata_code) {
			Airport_Name = airport_name;
			IATA_Code = iata_code;
			ICAO_Code = icao_code;
			Latitude = latitude;
			Longitude = longitude;
			Geoname_ID = geoname_id;
			Timezone = timezone;
			GMT = gmt;
			Phone_number = phone_number;
			Country_Name = country_name;
			Country_ISO2 = country_iso2;
			City_IATA_Code = city_iata_code;
		}
	}

	class AviationStackAirlineREST {

		public string Airline_Name { get; }
		public string IATA_Code { get; }
		public string ICAO_Code { get; }
		public string Callsign { get; }
		public string Type { get; }
		public string Status { get; }
		public int Fleet_size { get; }
		public int Fleet_average_age { get; }
		public int DateFounded { get; }
		public string Hub_Code { get; }
		public string Country_Name { get; }
		public string Coutry_ISO2 { get; }
	}

	#endregion

	#region RapidAPI

	abstract class RapidAPI : IProvider {
		public string RootURI { get; } = @"https://airport-info.p.rapidapi.com";
	}

	class RapidAPIAirportInfoFinder : RapidAPI, IAirportFinder {

		protected string APIEndpoint { get; } = "airport";

		public async void SetFavouriteAirportByICAO(string airportIcaoCode, IUserProfileWriter profileWriter) {
			var loggedUser = Session.ActiveSession.LoggedUser;

			if (loggedUser == null) {
				throw new ArgumentNullException("A user has to be logged in order to use this method.");
			}

			string apiKey = Encryptor.Decrypt(loggedUser.APIKey);

			var uri = new Uri($"{ RootURI }/{ APIEndpoint }?icao={ airportIcaoCode.ToLower() }");

			/*var client = new HttpClient();
			var request = new HttpRequestMessage {
				Method = HttpMethod.Get,
				RequestUri = uri,
				Headers = {
					{ "x-rapidapi-key", apiKey },
					{ "x-rapidapi-host", "airport-info.p.rapidapi.com" }
				},
			};

			using var response = await client.SendAsync(request);

			if (! response.IsSuccessStatusCode) {
				Console.Error.WriteLine("Selected airport could not be added to favourite airports. Try checking you have entered an existing ICAO airport code.");
				Console.Error.WriteLine(response.ReasonPhrase);
				return;
			}*/

			Console.WriteLine($"Got successful response for airport \"{ airportIcaoCode }\"");
			//var body = response.Content.ReadAsStringAsync(); 
			var body = "{\"id\":1213,\"iata\":\"CDG\",\"icao\":\"LFPG\",\"name\":\"Charles de Gaulle Airport (Roissy Airport)\",\"location\":\"Paris, Île-de-France, France\",\"street_number\":\"\",\"street\":\"\",\"city\":\"Roissy-en-France\",\"county\":\"\",\"state\":\"Île-de-France\",\"country_iso\":\"FR\",\"country\":\"France\",\"postal_code\":\"95700\",\"phone\":\"+33 1 70 36 39 50\",\"latitude\":49.00969,\"longitude\":2.5479245,\"uct\":120,\"website\":\"http://www.parisaeroport.fr/\"}\n";
			body = "{\"icao\": \"EDDF\", \"iata\": \"FRA\", \"shortName\": \"Frankfurt-am-Main\", \"fullName\": \"Frankfurt-am-Main\", \"municipalityName\": \"Frankfurt-am-Main\", \"location\": { \"lat\": 50.0264, \"lon\": 8.543129}, \"country\": { \"code\": \"DE\", \"name\": \"Germany\"}, \"continent\": { \"code\": \"EU\", \"name\": \"Europe\" }, \"timeZone\": \"Europe/Berlin\", \"urls\": { \"WebSite\": \"http://www.frankfurt-airport.de/ \", \"Wikipedia\": \"https://en.wikipedia.org/wiki/Frankfurt_Airport \", \"Twitter\": \"http://twitter.com/Airport_FRA \", \"GoogleMaps\": \"https://www.google.com/maps/@50.026401,8.543129,14z \", \"FlightRadar\": \"https://www.flightradar24.com/50.03,8.54/14 \" } }";
			var airportObject = JsonSerializer.Deserialize<ResponseObjects.Airport>(body, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

			loggedUser.FavouriteAirports.Add(airportObject.ICAO.ToLower() + "a", airportObject);

			profileWriter.WriteUserProfile(loggedUser);
		}
	}

	#endregion
}
