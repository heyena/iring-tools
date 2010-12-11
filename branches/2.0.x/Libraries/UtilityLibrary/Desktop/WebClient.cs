﻿// Copyright (c) 2009, ids-adi.org /////////////////////////////////////////////
// All rights reserved.
//------------------------------------------------------------------------------
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the ids-adi.org nor the
//       names of its contributors may be used to endorse or promote products
//       derived from this software without specific prior written permission.
//------------------------------------------------------------------------------
// THIS SOFTWARE IS PROVIDED BY ids-adi.org ''AS IS'' AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL ids-adi.org BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Web;
using System.Collections.Generic;
using System.Configuration;
using System.Security.Cryptography.X509Certificates;

namespace org.iringtools.utility
{
    public class MultiPartMessage
    {
      public MultipartMessageType type { get; set; }
      public string name { get; set; }
      public string message { get; set; }
      public string fileName { get; set; }
      public string mimeType { get; set; }
    }

    public enum MultipartMessageType
    {
      File,
      FormData,
    }

    public class WebHttpClient : IWebHttpClient
    {
        private string _boundary = Guid.NewGuid().ToString().Replace("-", "");
        private string _baseUri = String.Empty;
        private NetworkCredential _credentials = null;
        private WebProxy _proxy = null;

        private const string NEW_LINE = "\r\n";
        private const int TIMEOUT = 300000;

        public WebHttpClient(string baseUri)
            : this(baseUri, String.Empty, String.Empty, String.Empty)
        {
        }

        public WebHttpClient(string baseUri, string userName, string password)
            : this(baseUri, userName, password, String.Empty)
        {
        }

        public WebHttpClient(string baseUri, string userName, string password, string domain)
        {
            _baseUri = baseUri;

            if (userName != String.Empty && userName != null && domain != String.Empty && domain != null)
            {
                _credentials = new NetworkCredential(userName, password, domain);
            }
            else if (userName != String.Empty && userName != null)
            {
                _credentials = new NetworkCredential(userName, password);
            }
        }

        public WebHttpClient(string baseUri, string proxyUserName, string proxyPassword, string proxyDomain, string proxyHost, int proxyPort)
        {
          _baseUri = baseUri;

          if (proxyHost != String.Empty && proxyHost != null)
          {
            if (proxyUserName != String.Empty && proxyUserName != null && proxyDomain != String.Empty && proxyDomain != null)
            {
              _proxy = new WebProxy(proxyHost, proxyPort);
              _proxy.Credentials = new NetworkCredential(proxyUserName, proxyPassword, proxyDomain);
              ;
            }
            else if (proxyUserName != String.Empty && proxyUserName != null)
            {
              _proxy = new WebProxy(proxyHost, proxyPort);
              _proxy.Credentials = new NetworkCredential(proxyUserName, proxyPassword);
            }
          }
        }

        public WebHttpClient(string baseUri, NetworkCredential credentials, string proxyHost, int proxyPort, NetworkCredential proxyCredentials)
        {
          _baseUri = baseUri;

          _credentials = credentials;

          if (proxyHost != String.Empty && proxyHost != null)
          {
            _proxy = new WebProxy(proxyHost, proxyPort);
            _proxy.Credentials = proxyCredentials;
          }
        }

        public WebHttpClient(string baseUri, NetworkCredential credentials, WebProxy webProxy)
        {
          _baseUri = baseUri;

          _credentials = credentials;
          _proxy = webProxy;
        }

        public WebHttpClient(string baseUri, NetworkCredential credentials)
        {
            this._baseUri = baseUri;

            this._credentials = credentials;
        }

        /// <summary>
        /// HttpUtility.UrlEncode does not encode alpha-numeric characters such as _ - . ' ( ) * and !
        /// This function encodes these characters to create a fully encoded uri
        /// </summary>
        private static string FullEncodeUri(string semiEncodedUri)
        {
            string fullEncodedUri = string.Empty;

            fullEncodedUri = semiEncodedUri.Replace("'", "%22");
            fullEncodedUri = fullEncodedUri.Replace("(", "%28");
            fullEncodedUri = fullEncodedUri.Replace(")", "%29");
            fullEncodedUri = fullEncodedUri.Replace("+", "%20");

            return fullEncodedUri;
        }

