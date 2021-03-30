using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace AdviPort {
	class UserProfile {

		public string APIKey { get; set; }

		public string UserName { get; set; }

		public string Password { get; set; }

		public Dictionary<string, ResponseObjects.Airport> FavouriteAirports { get; set; }

		public List<string> SchedulesHistory { get; set; }

		public List<string> SavedFlights { get; set; }

		public UserProfile() { }

		public UserProfile(string userName, string password, string apiKey) {
			APIKey = apiKey;
			UserName = userName;
			Password = password;
			FavouriteAirports = new Dictionary<string, ResponseObjects.Airport>();
			SchedulesHistory = new List<string>(10);	// 10 last successful schedule table queries should be saved into user's history
			SavedFlights = new List<string>();
		}
	}

	class FileSystemProfileDB : IUserChecker, IUserProfileCreator  {

		private IUserProfileWriter ProfileWriter { get; }

		private string ProfilesDirectoryPath { get; } = GeneralApplicationSettings.GetProfilesDirectoryPath();

		public FileSystemProfileDB() {
			ProfileWriter = new FileSystemProfileDBWriter();
		}

		public int CreateProfile(string userName, string password, string apiKey) {

			apiKey = Encryptor.Encrypt(apiKey);
			password = Encryptor.Encrypt(password);

			if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(apiKey)) {
				Console.Error.WriteLine("The username nor API key cannot be an empty string.");
				return 1;
			}

			if (password == null) { return 1; }

			var userProfile = new UserProfile(userName, password, apiKey);

			int writeExitCode = ProfileWriter.WriteUserProfile(userProfile);
			if (writeExitCode == 0) {
				Console.WriteLine("Registration of a new user is successful.");
			}

			return writeExitCode;
		}

		public UserProfile GetProfile(string userName) {
			string[] profiles = GeneralApplicationSettings.SearchFiles(ProfilesDirectoryPath, GetProfileFileName(userName), requiredFiles: 1);

			if (profiles == null) {
				Console.Error.WriteLine("User not found.");
				return null;
			}

			using var profileReader = GeneralApplicationSettings.GetTextReader(profiles);

			string content = profileReader.ReadToEnd();

			var profile = JsonSerializer.Deserialize<UserProfile>(content, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

			return profile;
		}

		public bool UserExists(string userName) {

			string[] profilePaths = GeneralApplicationSettings.SearchFiles(ProfilesDirectoryPath, GetProfileFileName(userName));

			if (profilePaths == null) {
				throw new ArgumentNullException("File not found");
			}

			// If true, userprofile file with such username can be created.
			return profilePaths.Length == 1;
		}

		private string GetProfileFileName(string userName) => $"{userName}_userprofile.apt";
	}

	class FileSystemProfileDBWriter : IUserProfileWriter {

		private string ProfilesDirectoryPath { get; }

		private TextWriter FileWriter { get; set; }

		public FileSystemProfileDBWriter() {
			ProfilesDirectoryPath = GeneralApplicationSettings.GetProfilesDirectoryPath();
		}

		public int WriteUserProfile(UserProfile profile) {

			try {
				FileWriter = new StreamWriter(GetProfileFilePath(profile.UserName));
			} catch {
				Console.Error.WriteLine("Profile file for this user could not be created.");
				return 1;
			}

			if (FileWriter is null) { return 1; }

			using (FileWriter) {
				try {
					var options = new JsonSerializerOptions() {
						WriteIndented = true
					};

					string serializedProfile = JsonSerializer.Serialize<UserProfile>(profile, options);

					FileWriter.Write(serializedProfile);
				} catch {
					// Log the error 
					// User profile should be deleted if anything goes wrong 
					File.Delete(GetProfileFilePath(profile.UserName));
					return 1;
				}
			}

			Console.WriteLine("Changes written successfully.");
			return 0;
		}
		private string GetProfileFilePath(string userName) => ProfilesDirectoryPath + Path.DirectorySeparatorChar + GetProfileFileName(userName);

		private string GetProfileFileName(string userName) => $"{userName}_userprofile.apt";
	}
}
