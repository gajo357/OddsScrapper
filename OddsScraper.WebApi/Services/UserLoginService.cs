using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OddsScraper.WebApi.Services
{
    public class UserLoginService : IUserLoginService
    {
        public UserLoginService(FSharp.CommonScraping.Downloader.IDownloader downloader)
        {
            Downloader = downloader;
        }


        private IDictionary<string, string> Users { get; } = new ConcurrentDictionary<string, string>();
        private FSharp.CommonScraping.Downloader.IDownloader Downloader { get; }

        public bool IsUserLoggedIn(string username) => Users.ContainsKey(username);

        public async Task<string> LogInAsync(string username, string password)
        {
            if (username.Contains("@"))
                return string.Empty;

            if (IsUserLoggedIn(username))
                return Users[username];

            try
            {
                if (!await Downloader.LogIn(username, password))
                    return string.Empty;

                var userCode = GetHashCode(username, password);
                if (string.IsNullOrEmpty(userCode))
                    return string.Empty;

                Users.Add(username, userCode);
                return userCode;
            }
            catch
            {
                return string.Empty;
            }
        }

        public bool IsHashPresent(string hash) => Users.Values.Any(v => v == hash);

        private static string GetHashCode(string username, string password)
        {
            using (SHA256 mySHA256 = SHA256.Create())
            {
                // Compute the hash of the fileStream.
                byte[] hashValue = mySHA256.ComputeHash(Encoding.UTF8.GetBytes(username + password + DateTime.Now.ToString()));

                return HashToString(hashValue);
            }
        }

        private static string HashToString(byte[] data)
        {
            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
    }
}
