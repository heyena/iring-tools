using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;

namespace Bechtel.DataLayer
{
    internal class WebClient : IWebClient
    {
        private HttpClient _client = null;

        public WebClient()
        {
            _client = new HttpClient();
        }

        public WebClient(string baseUrl)
            : this()
        {
            _client.BaseAddress = new Uri(baseUrl);
            // Add an Accept header for JSON format.
            _client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));


        }

        public WebClient(string baseUrl, string appKey, string accessToken)
            : this(baseUrl)
        {
            _client.DefaultRequestHeaders.Add("Authorization", accessToken);
            _client.DefaultRequestHeaders.Add("X-myPSN-AppKey", appKey);
        }

        public string MakeGetRequest(string url)
        {
            string response = _client.GetStringAsync(url).Result;
            return response;
        }

        public void MakePutRequest(string url, string jsonString)
        {
            StringContent sc = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var rsponse = _client.PutAsync(url, sc).Result.EnsureSuccessStatusCode();
        }

        public void MakePostRequest(string url, string jsonString)
        {
            StringContent sc = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var rsponse = _client.PostAsync(url, sc).Result.EnsureSuccessStatusCode();
        }

        public void MakeDeleteRequest(string url)
        {
            _client.DeleteAsync(url).Result.EnsureSuccessStatusCode();
        }


    }
}
