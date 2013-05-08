﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Bechtel.DataLayer
{
    public interface IWebClient
    {
        string MakeGetRequest(string url);
        void MakePutRequest(string url, string objectString);
        void MakePostRequest(string url, string objectString);
        void MakeDeleteRequest(string url);

    }

    internal class IringWebClient : IWebClient
    {
        org.iringtools.utility.WebHttpClient _client = null ;

        public IringWebClient()
        {
            _client = new org.iringtools.utility.WebHttpClient("");
        }

        public IringWebClient(string baseUrl)
        {
            _client = new org.iringtools.utility.WebHttpClient(baseUrl);
        }

        public IringWebClient(string baseUrl, string appKey, string accessToken)
            : this(baseUrl)
        {
            _client.AppKey = appKey;
            _client.AccessToken = accessToken;
            _client.ContentType = @"application/json";

            string testAppKey = _client.AppKey;
            string testToken = _client.AccessToken;
            
        }

        public IringWebClient(string baseUrl, string appKey, string accessToken,string contentType)
            : this(baseUrl)
        {
            _client.AppKey = appKey;
            _client.AccessToken = accessToken;
            _client.ContentType = @contentType;

            string testAppKey = _client.AppKey;
            string testToken = _client.AccessToken;

        }

        public string MakeGetRequest(string url)
        {
           return  _client.GetJson(url);
           //Stream st = _client.GetStream(url);
           //string response = _client.GetMessage(url);
           //return response;
        }

        public void MakePutRequest(string url, string jsonString)
        {

            _client.PutJson(url, jsonString);
            //byte[] byteArray = Encoding.UTF8.GetBytes(jsonString);
            //using (MemoryStream stream = new MemoryStream(byteArray)) 
            //{
            //    _client.PutStream(url, stream);
            //}
        }

        public void MakePostRequest(string url, string jsonString)
        {
            _client.PostJson(url, jsonString);
            //byte[] byteArray = Encoding.UTF8.GetBytes(jsonString);
            //using (MemoryStream stream = new MemoryStream(byteArray))
            //{
            //    _client.PostStream(url, stream);
            //}
        }


        public void MakeDeleteRequest(string url)
        {
            throw new NotImplementedException();
        }
    }
}
