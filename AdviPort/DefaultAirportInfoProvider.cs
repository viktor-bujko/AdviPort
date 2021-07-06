using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AdviPort.ResponseObjects;
using AdviPort.UI;

namespace AdviPort {

	interface IInfoProvider<TResultTypeDescriptor> {
		Task<TResultTypeDescriptor> GetInformationByICAOAsync<TResultDescriptor>(string airportIcaoCode);
	}

	/// <summary>Provides information about given airport, if such information is available.</summary>
	interface IAirportProvider {
		Task<TAirportDescriptor> GetAirportByICAOAsync<TAirportDescriptor>(string airportIcaoCode) 
			where TAirportDescriptor : IAirport, new();
	}

	/// <summary>Retrieves a flight schedule for given airport.</summary>
	interface IAirportScheduleProvider {
		Task<TSchedule> GetAirportScheduleAsync<TSchedule>(string airportIcaoCode) 
			where TSchedule : ISchedule;
	}

	/// <summary>Gets available information about given airport's runways.</summary>
	interface IRunwayInfoProvider {
		Task<TRunwayInfo[]> GetAirportRunwayInfoAsync<TRunwayInfo>(string airportIcaoCode)
			where TRunwayInfo : IRunway;
	}

	/// <summary>Provides information about a given flight for given date.</summary>
	interface IFlightInfoProvider {
		Task<TFlightInfo[]> GetFlightInfoAsync<TFlightInfo>(string flightNumber, DateTime flightDate) 
			where TFlightInfo : IFlight;
	}

	/// <summary>Class which is responsible for providing information for different plugins from aerodatabox API supplier.</summary>
	class AeroDataBoxProvider : IAirportProvider, IAirportScheduleProvider, IRunwayInfoProvider, IFlightInfoProvider {

		private string RootURI { get; } = @"https://aerodatabox.p.rapidapi.com";

		private JsonSerializerOptions options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };

		/// <summary>Sends a request to an API endpoint defined by the <paramref name="destinationUri"/>.</summary>
		/// <typeparam name="ResponseObjectType">The type of object to which should the JSON response string be deserialized.</typeparam>
		/// <param name="destinationUri">API endpoint uri.</param>
		/// <param name="apiKey">User's identification API key.</param>
		/// <returns>The task which promises the creation of <typeparamref name="ResponseObjectType"/> instance.</returns>
		private async Task<ResponseObjectType> GetApiEndpointResponseMsg<ResponseObjectType>(Uri destinationUri, string apiKey) {
			var client = new HttpClient();

			var request = new HttpRequestMessage {
				Method = HttpMethod.Get,
				RequestUri = destinationUri,
				Headers = {
					{ "x-rapidapi-key", apiKey },
					{ "x-rapidapi-host", "aerodatabox.p.rapidapi.com" }
				},
			};

			var response = await client.SendAsync(request);

			if (!response.IsSuccessStatusCode) {
				Console.Error.WriteLine(response.ReasonPhrase);
				return Task.FromException<ResponseObjectType>(new ArgumentException($"{response.ReasonPhrase} ({(int)response.StatusCode})")).Result;
			}

			var content = await response.Content.ReadAsStringAsync();

			options = new JsonSerializerOptions() {
				PropertyNameCaseInsensitive = true,
				WriteIndented = true
			};

			return JsonSerializer.Deserialize<ResponseObjectType>(content, options);
		}

		/// <summary>Gets information about an airport for currently logged-in user and given airport.</summary>
		/// <typeparam name="TAirportDescriptor">The type of Airport object descriptor.</typeparam>
		/// <param name="aptIcao">The ICAO code of the airport to search for.</param>
		/// <returns>The task which promises to return an instance of airport descriptor.</returns>
		public Task<TAirportDescriptor> GetAirportByICAOAsync<TAirportDescriptor>(string aptIcao) 
			where TAirportDescriptor : IAirport, new() {

			if (! Session.ActiveSession.HasLoggedUser) throw new ArgumentNullException("A user has to be logged in order to use this method.");

			var loggedUser = Session.ActiveSession.LoggedUser;

			if (loggedUser.FavouriteAirports.ContainsKey(aptIcao.ToLower())) {
				// Avoiding sending a request from already downloaded data.
				return Task.Run(() => (TAirportDescriptor)(IAirport)loggedUser.FavouriteAirports[aptIcao]);
			}

			string apiKey = Encryptor.Decrypt(loggedUser.APIKey);

			var uri = new Uri($"{RootURI}/airports/icao/{aptIcao.ToLower()}");

			var airportObject = GetApiEndpointResponseMsg<TAirportDescriptor>(uri, apiKey);

			return airportObject;
		}

