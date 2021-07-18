using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;

namespace AdviPort.UI {

	/// <summary>
	/// Provides a way to write changes for a given user profile.
	/// </summary>
	public interface IUserProfileWriter {
		int WriteUserProfile(UserProfile profile);
	}

	/// <summary>
	/// Describes input reading contract. The contract can be fulfilled using
	/// any available reader object.
	/// </summary>
	public interface IUserInterfaceReader {
		string ReadUserInput(string initialPrompt = "Please enter your choice");
	}

	/// <summary>
	/// Checks for user profile existence and attempts to retrieve 
	/// a user profile.
	/// </summary>
	public interface IUserChecker {
		bool UserExists(string userName);
		UserProfile GetProfile(string userName);
	}

	/// <summary>
	/// Provides a method used to create a new user profile.
	/// </summary>
	public interface IUserProfileCreator {
		int CreateProfile(string userName, string password, string apiKey);
	}

	public interface IUserPasswordCreator {
		string CreateUserPassword();
	}

	public class UserInputReader : IUserInterfaceReader {

		public UserInputReader() { }

		public virtual string ReadUserInput(string initialPrompt) {
			if (!(initialPrompt == null)) {
				Console.Write(initialPrompt + ": ");
			}

			var input = Console.ReadLine();

			if (input == null) throw new ArgumentNullException("The input should not be null.");

			return input.Trim();
		}
	}

	public class ConsolePasswordReader : IUserInterfaceReader {
		public static ConsolePasswordReader Instance { get; } = new ConsolePasswordReader();

		private ConsolePasswordReader() { }

		public string ReadUserInput(string initialPrompt) {
			if (!(initialPrompt == null)) {
				Console.Write(initialPrompt + ": ");
			}

			StringBuilder sb = new StringBuilder();
			ConsoleKeyInfo key;

			do {
				key = Console.ReadKey(true);
				switch (key.Key) {
					case ConsoleKey.Escape:
					case ConsoleKey.Backspace:
					case ConsoleKey.Home:
					case ConsoleKey.End:
						continue;
					case ConsoleKey.Enter:
						Console.SetCursorPosition(0, Console.CursorTop + 1);
						break;
					default:
						sb.Append(key.KeyChar);
						break;
				}

			} while (key.Key != ConsoleKey.Enter);

			return sb.ToString();
		}

		public void ConsoleClearLine(int initPosition, int rowsToClear = 2) {
			Console.SetCursorPosition(0, initPosition);
			for (int i = 0; i < rowsToClear; i++) {
				Console.WriteLine(new string(' ', Console.WindowWidth));
			}
			Console.SetCursorPosition(0, initPosition);
		}
	}

	public class DefaultUserPasswordCreator : IUserPasswordCreator {

		private static DefaultUserPasswordCreator Instance { get; set; }
		private IUserInterfaceReader Reader { get; }
		private DefaultUserPasswordCreator(IUserInterfaceReader inputReader) {
			Reader = inputReader;
		}

		public static DefaultUserPasswordCreator GetInstance(IUserInterfaceReader inputReader) {
			if (Instance == null || Instance.Reader != inputReader) {
				// Creating new instance only if it does not exist yet or if the conditions change
				Instance = new DefaultUserPasswordCreator(inputReader);
			}

			return Instance;
		}

		public string CreateUserPassword() {

			bool incorrectPassword;
			Regex regex = new Regex("^[a-zA-Z0-9]{8,}$");
			string passwd1;

			do {
				incorrectPassword = true;
				int cursor = Console.CursorTop;
				passwd1 = ConsolePasswordReader.Instance.ReadUserInput("Please enter password you want to use (at least 8 characters - letters and numbers only)");

				if (string.IsNullOrWhiteSpace(passwd1)) {
					Console.Error.WriteLine("The password cannot be empty. Please try again.");
					Thread.Sleep(350);
					ConsolePasswordReader.Instance.ConsoleClearLine(cursor);
					continue;
				}

				if (!regex.IsMatch(passwd1)) {
					Console.Error.WriteLine("Please make sure your password contains at least 8 characters (letters and numbers only)");
					incorrectPassword = true;
					continue;
				}

				var passwd2 = ConsolePasswordReader.Instance.ReadUserInput("Please type your password again");

				if (passwd1 != passwd2) {
					Console.Error.WriteLine("Passwords do not match. Please try again.");
					incorrectPassword = true;
					continue;
				}

				incorrectPassword = false;

			} while (incorrectPassword);

			return passwd1;
		}
	}

