using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AdviPort {
	static class Encryptor {

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

			if (input is null) return null;

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
				// TODO: Add logging error here
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
}