		/// <summary>Gets information about the flights schedule for currently logged-in user and given airport.</summary>
		/// <typeparam name="TSchedule">The type of Schedule object descriptor.</typeparam>
		/// <param name="aptIcaoCode">The ICAO code of the airport to get the schedule of flights for.</param>
		/// <returns>The task which promises to return an instance of schedule descriptor.</returns>
		public Task<TSchedule> GetAirportScheduleAsync<TSchedule>(string aptIcaoCode) 
			where TSchedule : ISchedule {

			if (! Session.ActiveSession.HasLoggedUser) throw new ArgumentNullException("A user has to be logged in order to use this method.");

			var loggedUser = Session.ActiveSession.LoggedUser;
			string apiKey = Encryptor.Decrypt(loggedUser.APIKey);
            DateTime localNow = DateTime.Now;

			var uri = new Uri($"{ RootURI }/flights/airports/icao/{ aptIcaoCode }/{ localNow.AddMinutes(-15).ToString("yyyy-MM-ddTHH:mm") }/{ localNow.AddHours(4).ToString("yyyy-MM-ddTHH:mm") }?withLeg=false&direction=Both&withCancelled=true&withCodeshared=true");

			var schedule = GetApiEndpointResponseMsg<TSchedule>(uri, apiKey);

			return schedule;
		}

		/// <summary>Gets information about the runways for currently logged-in user and given airport.</summary>
		/// <typeparam name="TRunwayInfo">The type of runway object descriptor.</typeparam>
		/// <param name="aptIcaoCode">The ICAO code of the airport to get the runway information for.</param>
		/// <returns>The task which promises to return an instance of runways information descriptor.</returns>
		public Task<TRunwayInfo[]> GetAirportRunwayInfoAsync<TRunwayInfo>(string aptIcaoCode) 
			where TRunwayInfo : IRunway {

			if (!Session.ActiveSession.HasLoggedUser) throw new ArgumentNullException("A user has to be logged in order to use this method.");

			var loggedUser = Session.ActiveSession.LoggedUser;
			string apiKey = Encryptor.Decrypt(loggedUser.APIKey);

			var uri = new Uri($"{RootURI}/airports/icao/{aptIcaoCode}/runways");

			var runways = GetApiEndpointResponseMsg<TRunwayInfo[]>(uri, apiKey);

			return runways;
		}

		/// <summary>Get information about a given flight and currently logged-in user.</summary>
		/// <typeparam name="TFlightInfo">The type of flight information object descriptor.</typeparam>
		/// <param name="flightNumber">The flight number to search for.</param>
		/// <param name="flightDate">Day for which the flight information should be retrieved.</param>
		/// <returns>The task which promises to return an instance of flight information descriptor.</returns>
		public Task<TFlightInfo[]> GetFlightInfoAsync<TFlightInfo>(string flightNumber, DateTime flightDate) 
			where TFlightInfo : IFlight {

			if (!Session.ActiveSession.HasLoggedUser) throw new ArgumentNullException("A user has to be logged in order to use this method.");

			var loggedUser = Session.ActiveSession.LoggedUser;
			string apiKey = Encryptor.Decrypt(loggedUser.APIKey);

			var uri = new Uri($"{RootURI}/flights/number/{flightNumber.ToUpper()}/{flightDate.Year}-{flightDate.Month.ToString().PadLeft(2, '0')}-{flightDate.Day.ToString().PadLeft(2, '0')}");
			
			var flightInfos = GetApiEndpointResponseMsg<TFlightInfo[]>(uri, apiKey);

			return flightInfos;
		}
	}
}
