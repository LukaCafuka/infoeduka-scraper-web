using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components;

namespace InfoedukaScraper
{
    public class IeAuthentication
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
                    Debug.WriteLine("Password is too short.");
                    password = value;
                }
                else
                {
                    password = value;
                }
            }
            
        }

        public IeAuthentication()
        {
            Username = string.Empty;
            password = string.Empty;
        }
        
        public int CheckCredentials(string inputUsername, string inputPassword)
        {
            if (!(string.IsNullOrWhiteSpace(inputUsername) && string.IsNullOrWhiteSpace(inputPassword)))
            {
                if (inputPassword.Length < 8)
                {
                    return 2;
                }
                else
                {
                    Username = inputUsername;
                    Password = inputPassword;
                    return 0;
                }

                return 1;
            }

            return 1;
        }

        public override string ToString()
        {
            return $"Username: {Username}\nPassword Length: {Password.Length}";
        }
        
        public string LoginCookie { get; set; }

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
