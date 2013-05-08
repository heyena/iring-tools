using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;

namespace Bechtel.DataLayer
{
    internal class MockWebClient : IWebClient
    {
       
        public MockWebClient()
        { }

        public MockWebClient(string baseUrl)
            : this()
        { }

        public MockWebClient(string baseUrl, string appKey, string accessToken)
            : this(baseUrl)
        { }

        public string MakeGetRequest(string url)
        {
            string response = "{\"total\":1,\"start\":1,\"limit\":1,\"Items\":[{\"Id\":3,\"Name\":\"GENERAL MANAGEMENT\",\"Description\":\"GENERAL MANAGEMENT\"}]}";
            return response;
        }


        public void MakePutRequest(string url, string jsonString)
        {
            //
        }

        public void MakePostRequest(string url, string jsonString)
        {
            //
        }


        public void MakeDeleteRequest(string url)
        {
            throw new NotImplementedException();
        }
    }
}