        public static string GetUri(string relativeUri)
        {
            string semiEncodedUri = string.Empty;
            string encodedUri = string.Empty;
            //.Net does not allow certain characters in a uri, hence the uri has to be encoded 
            semiEncodedUri = HttpUtility.UrlEncode(relativeUri);

            //encode alpha-numberic characters that are not encoded by HttpUtility.UrlEncode
            encodedUri = FullEncodeUri(semiEncodedUri);

            return encodedUri;
        }

        private void PrepareCredentials(WebRequest request)
        {
          if (_credentials == null)
          {
            request.Credentials = CredentialCache.DefaultCredentials;
          }
          else
          {
            request.Credentials = _credentials;
          }
          
          if (_proxy != null)
          {
            request.Proxy = _proxy;
          }
        }

        // callback used to validate the certificate in an SSL conversation
        private static bool ValidateRemoteCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors policyErrors)
        {
          if (Convert.ToBoolean(ConfigurationManager.AppSettings["IgnoreSslErrors"]))
          {
            // allow any old dodgy certificate...
            return true;
          }
          else
          {
            return policyErrors == SslPolicyErrors.None;
          }
        }

        public T Get<T>(string relativeUri)
        {
            return Get<T>(relativeUri, true);
        }

        public T Get<T>(string relativeUri, bool useDataContractSerializer)
        {
            try
            {
                string uri = _baseUri + relativeUri; // GetUri(relativeUri);

                WebRequest request = HttpWebRequest.Create(uri);

                PrepareCredentials(request);
              
                request.Method = "Get";
                request.Timeout = TIMEOUT;

                // allows for validation of SSL conversations
                ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(
                  ValidateRemoteCertificate
                );

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                T entity = Utility.DeserializeFromStream<T>(response.GetResponseStream(), useDataContractSerializer);

                return entity;
            }
            catch (Exception exception)
            {
                string uri = _baseUri + relativeUri;

                throw new Exception("Error while executing HTTP GET request on" + uri + ".", exception);
            }
        }

        public string GetMessage(string relativeUri)
        {
            try
            {
              string uri = _baseUri + relativeUri; // GetUri(relativeUri);

                WebRequest request = HttpWebRequest.Create(uri);

                PrepareCredentials(request);
                
                request.Method = "Get";
                request.Timeout = TIMEOUT;

                // allows for validation of SSL conversations
                ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(
                  ValidateRemoteCertificate
                );

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                string message = Utility.SerializeFromStream(response.GetResponseStream());

                return message;
            }
            catch (Exception exception)
            {
                string uri = _baseUri + relativeUri;

                throw new Exception("Error while executing HTTP GET request on" + uri + ".", exception);
            }
        }

        public R Post<T, R>(string relativeUri, T requestEntity)
        {
            return Post<T, R>(relativeUri, requestEntity, true);
        }

