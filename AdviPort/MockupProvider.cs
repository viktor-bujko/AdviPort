using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AdviPort.ResponseObjects;


namespace AdviPort {
    class MockupInfoProvider : IAirportProvider, IAirportScheduleProvider, IRunwayInfoProvider, IFlightInfoProvider {

        private JsonSerializerOptions options { get; } = new JsonSerializerOptions() {
            PropertyNameCaseInsensitive = true
        };

        public Task<TAirportDescriptor> GetAirportByICAOAsync<TAirportDescriptor>(string aptIcaoCode)
            where TAirportDescriptor : IAirport, new() {
            var contentString =
                @"{
					""icao"": ""EDDF"", 
					""iata"": ""FRA"", 
					""shortName"": ""Frankfurt-am-Main"", 
					""fullName"": ""Frankfurt-am-Main"", 
					""municipalityName"": ""Frankfurt-am-Main"", 
					""location"": { 
						""lat"": 50.0264, 
						""lon"": 8.543129
					}, 
					""country"": { 
						""code"": ""DE"", 
						""name"": ""Germany""
					}, 
					""continent"": { 
						""code"": ""EU"", 
						""name"": ""Europe"" 
					}, 
					""timeZone"": ""Europe/Berlin"",
					""urls"": { 
						""WebSite"": ""http://www.frankfurt-airport.de/ "", 
						""Wikipedia"": ""https://en.wikipedia.org/wiki/Frankfurt_Airport "", 
						""Twitter"": ""http://twitter.com/Airport_FRA "", 
						""GoogleMaps"": ""https://www.google.com/maps/@50.026401,8.543129,14z "", 
						""FlightRadar"": ""https://www.flightradar24.com/50.03,8.54/14 "" 
					} 
				}";

            var airportObject = JsonSerializer.Deserialize<TAirportDescriptor>(contentString, options);
            return Task.Run(() => airportObject);
        }

        public Task<TRunwayInfo[]> GetAirportRunwayInfoAsync<TRunwayInfo>(string aptIcaoCode)
            where TRunwayInfo : IRunway {

            var contentString =
                    @"[
						{ 
							""name"":""04"",
							""trueHdg"":41.1,
							""length"": {
								""meter"":2021,
								""km"":2.021,
								""mile"":1.256,
								""nm"":1.091,
								""feet"":663
							},
							""width"": {
								""km"":0.044,
								""mile"":0.027,
								""meter"":44,
								""nm"":0.024,
								""feet"":144
							},
							""isClosed"":false,
							""location"": {
								""lat"":52.3003578,
								""lon"":4.783485
							},
							""surface"":""Asphalt"",
							""displacedThreshold"": {
								""meter"":0,
								""km"":0,
								""mile"":0,
								""nm"":0,
								""feet"":0
							},
							""hasLighting"":true
						},
						{
							""name"":""06"",
							""trueHdg"":57.9,
							""length"":{
								""meter"":3438,
								""km"":3.438,
								""mile"":2.136,
								""nm"":1.856,
								""feet"":11279
							},
							""width"":{
								""meter"":44,
								""km"":0.044,
								""mile"":0.027,
								""nm"":0.024,
								""feet"":144
							},
							""isClosed"":false,
							""location"":{
								""lat"":52.28792,
								""lon"":4.73415327
							},
							""surface"":""Asphalt"",
							""displacedThreshold"":{
								""meter"":249,
								""km"":0.249,
								""mile"":0.155,
								""nm"":0.134,
								""feet"":816
							},
							""hasLighting"":true
						},
						{
							""name"":""09"",
							""trueHdg"":86.8,
							""length"":{
								""meter"":3445,
								""km"":3.445,
								""mile"":2.141,
								""nm"":1.86,
								""feet"":11302
							},
							""width"":{
								""meter"":44,
								""km"":0.044,
								""mile"":0.027,
								""nm"":0.024,
								""feet"":144
							},
							""isClosed"":false,
							""location"":{
								""lat"":52.3166275,
								""lon"":4.74632835
							},
							""surface"":""Asphalt"",
							""displacedThreshold"":{
								""meter"":88,
								""km"":0.088,
								""mile"":0.055,
								""nm"":0.048,
								""feet"":288
							},
							""hasLighting"":true
						},
						{
							""name"":""18C"",
							""trueHdg"":183.2,
							""length"":{
								""meter"":3301,
								""km"":3.301,
								""mile"":2.051,
								""nm"":1.782,
								""feet"":10830
							},
							""width"":{
								""meter"":44,
								""km"":0.044,
								""mile"":0.027,
								""nm"":0.024,
								""feet"":144
							},
							""isClosed"":false,
							""location"":{
								""lat"":52.3313942,
								""lon"":4.740041
							},
							""surface"":""Asphalt"",
							""displacedThreshold"":{
								""meter"":0,
								""km"":0,
								""mile"":0,
								""nm"":0,
								""feet"":0
							},
							""hasLighting"":true
						},
						{
							""name"":""18L"",
							""trueHdg"":183.2,
							""length"":{
								""meter"":3397,
								""km"":3.397,
								""mile"":2.111,
								""nm"":1.834,
								""feet"":11145
							},
							""width"":{
								""meter"":44,
								""km"":0.044,
								""mile"":0.027,
								""nm"":0.024,
								""feet"":144
							},
							""isClosed"":false,
							""location"":{
								""lat"":52.3212929,
								""lon"":4.78016043
							},
							""surface"":""Asphalt"",
							""displacedThreshold"":{
								""meter"":573,
								""km"":0.573,
								""mile"":0.356,
								""nm"":0.309,
								""feet"":1879
							},
							""hasLighting"":true
						}]";