	public interface IUserDBHandler : IUserChecker, IUserPasswordCreator, IUserProfileCreator { }

	public static class Encryptor {

		private static readonly byte[] key = new byte[] {
			0x64, 0x01, 0x6f, 0xe1, 0xbe, 0xa7, 0x51, 0xd6,
			0xb9, 0x4b, 0xfa, 0xdc, 0x65, 0x8c, 0x4a, 0xfb,
			0x7e, 0xa9, 0x45, 0xc1, 0xfe, 0x12, 0xda, 0x4c,
			0x2b, 0x5d, 0x59, 0x27, 0x8e, 0x78, 0x2a, 0x30
		};

		private static readonly byte[] iv = new byte[] {
			0x65, 0x8c, 0x4a, 0xfb, 0x8e, 0x78, 0x2a, 0x30,
			0x2b, 0x5d, 0x59, 0x27, 0xb9, 0x4b, 0xfa, 0xdc,
		};

		public static string Encrypt(string input) {

			if (input == null) return null;

			try {
				using Aes aes = Aes.Create();
				aes.Key = key;
				aes.IV = iv;

				ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

				byte[] encBytes;
				using (MemoryStream stream = new MemoryStream()) {
					using CryptoStream cryptoStream = new CryptoStream(stream, encryptor, CryptoStreamMode.Write);
					using (TextWriter writer = new StreamWriter(cryptoStream)) {
						writer.Write(input);
					}

					encBytes = stream.ToArray();
				}

				return Convert.ToBase64String(encBytes);
			} catch {
				return null;
			}
		}

		public static string Decrypt(string input) {
			try {
				using Aes aes = Aes.Create();
				aes.Key = key;
				aes.IV = iv;

				var decryptor = aes.CreateDecryptor();

				using MemoryStream stream = new MemoryStream(Convert.FromBase64String(input));
				using CryptoStream cryptoStream = new CryptoStream(stream, decryptor, CryptoStreamMode.Read);
				using TextReader reader = new StreamReader(cryptoStream);

				return reader.ReadToEnd();
			} catch {
				return null;
			}
		}
	}

	public class FileSystemProfileDB : IUserChecker, IUserProfileCreator {

		internal IUserProfileWriter ProfileWriter { get; }

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

		private class FileSystemProfileDBWriter : IUserProfileWriter {

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

				if (FileWriter == null) { return 1; }

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

	public interface IMainPageNavigator {
		string NavigateOrReadInput(int maxPlugins);
	}

	public class DefaultMainPageNavigator : IMainPageNavigator {

		private int cursorTop;
		private int maxPlugins;
		private readonly string message = "Please enter your choice: ";

		public string NavigateOrReadInput(int maxPlugins) {

			bool textReadState = true;
			cursorTop = Console.CursorTop;
			this.maxPlugins = maxPlugins;
			string resultInput;

			do {
				Console.CursorTop = cursorTop;
				Console.Write(message);

				if (textReadState) { 
					resultInput = ReadInput(); 
				} else {
					resultInput = Navigate(0); 
				}

				resultInput = resultInput.Trim();
			} while (resultInput == null || resultInput.Length == 0);

			return resultInput;
		}

		private string ReadInput() {
			var key = Console.ReadKey();

			if (key.Key == ConsoleKey.UpArrow || key.Key == ConsoleKey.DownArrow) {
				return Navigate(1); 
			}

			if (key.Key == ConsoleKey.Spacebar || key.Key == ConsoleKey.Backspace) {
				Console.CursorLeft = message.Length;
				return ReadInput();
			}

			string stringInput = Console.ReadLine();

			return key.KeyChar + stringInput;
		}

		private string Navigate(int initValue) {
			ConsoleKeyInfo key;
			int position = initValue;

			do {
				Console.Write(position);
				key = Console.ReadKey();
				Console.CursorLeft = message.Length;

				if (key.Key == ConsoleKey.UpArrow) { --position; }
				if (key.Key == ConsoleKey.DownArrow && position + 1 <= maxPlugins) { ++position; }

				if (position == 0) {
					Console.CursorLeft = message.Length;
					Console.Write(new string(' ', Console.WindowWidth));
					Console.CursorLeft = message.Length;
					Console.CursorTop = cursorTop;
					return ReadInput();
				}

			} while (key.Key != ConsoleKey.Enter);

			return position.ToString();
		}
	}
}
