using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace AdviPort {
	class UserProfile {

		public string APIKey { get; }

		public string UserName { get; }

		public List<string> FavouriteAirports { get; private set; }

		public List<string> SchedulesHistory { get; private set; }

		public List<string> SavedFlights { get; private set; }

		public UserProfile(string userName, string apiKey) {
			APIKey = apiKey;
			UserName = userName;
			FavouriteAirports = new List<string>();
			SchedulesHistory = new List<string>(10);	// 10 last successful schedule table queries should be saved into user's history
			SavedFlights = new List<string>();
		}
	}

	interface IUserProfileCreator {
		int CreateProfile(string userName, string apiKey);
	}

	interface IUserChecker {
		bool UserExists(string userName);
		UserProfile GetProfile(string userName);
	}

	interface IUserDBHandler : IUserChecker, IUserProfileCreator { }

	class FileSystemProfileDB : IUserDBHandler  {

		private string ProfilesDirectoryPath { get; }

		public FileSystemProfileDB() {
			string profilesPath = GeneralApplicationSettings.SearchDir(Directory.GetCurrentDirectory(), "profiles");

			if (profilesPath is null) {
				string current = Directory.GetCurrentDirectory();
				const int PARENTS = 3;
				for (int i = 0; i < PARENTS; i++) {
					// Backing up from current directory PARENTS times 
					current = Directory.GetParent(current).FullName;
				}

				profilesPath = current + Path.DirectorySeparatorChar + "profiles";
				Directory.CreateDirectory(profilesPath);
				Console.Error.WriteLine($"TO LOG: Created a new directory: {profilesPath}");
				// TODO: Log the creation of given directory.
			}

			ProfilesDirectoryPath = profilesPath;
		}

		public int CreateProfile(string userName, string apiKey) {

			apiKey = Encryptor.Encrypt(apiKey);

			if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(apiKey)) {
				Console.Error.WriteLine("The username nor API key cannot be an empty string.");
				return 1;
			}

			var writer = CreateProfileFile(userName);

			if (writer is null) { return 1; }

			using (writer) {
				try {
					var profile = new UserProfile(userName, apiKey);
					var options = new JsonSerializerOptions() {
						WriteIndented = true
					};

					string serializedProfile = JsonSerializer.Serialize<UserProfile>(profile, options);

					writer.Write(serializedProfile);
					Console.WriteLine("Registration of a new user is successful.");
				} catch {
					// Log the error 
					// User profile should be deleted if anything goes wrong 
					File.Delete(GetProfileFilePath(userName));
					return 1;
				}
			}

			return 0;
		}

		public UserProfile GetProfile(string userName) {
			string[] profiles = GeneralApplicationSettings.SearchFiles(ProfilesDirectoryPath, GetProfileFileName(userName), requiredFiles: 1);

			if (profiles == null) {
				Console.Error.WriteLine("User not found.");
				return null;
			}

			using var profileReader = GeneralApplicationSettings.GetTextReader(profiles);

			var profile = JsonSerializer.Deserialize<UserProfile>(profileReader.ReadToEnd(), new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

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

		private string GetProfileFilePath(string userName) => ProfilesDirectoryPath + Path.DirectorySeparatorChar + GetProfileFileName(userName);

		private TextWriter CreateProfileFile(string userName) {
			try {
				TextWriter writer = new StreamWriter(GetProfileFilePath(userName));
				return writer;
			} catch {
				Console.Error.WriteLine("Profile file for this user could not be created.");
				return null;
			}
		}
	}
}