            var runwaysObject = JsonSerializer.Deserialize<TRunwayInfo[]>(contentString, options);

            return Task.Run(() => runwaysObject);
        }

        public Task<TSchedule> GetAirportScheduleAsync<TSchedule>(string aptIcaoCode)
            where TSchedule : ISchedule {

            var contentString =
                @"{
    ""departures"": [
        {
            ""movement"": {
                ""airport"": {
                    ""name"": ""Unknown""
                },
                ""scheduledTimeLocal"": ""2021-06-30 23:18+02:00"",
                ""actualTimeLocal"": ""2021-06-30 23:18+02:00"",
                ""runwayTimeLocal"": ""2021-06-30 23:19+02:00"",
                ""scheduledTimeUtc"": ""2021-06-30 21:18Z"",
                ""actualTimeUtc"": ""2021-06-30 21:18Z"",
                ""runwayTimeUtc"": ""2021-06-30 21:19Z"",
                ""runway"": ""24"",
                ""quality"": [
                    ""Basic"",
                    ""Live""
                ]
            },
            ""number"": ""5X 295"",
            ""callSign"": ""UPS295"",
            ""status"": ""Departed"",
            ""codeshareStatus"": ""IsOperator"",
            ""isCargo"": true,
            ""aircraft"": {
                ""reg"": ""N428UP"",
                ""modeS"": ""A51B27"",
                ""model"": ""Boeing 757-200 Freighter""
            },
            ""airline"": {
                    ""name"": ""UPS""
            }
        },
        {
                ""movement"": {
                    ""airport"": {
                        ""icao"": ""HEGN"",
                        ""iata"": ""HRG"",
                        ""name"": ""Hurghada""
                    },
                    ""scheduledTimeLocal"": ""2021-07-01 00:05+02:00"",
                    ""actualTimeLocal"": ""2021-07-01 00:05+02:00"",
                    ""scheduledTimeUtc"": ""2021-06-30 22:05Z"",
                    ""actualTimeUtc"": ""2021-06-30 22:05Z"",
                    ""terminal"": ""T1"",
                    ""gate"": ""B7"",
                    ""quality"": [
                        ""Basic"",
                        ""Live""
                    ]
                },
                ""number"": ""QS 2532"",
                ""status"": ""Boarding"",
                ""codeshareStatus"": ""Unknown"",
                ""isCargo"": false,
                ""airline"": {
                    ""name"": ""SmartWings""
                }
        },
        {
                ""movement"": {
                    ""airport"": {
                        ""icao"": ""HEMA"",
                    ""iata"": ""RMF"",
                    ""name"": ""Marsa Alam""
                    },
                ""scheduledTimeLocal"": ""2021-07-01 00:05+02:00"",
                ""actualTimeLocal"": ""2021-07-01 00:05+02:00"",
                ""scheduledTimeUtc"": ""2021-06-30 22:05Z"",
                ""actualTimeUtc"": ""2021-06-30 22:05Z"",
                ""terminal"": ""T1"",
                ""gate"": ""A4"",
                ""quality"": [
                    ""Basic"",
                    ""Live""
                    ]
            },
            ""number"": ""QS 2534"",
            ""status"": ""Boarding"",
            ""codeshareStatus"": ""Unknown"",
            ""isCargo"": false,
            ""airline"": {
                    ""name"": ""SmartWings""
            }
            },
        {
                ""movement"": {
                    ""airport"": {
                        ""icao"": ""HESH"",
                    ""iata"": ""SSH"",
                    ""name"": ""Sharm el Sheikh""
                    },
                ""scheduledTimeLocal"": ""2021-07-01 00:35+02:00"",
                ""actualTimeLocal"": ""2021-07-01 00:35+02:00"",
                ""scheduledTimeUtc"": ""2021-06-30 22:35Z"",
                ""actualTimeUtc"": ""2021-06-30 22:35Z"",
                ""terminal"": ""T1"",
                ""gate"": ""B4"",
                ""quality"": [
                    ""Basic"",
                    ""Live""
                    ]
            },
            ""number"": ""QS 2578"",
            ""status"": ""Boarding"",
            ""codeshareStatus"": ""Unknown"",
            ""isCargo"": false,
            ""airline"": {
                    ""name"": ""SmartWings""
            }
            },
        {
                ""movement"": {
                    ""airport"": {
                        ""icao"": ""HEMA"",
                    ""iata"": ""RMF"",
                    ""name"": ""Marsa Alam""
                    },
                ""scheduledTimeLocal"": ""2021-07-01 00:45+02:00"",
                ""actualTimeLocal"": ""2021-07-01 00:45+02:00"",
                ""scheduledTimeUtc"": ""2021-06-30 22:45Z"",
                ""actualTimeUtc"": ""2021-06-30 22:45Z"",
                ""terminal"": ""T1"",
                ""gate"": ""A3"",
                ""quality"": [
                    ""Basic"",
                    ""Live""
                    ]
            },
            ""number"": ""QS 2588"",
            ""status"": ""Boarding"",
            ""codeshareStatus"": ""Unknown"",
            ""isCargo"": false,
            ""airline"": {
                    ""name"": ""SmartWings""
            }
            },
        {
                ""movement"": {
                    ""airport"": {
                        ""icao"": ""BIKF"",
                    ""iata"": ""KEF"",
                    ""name"": ""Reykjavik""
                    },
                ""scheduledTimeLocal"": ""2021-06-30 22:40+02:00"",
                ""actualTimeLocal"": ""2021-06-30 22:31+02:00"",
                ""runwayTimeLocal"": ""2021-06-30 22:42+02:00"",
                ""scheduledTimeUtc"": ""2021-06-30 20:40Z"",
                ""actualTimeUtc"": ""2021-06-30 20:31Z"",
                ""runwayTimeUtc"": ""2021-06-30 20:42Z"",
                ""terminal"": ""T2"",
                ""gate"": ""C3"",
                ""quality"": [
                    ""Basic"",
                    ""Live""
                    ]
            },
            ""number"": ""OK 460"",
            ""callSign"": ""CSA460"",
            ""status"": ""Departed"",
            ""codeshareStatus"": ""IsOperator"",
            ""isCargo"": false,
            ""aircraft"": {
                    ""reg"": ""OK-REQ"",
                ""modeS"": ""49D027"",
                ""model"": ""Airbus A320""
            },
            ""airline"": {
                    ""name"": ""CSA""
            }
            },
        {
                ""movement"": {
                    ""airport"": {
                        ""icao"": ""HEGN"",
                    ""iata"": ""HRG"",
                    ""name"": ""Hurghada""
                    },
                ""scheduledTimeLocal"": ""2021-07-01 00:50+02:00"",
                ""actualTimeLocal"": ""2021-07-01 00:50+02:00"",
                ""scheduledTimeUtc"": ""2021-06-30 22:50Z"",
                ""actualTimeUtc"": ""2021-06-30 22:50Z"",
                ""terminal"": ""T1"",
                ""gate"": ""B3"",
                ""quality"": [
                    ""Basic"",
                    ""Live""
                    ]
            },
            ""number"": ""QS 1240"",
            ""status"": ""Boarding"",
            ""codeshareStatus"": ""Unknown"",
            ""isCargo"": false,
            ""aircraft"": {
                    ""model"": ""Boeing 737""
            },
            ""airline"": {
                    ""name"": ""SmartWings""
            }
            },
        {
                ""movement"": {
                    ""airport"": {
                        ""icao"": ""LLBG"",
                    ""iata"": ""TLV"",
                    ""name"": ""Tel Aviv Yafo""
                    },
                ""scheduledTimeLocal"": ""2021-06-30 23:00+02:00"",
                ""actualTimeLocal"": ""2021-06-30 23:09+02:00"",
                ""scheduledTimeUtc"": ""2021-06-30 21:00Z"",
                ""actualTimeUtc"": ""2021-06-30 21:09Z"",
                ""terminal"": ""T1"",
                ""gate"": ""A6"",
                ""checkInDesk"": ""201-205"",
                ""quality"": [
                    ""Basic"",
                    ""Live""
                    ]
            },
            ""number"": ""QS 1286"",
            ""status"": ""Departed"",
            ""codeshareStatus"": ""Unknown"",
            ""isCargo"": false,
            ""aircraft"": {
                    ""model"": ""Boeing 737-900""
            },
            ""airline"": {
                    ""name"": ""SmartWings""
            }
            }
    ],
    ""arrivals"": [
        {
                ""movement"": {
                    ""airport"": {
                        ""icao"": ""UKBB"",
                    ""iata"": ""KBP"",
                    ""name"": ""Kiev""
                    },
                ""scheduledTimeLocal"": ""2021-06-30 22:25+02:00"",
                ""actualTimeLocal"": ""2021-06-30 22:36+02:00"",
                ""runwayTimeLocal"": ""2021-06-30 22:32+02:00"",
                ""scheduledTimeUtc"": ""2021-06-30 20:25Z"",
                ""actualTimeUtc"": ""2021-06-30 20:36Z"",
                ""runwayTimeUtc"": ""2021-06-30 20:32Z"",
                ""terminal"": ""T1"",
                ""baggageBelt"": ""12"",
                ""quality"": [
                    ""Basic"",
                    ""Live""
                    ]
            },
            ""number"": ""OK 919"",
            ""callSign"": ""CSA919"",
            ""status"": ""Arrived"",
            ""codeshareStatus"": ""IsOperator"",
            ""isCargo"": false,
            ""aircraft"": {
                    ""reg"": ""OK-HEU"",
                ""modeS"": ""49D357"",
                ""model"": ""Airbus A320""
            },
            ""airline"": {
                    ""name"": ""CSA""
            }
            },
        {
                ""movement"": {
                    ""airport"": {
                        ""icao"": ""GCLP"",
                    ""iata"": ""LPA"",
                    ""name"": ""Gran Canaria Island""
                    },
                ""scheduledTimeLocal"": ""2021-06-30 22:55+02:00"",
                ""actualTimeLocal"": ""2021-06-30 22:42+02:00"",
                ""runwayTimeLocal"": ""2021-06-30 22:38+02:00"",
                ""scheduledTimeUtc"": ""2021-06-30 20:55Z"",
                ""actualTimeUtc"": ""2021-06-30 20:42Z"",
                ""runwayTimeUtc"": ""2021-06-30 20:38Z"",
                ""terminal"": ""T2"",
                ""baggageBelt"": ""21"",
                ""quality"": [
                    ""Basic"",
                    ""Live""
                    ]
            },
            ""number"": ""QS 2041"",
            ""callSign"": ""TVS6X"",
            ""status"": ""Arrived"",
            ""codeshareStatus"": ""IsOperator"",
            ""isCargo"": false,
            ""aircraft"": {
                    ""reg"": ""OK-SWD"",
                ""modeS"": ""49D3D6"",
                ""model"": ""Boeing 737""
            },
            ""airline"": {
                    ""name"": ""SmartWings""
            }
            },
        {
                ""movement"": {
                    ""airport"": {
                        ""name"": ""Unknown""
                    },
                ""scheduledTimeLocal"": ""2021-06-30 22:57+02:00"",
                ""actualTimeLocal"": ""2021-06-30 22:57+02:00"",
                ""runwayTimeLocal"": ""2021-06-30 22:51+02:00"",
                ""scheduledTimeUtc"": ""2021-06-30 20:57Z"",
                ""actualTimeUtc"": ""2021-06-30 20:57Z"",
                ""runwayTimeUtc"": ""2021-06-30 20:51Z"",
                ""quality"": [
                    ""Basic"",
                    ""Live""
                    ]
            },
            ""number"": ""TIE 650U"",
            ""callSign"": ""TIE650U"",
            ""status"": ""Arrived"",
            ""codeshareStatus"": ""IsOperator"",
            ""isCargo"": false,
            ""aircraft"": {
                    ""reg"": ""OK-NTU"",
                ""modeS"": ""49D025"",
                ""model"": ""Beechcraft 400""
            },
            ""airline"": {
                    ""name"": ""TIE""
            }
            },
        {
                ""movement"": {
                    ""airport"": {
                        ""icao"": ""LEPA"",
                    ""iata"": ""PMI"",
                    ""name"": ""Palma De Mallorca""
                    },
                ""scheduledTimeLocal"": ""2021-06-30 23:50+02:00"",
                ""actualTimeLocal"": ""2021-06-30 23:33+02:00"",
                ""scheduledTimeUtc"": ""2021-06-30 21:50Z"",
                ""actualTimeUtc"": ""2021-06-30 21:33Z"",
                ""terminal"": ""T2"",
                ""baggageBelt"": ""24"",
                ""quality"": [
                    ""Basic"",
                    ""Live""
                    ]
            },
            ""number"": ""QS 1165"",
            ""status"": ""Arrived"",
            ""codeshareStatus"": ""Unknown"",
            ""isCargo"": false,
            ""aircraft"": {
                    ""model"": ""Boeing 737-800""
            },
            ""airline"": {
                    ""name"": ""SmartWings""
            }
            },
        {
                ""movement"": {
                    ""airport"": {
                        ""icao"": ""LEMH"",
                    ""iata"": ""MAH"",
                    ""name"": ""Menorca Island""
                    },
                ""scheduledTimeLocal"": ""2021-06-30 23:30+02:00"",
                ""actualTimeLocal"": ""2021-06-30 23:19+02:00"",
                ""scheduledTimeUtc"": ""2021-06-30 21:30Z"",
                ""actualTimeUtc"": ""2021-06-30 21:19Z"",
                ""terminal"": ""T2"",
                ""baggageBelt"": ""24"",
                ""quality"": [
                    ""Basic"",
                    ""Live""
                    ]
            },
            ""number"": ""QS 1169"",
            ""status"": ""Arrived"",
            ""codeshareStatus"": ""Unknown"",
            ""isCargo"": false,
            ""aircraft"": {
                    ""model"": ""Boeing 737""
            },
            ""airline"": {
                    ""name"": ""SmartWings""
            }
            },
        {
                ""movement"": {
                    ""airport"": {
                        ""icao"": ""EHAM"",
                    ""iata"": ""AMS"",
                    ""name"": ""Amsterdam""
                    },
                ""scheduledTimeLocal"": ""2021-06-30 22:30+02:00"",
                ""actualTimeLocal"": ""2021-06-30 22:38+02:00"",
                ""runwayTimeLocal"": ""2021-06-30 22:33+02:00"",
                ""scheduledTimeUtc"": ""2021-06-30 20:30Z"",
                ""actualTimeUtc"": ""2021-06-30 20:38Z"",
                ""runwayTimeUtc"": ""2021-06-30 20:33Z"",
                ""terminal"": ""T2"",
                ""baggageBelt"": ""23"",
                ""quality"": [
                    ""Basic"",
                    ""Live""
                    ]
            },
            ""number"": ""KL 1359"",
            ""callSign"": ""KLM73W"",
            ""status"": ""Arrived"",
            ""codeshareStatus"": ""IsCodeshared"",
            ""isCargo"": false,
            ""aircraft"": {
                    ""reg"": ""PH-EXT"",
                ""modeS"": ""485814"",
                ""model"": ""Embraer 190""
            },
            ""airline"": {
                    ""name"": ""KLM""
            }
            },
        {
                ""movement"": {
                    ""airport"": {
                        ""icao"": ""EDDF"",
                    ""iata"": ""FRA"",
                    ""name"": ""Frankfurt-am-Main""
                    },
                ""scheduledTimeLocal"": ""2021-06-30 22:10+02:00"",
                ""actualTimeLocal"": ""2021-06-30 22:25+02:00"",
                ""runwayTimeLocal"": ""2021-06-30 22:22+02:00"",
                ""scheduledTimeUtc"": ""2021-06-30 20:10Z"",
                ""actualTimeUtc"": ""2021-06-30 20:25Z"",
                ""runwayTimeUtc"": ""2021-06-30 20:22Z"",
                ""terminal"": ""T2"",
                ""baggageBelt"": ""24"",
                ""quality"": [
                    ""Basic"",
                    ""Live""
                    ]
            },
            ""number"": ""LH 1402"",
            ""callSign"": ""DLH6UP"",
            ""status"": ""Arrived"",
            ""codeshareStatus"": ""IsCodeshared"",
            ""isCargo"": false,
            ""aircraft"": {
                    ""reg"": ""D-AIUC"",
                ""modeS"": ""3C66A3"",
                ""model"": ""Airbus A320""
            },
            ""airline"": {
                    ""name"": ""Lufthansa""
            }
            },
        {
                ""movement"": {
                    ""airport"": {
                        ""icao"": ""DTNH"",
                    ""iata"": ""NBE"",
                    ""name"": ""Enfidha""
                    },
                ""scheduledTimeLocal"": ""2021-06-30 23:55+02:00"",
                ""actualTimeLocal"": ""2021-06-30 23:15+02:00"",
                ""runwayTimeLocal"": ""2021-06-30 23:12+02:00"",
                ""scheduledTimeUtc"": ""2021-06-30 21:55Z"",
                ""actualTimeUtc"": ""2021-06-30 21:15Z"",
                ""runwayTimeUtc"": ""2021-06-30 21:12Z"",
                ""terminal"": ""T1"",
                ""baggageBelt"": ""13"",
                ""quality"": [
                    ""Basic"",
                    ""Live""
                    ]
            },
            ""number"": ""QS 2831"",
            ""callSign"": ""TVS2831"",
            ""status"": ""Arrived"",
            ""codeshareStatus"": ""IsOperator"",
            ""isCargo"": false,
            ""aircraft"": {
                    ""reg"": ""OK-SWW"",
                ""modeS"": ""49D2B9"",
                ""model"": ""Boeing 737-700""
            },
            ""airline"": {
                    ""name"": ""SmartWings""
            }
            }
    ]
}";
            var scheduleObject = JsonSerializer.Deserialize<TSchedule>(contentString, options);

            return Task.Run(() => scheduleObject);
        }

        public Task<TFlightInfo[]> GetFlightInfoAsync<TFlightInfo>(string flightNumber, DateTime flightDate)
            where TFlightInfo : IFlight {

            var contentString = "[{\"greatCircleDistance\":{\"meter\":408509.87,\"km\":408.51,\"mile\":253.836,\"nm\":220.578,\"feet\":1340255.48},\"departure\":{\"airport\":{\"icao\":\"LKPR\",\"iata\":\"PRG\",\"name\":\"Prague, Ruzyně\",\"shortName\":\"Ruzyně\",\"municipalityName\":\"Prague\",\"location\":{\"lat\":50.1008,\"lon\":14.26},\"countryCode\":\"CZ\"},\"scheduledTimeLocal\":\"2021 - 07 - 01 10:20 + 02:00\",\"actualTimeLocal\":\"2021 - 07 - 01 10:35 + 02:00\",\"runwayTimeLocal\":\"2021 - 07 - 01 10:43 + 02:00\",\"scheduledTimeUtc\":\"2021 - 07 - 01 08:20Z\",\"actualTimeUtc\":\"2021 - 07 - 01 08:35Z\",\"runwayTimeUtc\":\"2021 - 07 - 01 08:43Z\",\"terminal\":\"T2\",\"gate\":\"C9\",\"runway\":\"24\",\"quality\":[\"Basic\",\"Live\"]},\"arrival\":{\"airport\":{\"icao\":\"EDDF\",\"iata\":\"FRA\",\"name\":\"Frankfurt - am - Main\",\"shortName\":\"Frankfurt - am - Main\",\"municipalityName\":\"Frankfurt - am - Main\",\"location\":{\"lat\":50.0264,\"lon\":8.543129},\"countryCode\":\"DE\"},\"scheduledTimeLocal\":\"2021 - 07 - 01 11:30 + 02:00\",\"actualTimeLocal\":\"2021 - 07 - 01 11:38 + 02:00\",\"runwayTimeLocal\":\"2021 - 07 - 01 11:27 + 02:00\",\"scheduledTimeUtc\":\"2021 - 07 - 01 09:30Z\",\"actualTimeUtc\":\"2021 - 07 - 01 09:38Z\",\"runwayTimeUtc\":\"2021 - 07 - 01 09:27Z\",\"terminal\":\"1\",\"gate\":\"A2\",\"runway\":\"25R\",\"quality\":[\"Basic\",\"Live\"]},\"lastUpdatedUtc\":\"2021 - 07 - 01 14:27Z\",\"number\":\"LH 1393\",\"callSign\":\"DLH7P\",\"status\":\"Arrived\",\"codeshareStatus\":\"IsOperator\",\"isCargo\":false,\"aircraft\":{\"reg\":\"D - AIUG\",\"modeS\":\"3C66A7\",\"model\":\"Airbus A320\"},\"airline\":{\"name\":\"Lufthansa\"}}]";

            var flightInfoObject = JsonSerializer.Deserialize<TFlightInfo[]>(contentString, options);

            return Task.Run(() => flightInfoObject);
        }
    }
}
