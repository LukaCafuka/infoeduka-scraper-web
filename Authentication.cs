using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components;

namespace InfoedukaScraper
{
    public class IeAuthentication()
    {
        private string Username { get; set; }
        private string password;
        private string Password
        {
            get => password;
            set
            {
                if (value.Length < 8)
                {
                    throw new Exception("Password must be at least 8 characters long");
                }
                else
                {
                    password = value;
                }
            }
            
        }

        public void SetCredentials(string username, string password)
        {
            Username = username;
            Password = password;
        }

        public override string ToString()
        {
            return $"Username: {Username}\nPassword: {Password.Substring(2, 2)}";
        }

        // function to create the request to the Infoeduka login page
        public async Task<string> GetCookie()
        {
            string loginUrl = "https://student.algebra.hr/digitalnareferada/api/login";
            string cookieType = "PHPSESSID";
            
            // initialize http client
            var handler = new HttpClientHandler
            {
                CookieContainer = new CookieContainer()
            };
            var client = new HttpClient(handler);

            try
            {

                // define the payload with username and password in JSON format
                var payload = new
                {
                    username = Username,
                    password = Password
                };

                // serialize the payload to JSON
                var jsonContent =
                    new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                // send the POST request
                var response = await client.PostAsync(loginUrl, jsonContent);

                // ensure the response indicates success
                response.EnsureSuccessStatusCode();

                // get cookies from the response URI
                Uri responseUri = response.RequestMessage.RequestUri;
                CookieCollection cookies = handler.CookieContainer.GetCookies(responseUri);

                // find cookie
                string loginCookie = null;
                foreach (Cookie cookie in cookies)
                {
                    if (cookie.Name.Equals(cookieType,
                            StringComparison.OrdinalIgnoreCase)) // replace with actual cookie name if known
                    {
                        loginCookie = cookie.ToString();
                        break;
                    }
                }

                return loginCookie;

            }
            catch (HttpRequestException ex)
            {
                // clear cookies on failure to allow a fresh attempt next time
                Console.WriteLine("Login failed: " + ex.Message);
                return null;
            }
            finally
            {
                handler.Dispose();
            }
        }

    }
}