        public R Post<T, R>(string relativeUri, T requestEntity, bool useDataContractSerializer)
        {
            try
            {
              string uri = _baseUri + relativeUri; // GetUri(relativeUri);

                MemoryStream stream = Utility.SerializeToMemoryStream<T>(requestEntity, useDataContractSerializer);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);

                PrepareCredentials(request);

                request.Timeout = TIMEOUT;
                request.Method = "POST";
                request.ContentType = "text/xml";
                request.ContentLength = stream.Length;

                // allows for validation of SSL conversations
                ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(
                  ValidateRemoteCertificate
                );

                request.GetRequestStream().Write(stream.ToArray(), 0, (int)stream.Length);

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                R responseEntity = Utility.DeserializeFromStream<R>(response.GetResponseStream(), useDataContractSerializer);

                return responseEntity;
            }
            catch (Exception exception)
            {
                string uri = _baseUri + relativeUri;

                throw new Exception("Error while executing HTTP POST request on " + uri + ".", exception);
            }
        }

        public T PostMessage<T>(string relativeUri, string requestMessage, bool useDataContractSerializer)
        {
            try
            {
                string uri = _baseUri + relativeUri; // GetUri(relativeUri);
                MemoryStream stream = new MemoryStream();
                StreamWriter writer = new StreamWriter(stream);
                writer.Write(requestMessage);
                writer.Flush();
                byte[] bytes = stream.ToArray();

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);

                PrepareCredentials(request);
                
                request.Timeout = TIMEOUT;
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = bytes.Length;

                System.Net.ServicePointManager.Expect100Continue = false;

                // allows for validation of SSL conversations
                ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(
                  ValidateRemoteCertificate
                );

                Stream requestStream = request.GetRequestStream();
                requestStream.Write(bytes, 0, bytes.Length);
                requestStream.Flush();

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                T responseEntity = Utility.DeserializeFromStream<T>(response.GetResponseStream(), useDataContractSerializer);

                return responseEntity;
            }
            catch (Exception exception)
            {
                string uri = _baseUri + relativeUri;

                throw new Exception("Error while executing HTTP POST request on " + uri + ".", exception);
            }
        }

        public R PutMessage<T, R>(string relativeUri, T requestEntity, bool useDataContractSerializer)
        {
            try
            {
                string uri = _baseUri + relativeUri; // GetUri(relativeUri);

                MemoryStream stream = Utility.SerializeToMemoryStream<T>(requestEntity, useDataContractSerializer);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);

                PrepareCredentials(request);

                request.Timeout = TIMEOUT;
                request.Method = "PUT";
                request.ContentType = "text/xml";
                request.ContentLength = stream.Length;

                // allows for validation of SSL conversations
                ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(
                  ValidateRemoteCertificate
                );

                request.GetRequestStream().Write(stream.ToArray(), 0, (int)stream.Length);

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                R responseEntity = Utility.DeserializeFromStream<R>(response.GetResponseStream(), useDataContractSerializer);

                return responseEntity;
            }
            catch (Exception exception)
            {
                string uri = _baseUri + relativeUri;

                throw new Exception("Error while executing HTTP POST request on " + uri + ".", exception);
            }
        }

        public void PostMultipartMessage(string relativeUri, List<MultiPartMessage> requestMessages)
        {
            try
            {
                string uri = _baseUri + relativeUri;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.ContentType = "multipart/form-data; boundary=" + _boundary;
                request.Method = "POST";
                request.Timeout = TIMEOUT;
                MemoryStream stream = new MemoryStream();
                StreamWriter writer = new StreamWriter(stream);

                foreach (MultiPartMessage requestMessage in requestMessages)
                {
                  writer.Write("--" + _boundary + NEW_LINE);

                  if (requestMessage.type == MultipartMessageType.File)
                  {
                    writer.Write("Content-Disposition: file; name=\"{1}\"; filename=\"{2}\"{3}", requestMessage.mimeType, requestMessage.name, requestMessage.fileName, NEW_LINE);
                    writer.Write("Content-Type: {0}; {1}", requestMessage.mimeType, NEW_LINE);
                  }
                  else
                  {
                    writer.Write("Content-Disposition: form-data; name=\"{0}\"{1}", requestMessage.name, NEW_LINE);
                  }
                  writer.Flush();

                  writer.Write(NEW_LINE + requestMessage.message);
                  writer.Write(NEW_LINE);
                  writer.Write("--{0}--{1}", _boundary, NEW_LINE);
                  writer.Flush();
                }

                PrepareCredentials(request);

                request.ContentLength = stream.Length;
                request.GetRequestStream().Write(stream.ToArray(), 0, (int)stream.Length);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                stream.Close();

                string message = Utility.SerializeFromStream(response.GetResponseStream());

                //Handle the fact that dotNetRDF return 200 for parsing errors.
                if (response.StatusCode == HttpStatusCode.OK && message.StartsWith("Parsing Error"))
                {
                  throw new Exception(message);
                }
            }
            catch (Exception exception)
            {
                string uri = _baseUri + relativeUri;

                throw new Exception("Error while executing HTTP POST request on " + uri + ".", exception);
            }
        }

        public string Post<T>(string relativeUri, T requestEntity, bool useDataContractSerializer)
        {
            try
            {
                string uri = _baseUri + relativeUri; // GetUri(relativeUri);

                MemoryStream stream = Utility.SerializeToMemoryStream<T>(requestEntity, useDataContractSerializer);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);

                PrepareCredentials(request);

                request.Timeout = TIMEOUT;
                request.Method = "POST";
                request.ContentType = "text/xml";
                request.ContentLength = stream.Length;

                // allows for validation of SSL conversations
                ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(
                  ValidateRemoteCertificate
                );

                request.GetRequestStream().Write(stream.ToArray(), 0, (int)stream.Length);

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                string responseMessage = Utility.SerializeFromStream(response.GetResponseStream());

                return responseMessage;
            }
            catch (Exception exception)
            {
                string uri = _baseUri + relativeUri;

                throw new Exception("Error while executing HTTP POST request on " + uri + ".", exception);
            }
        }
    }
}
