using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace UCDG.Infrastructure.Helpers
{
    public class ClientTokenHelper
    {

        public static HttpClient HttpClient(string token)
        {
            var authValue = new AuthenticationHeaderValue("Bearer", token);

            var client = new HttpClient()
            {
                DefaultRequestHeaders = { Authorization = authValue }
            };
            return client;
        }

        public static string GetToken(string url, string userName, string password, bool isFormUrlEncoded)
        {
            return isFormUrlEncoded ? GetFormUrlEncodedToken(url, userName, password) : GetJsonToken(url, userName, password);

        }


        private static string GetFormUrlEncodedToken(string url, string userName, string password)
        {
            var pairs = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>( "grant_type", "password" ),
                new KeyValuePair<string, string>( "username", userName ),
                new KeyValuePair<string, string> ( "Password", password )
            };
            var content = new FormUrlEncodedContent(pairs);

            using (var client = new HttpClient())
            {
                var response = client.PostAsync(url, content).Result;

                return response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable ? "ServiceUnavailable" : response.Content.ReadAsStringAsync().Result;
            }
        }

        //private static string GetJsonToken(string url, string userName, string password)
        //{
        //    var tokenStringModel = new TokenStringModel();
        //    using (var client = HttpClient(null))
        //    {
        //        var response = client.PostAsJsonAsync(url, new { Username = userName, Password = password }).Result;

        //        var responseData = response?.Content.ReadAsStringAsync().Result;

        //        if (url == "https://webservices.uj.ac.za/WorkflowAPI/api/Auth" || url == "https://localhost:44349/api/Auth" || url == "https://dev.soldev.uj.ac.za/api/WorkflowAPI/api/Auth")
        //        {
        //            return responseData;
        //        }
        //        return JsonConvert.DeserializeObject<TokenStringModel>(responseData).TokenString;

        //    }

        //}

        private static string GetJsonToken(string url, string userName, string password)
        {
            using (var client = new HttpClient())
            {
                // Create a dictionary to hold the request data
                var requestData = new Dictionary<string, string>
        {
            { "Username", userName },
            { "Password", password }
        };

                // Send the POST request
                var response = client.PostAsJsonAsync(url, requestData).Result;

                // Log the status code and reason phrase for debugging
                Console.WriteLine($"Response Status Code: {response.StatusCode}");
                Console.WriteLine($"Response Reason Phrase: {response.ReasonPhrase}");

                // Ensure the response is successful
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Request failed with status code {response.StatusCode}: {response.ReasonPhrase}");
                }

                // Log response headers for debugging
                foreach (var header in response.Headers)
                {
                    Console.WriteLine($"Header: {header.Key} = {string.Join(", ", header.Value)}");
                }

                // Log content type for debugging
                var contentType = response.Content.Headers.ContentType;
                Console.WriteLine($"Content Type: {contentType}");

                // Read and log the response data
                var responseData = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine("Response Data: " + responseData);

                try
                {
                    // Check if response data is null or empty
                    if (string.IsNullOrWhiteSpace(responseData))
                    {
                        throw new Exception("Response data is null or empty.");
                    }

                    // Ensure the content type is JSON or handle text/plain appropriately
                    if (contentType == null || contentType.MediaType.Equals("application/json", StringComparison.OrdinalIgnoreCase))
                    {
                        // Deserialize the response data
                        var tokenStringModel = JsonConvert.DeserializeObject<TokenStringModel>(responseData);
                        if (tokenStringModel == null)
                        {
                            throw new Exception("Deserialization resulted in a null object.");
                        }
                        if (string.IsNullOrEmpty(tokenStringModel.TokenString))
                        {
                            throw new Exception("Token string is null or empty.");
                        }
                        return tokenStringModel.TokenString;
                    }
                    else if (contentType.MediaType.Equals("text/plain", StringComparison.OrdinalIgnoreCase))
                    {
                        // Handle the text/plain response
                        Console.WriteLine("Received text/plain response: " + responseData);
                        return responseData; // Or handle as needed
                    }
                    else
                    {
                        throw new Exception($"Unexpected content type: {contentType.MediaType}");
                    }
                }
                catch (JsonException jsonEx)
                {
                    // Log or handle the JSON deserialization error
                    Console.WriteLine("Error deserializing JSON response: " + jsonEx.Message);
                    Console.WriteLine("Raw response data: " + responseData);
                    throw new Exception("Error deserializing JSON response: " + jsonEx.Message, jsonEx);
                }
                catch (Exception ex)
                {
                    // Handle any other exceptions
                    Console.WriteLine("An error occurred: " + ex.Message);
                    throw new Exception("An error occurred: " + ex.Message, ex);
                }
            }
        }


    }
    public class TokenStringModel
    {
        public string TokenString { get; set; }
    }
}

