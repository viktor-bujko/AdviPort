using System;
using System.Collections.Generic;
using System.Text;

namespace AdviPort {
	class UserProfile {

		public string APIKey { get; }
		public List<string> FavouriteAirports { get; private set; }

		public List<string> SchedulesHistory { get; private set; }

		public List<string> SavedFlights { get; private set; }

		public UserProfile(string apiKey) {
			APIKey = apiKey;
			FavouriteAirports = new List<string>();
			SchedulesHistory = new List<string>(10);	// 10 last successful schedule table queries should be saved into user's history
			SavedFlights = new List<string>();
		}
	}
}
