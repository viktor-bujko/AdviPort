using System;
using System.Collections.Generic;
using AdviPort.ResponseObjects;
using AdviPort.UI;

namespace AdviPort {

	/// <summary>
	/// Class representing an instance of a user. This class contains information about user
	/// as well as its identification (<code>UserName</code>, encrypted <code>Password</code> 
	/// and <code>APIKey</code> used to perform API calls). Other instance properties are used
	/// to improve and adapt application user interface or to cache information.
	/// </summary>
	public class UserProfile {
		public string APIKey { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }
		public string LastSearchedFlight { get; set; }

		/// <summary>
		/// Only Airport instances may be stored in the dictionary due to lack of support of JSON library interface deserialization.
		/// </summary>
		public Dictionary<string, Airport> FavouriteAirports { get; set; }
		public List<string> SchedulesHistory { get; set; }
		public string MainPageStyle { get; set; } = "";

		public UserProfile() { }

		public UserProfile(string userName, string password, string apiKey) {
			APIKey = apiKey;
			UserName = userName;
			Password = password;
			FavouriteAirports = new Dictionary<string, Airport>();
			SchedulesHistory = new List<string>(10);    // 10 last successful schedule table queries should be saved into user's history
		}

		/// <summary>
		/// Changes user's preferred style of the main application page.
		/// </summary>
		/// <param name="profileWriter">Instance which is responsible for writing the changes to the user profile file.</param>
		public void SetMainPageStyle(IUserProfileWriter profileWriter) {
			int length = MainPageHandlerSelector.MainPageDesignNames.Count;
			int currentLookIdx = MainPageHandlerSelector.MainPageDesignNames.IndexOf(MainPageStyle);

			MainPageStyle = MainPageHandlerSelector.MainPageDesignNames[++currentLookIdx % length];

			profileWriter.WriteUserProfile(this);
		}

		/// <summary>
		/// Method which saves only 10 last successful searches for flight information.
		/// </summary>
		/// <param name="profileWriter">Instance which is responsible for writing the changes to the user profile file.</param>
		public void TrimFlightsHistory(IUserProfileWriter profileWriter) {
			SchedulesHistory = SchedulesHistory.GetRange(Math.Max(SchedulesHistory.Count - 10, 0), Math.Min(SchedulesHistory.Count, 10));

			profileWriter.WriteUserProfile(this);
		}
	}
}
