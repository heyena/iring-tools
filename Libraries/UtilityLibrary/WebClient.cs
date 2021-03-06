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
using System.Text;
using System.ServiceModel.Web;
using log4net;

namespace org.iringtools.utility
{
    public class MultiPartMessage
    {
        public MultipartMessageType type { get; set; }
        public string name { get; set; }
        public object message { get; set; }
        public string fileName { get; set; }
        public string mimeType { get; set; }
    }

    public enum MultipartMessageType
    {
        File,
        FormData,
        Base64File,
        Data
    }

    public class WebHttpClient : IWebHttpClient
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(WebHttpClient));

        private string _boundary = Guid.NewGuid().ToString().Replace("-", "");
        private string _baseUri = String.Empty;
        private NetworkCredential _credentials = null;
        private IWebProxy _proxy = null;
        private string _proxyHost = null;
        private int _proxyPort = 8080;
        private string _proxyCredentialToken = null;
        private string _appKey = String.Empty;
        private string _accessToken = String.Empty;
        private string _contentType = String.Empty;
        private int _timeout = 300000;

        private const string NEW_LINE = "\r\n";
        private Encoding encoding = Encoding.UTF8;

        public IDictionary<string, string> Headers { get; set; }

        public bool DoSSOInjection { get; set; }

        public WebHttpClient(string baseUri)
        {
            _baseUri = baseUri;

            Headers = new Dictionary<string, string>();

            DoSSOInjection = true;

            object accessToken = ConfigurationManager.AppSettings["AccessToken"];
            if (accessToken != null)
                _accessToken = accessToken.ToString();

            object appKey = ConfigurationManager.AppSettings["AppKey"];
            if (appKey != null)
                _appKey = appKey.ToString();

            object proxyHost = ConfigurationManager.AppSettings["ProxyHost"];
            if (proxyHost != null)
                _proxyHost = proxyHost.ToString();

            object proxyPort = ConfigurationManager.AppSettings["ProxyPort"];
            if (proxyPort != null)
                int.TryParse(proxyPort.ToString(), out _proxyPort);

            object proxyCredentialToken = ConfigurationManager.AppSettings["ProxyCredentialToken"];
            if (proxyCredentialToken != null)
                _proxyCredentialToken = proxyCredentialToken.ToString();
        }

        public WebHttpClient(string baseUri, string userName, string password)
            : this(baseUri, userName, password, String.Empty)
        {
        }

        public int Timeout { get { return _timeout; } set { _timeout = value; } }

        public WebHttpClient(string baseUri, string userName, string password, string domain)
            : this(baseUri)
        {
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
            : this(baseUri)
        {
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
            : this(baseUri)
        {
            _credentials = credentials;

            if (proxyHost != String.Empty && proxyHost != null)
            {
                _proxy = new WebProxy(proxyHost, proxyPort);
                _proxy.Credentials = proxyCredentials;
            }
        }

        public WebHttpClient(string baseUri, NetworkCredential credentials, IWebProxy webProxy)
            : this(baseUri)
        {
            _credentials = credentials;
            _proxy = webProxy;
        }

        public WebHttpClient(string baseUri, NetworkCredential credentials)
            : this(baseUri)
        {
            _credentials = credentials;
        }

        public bool Async { get; set; }

        public string UserName { get; set; }

        public string AccessToken
        {
            get
            {
                try
                {
                    if (
                      WebOperationContext.Current != null &&
                      WebOperationContext.Current.IncomingRequest != null &&
                      WebOperationContext.Current.IncomingRequest.Headers != null &&
                      WebOperationContext.Current.IncomingRequest.Headers.Count > 0)
                    {
                        if (WebOperationContext.Current.IncomingRequest.Headers["Authorization"] != null)
                        {
                            _accessToken = WebOperationContext.Current.IncomingRequest.Headers["Authorization"];
                        }
                    }
                    else if (HttpContext.Current != null &&
                      HttpContext.Current.Request.Cookies != null &&
                      HttpContext.Current.Request.Cookies.Count > 0)
                    {
                        if (HttpContext.Current.Request.Cookies["Authorization"] != null)
                        {
                            HttpCookie authorizationCookie = HttpContext.Current.Request.Cookies["Authorization"];
                            _accessToken = authorizationCookie.Value;
                        }
                    }
                }
                catch (Exception)
                {
                    _logger.Warn("Unable to obtain authorization token from request headers.");
                }

                return _accessToken;
            }
            set
            {
                _accessToken = value;
            }
        }

        public string AppKey
        {
            get
            {
                try
                {
                    if (
                      WebOperationContext.Current != null &&
                      WebOperationContext.Current.IncomingRequest != null &&
                      WebOperationContext.Current.IncomingRequest.Headers.Count > 0)
                    {
                        if (WebOperationContext.Current.IncomingRequest.Headers["X-myPSN-AppKey"] != null &&
                            WebOperationContext.Current.IncomingRequest.Headers["X-myPSN-AppKey"] != String.Empty)
                        {
                            _appKey = WebOperationContext.Current.IncomingRequest.Headers["X-myPSN-AppKey"];
                        }
                    }
                    else if (HttpContext.Current != null && HttpContext.Current.Request.Cookies.Count > 0)
                    {
                        if (HttpContext.Current.Request.Cookies["X-myPSN-AppKey"] != null &&
                            HttpContext.Current.Request.Cookies["X-myPSN-AppKey"].ToString() != String.Empty)
                        {
                            HttpCookie appKeyCookie = HttpContext.Current.Request.Cookies["X-myPSN-AppKey"];
                            _appKey = appKeyCookie.Value;
                        }
                    }
                }
                catch (Exception)
                {
                    _logger.Warn("Unable to obtain application key from request headers.");
                }

                return _appKey;
            }
            set
            {
                _appKey = value;
            }
        }

        public string ContentType
        {
            get
            {
                return _contentType;
            }
            set
            {
                _contentType = value;
            }
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

        //TODO: delete hardcoded Authorization & AppKey
        private void PrepareHeaders(WebRequest request)
        {
            try
            {
                var ac = AccessToken;
                var ak = AppKey;

                if (DoSSOInjection && !string.IsNullOrEmpty(ac))
                {
                    _logger.Debug("Authorization: " + AccessToken);
                    request.Headers.Add("Authorization", AccessToken);
                }
                if (DoSSOInjection && !string.IsNullOrEmpty(ak))
                {
                    _logger.Debug("X-myPSN-AppKey: " + AppKey);
                    request.Headers.Add("X-myPSN-AppKey", AppKey);
                }

                request.Headers.Add("async", Async ? "True" : "False");

                if (!string.IsNullOrEmpty(UserName))
                {
                    _logger.Debug("UserName: " + UserName);
                    request.Headers.Add("UserName", UserName);
                }

                if (Headers != null)
                {
                    foreach (var pair in Headers)
                    {
                        request.Headers.Add(pair.Key, pair.Value);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error("Error preparing headers: " + e);
                throw e;
            }
        }

        private void PrepareCredentials(WebRequest request)
        {
            if (!string.IsNullOrEmpty(_proxyHost))
            {
                WebCredentials proxyCreds = new WebCredentials(_proxyCredentialToken);
                if (proxyCreds.isEncrypted) proxyCreds.Decrypt();

                WebProxy proxy = new WebProxy(_proxyHost, _proxyPort);
                proxy.Credentials = proxyCreds.GetNetworkCredential();
                request.Proxy = proxy;
            }

            if (_credentials == null)
            {
                request.Credentials = CredentialCache.DefaultCredentials;
                _logger.Debug("Use default credentials.");
            }
            else
            {
                request.Credentials = _credentials;
                _logger.Debug("Use saved credentials.");
            }

            if (_proxy != null)
            {
                request.Proxy = _proxy;
                _logger.Debug("Use passed in proxy.");
            }
            else if (!string.IsNullOrEmpty(_proxyHost))
            {
                _logger.Debug("Use proxy settings.");
                WebCredentials proxyCreds = new WebCredentials(_proxyCredentialToken);
                if (proxyCreds.isEncrypted) proxyCreds.Decrypt();

                WebProxy proxy = new WebProxy(_proxyHost, _proxyPort);
                proxy.Credentials = proxyCreds.GetNetworkCredential();
                request.Proxy = proxy;
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
                if (relativeUri.Contains(" ")) HttpUtility.HtmlEncode(relativeUri);

                string uri = _baseUri + relativeUri;
                _logger.Debug(string.Format("Performing GET request to URL: {0}.", uri));

                WebRequest request = HttpWebRequest.Create(uri);

                PrepareCredentials(request);
                PrepareHeaders(request);

                request.Method = "Get";
                request.Timeout = Timeout;

                // allows for validation of SSL conversations
                ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(
                  ValidateRemoteCertificate
                );

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                _logger.Debug("Response status code: " + response.StatusCode);
                _logger.Debug("Response content type: " + response.ContentType);
                _logger.Debug("Response content length: " + response.ContentLength);

                if (response.StatusCode == HttpStatusCode.Accepted)
                {
                    string statusUrl = response.Headers["location"];
                    return (T)Convert.ChangeType(statusUrl, typeof(T));
                }
                else
                {
                    Stream responseStream = response.GetResponseStream();

                    if (typeof(T) == typeof(String))
                    {
                        StreamReader reader = new StreamReader(responseStream);
                        string responseStr = reader.ReadToEnd();
                        reader.Close();

                        return (T)Convert.ChangeType(responseStr, typeof(T));
                    }
                    else
                    {
                        return Utility.DeserializeFromStream<T>(responseStream.ToMemoryStream(), useDataContractSerializer);
                    }
                }
            }
            catch (Exception exception)
            {
                string uri = _baseUri + relativeUri;
                throw new Exception("Error while executing HTTP GET request [" + uri + "].", exception);
            }
        }

        public Stream GetStream(string relativeUri)
        {
            string contentType;
            return GetStream(relativeUri, null, out contentType);
        }

        public Stream GetStream(string relativeUri, out string contentType)
        {
            return GetStream(relativeUri, null, out contentType);
        }

        private Stream GetStream(string relativeUri, string acceptType)
        {
            string contentType;
            return GetStream(relativeUri, acceptType, out contentType);
        }

        private Stream GetStream(string relativeUri, string acceptType, out string contentType)
        {
            try
            {
                string uri = _baseUri + relativeUri;
                _logger.Debug(string.Format("Getting stream from URL [{0}]...", uri));

                WebRequest request = HttpWebRequest.Create(uri);

                PrepareCredentials(request);
                PrepareHeaders(request);

                //Add Accept header
                if (!string.IsNullOrEmpty(acceptType))
                {
                    ((HttpWebRequest)request).Accept = acceptType;
                }

                request.Method = "Get";
                request.Timeout = Timeout;

                // allows for validation of SSL conversations
                ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(
                  ValidateRemoteCertificate
                );

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                contentType = response.ContentType;

                if (response.StatusCode == HttpStatusCode.Accepted)
                {
                    string statusUrl = response.Headers["location"];
                    byte[] byteArray = Encoding.ASCII.GetBytes(statusUrl);
                    return new MemoryStream(byteArray);
                }
                else
                {
                    Stream responseStream = response.GetResponseStream();
                    return responseStream.ToMemoryStream();
                }
            }
            catch (Exception e)
            {
                String error = String.Empty;

                _logger.Error(e.ToString());

                if (e.GetType() == typeof(WebException))
                {
                    HttpWebResponse response = ((HttpWebResponse)((WebException)e).Response);
                    Stream responseStream = response.GetResponseStream();
                    error = Utility.SerializeFromStream(responseStream);
                    if (error.Contains("<html>"))
                        error = error.Remove(error.IndexOf("<html>"));
                }
                else
                {
                    error = e.Message;
                }

                string uri = _baseUri + relativeUri;
                throw new Exception("Error getting data from URL [" + uri + "]. " + error);
            }
        }

        public String GetMessage(string relativeUri)
        {
            try
            {
                Stream stream = GetStream(relativeUri);
                string message = Utility.SerializeFromStream(stream);
                return message;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public String GetJson(string relativeUri)
        {
            try
            {
                Stream stream = GetStream(relativeUri, "application/json");
                string message = Utility.SerializeFromStream(stream);
                return message;
            }
            catch (Exception e)
            {
                throw e;
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
                string uri = _baseUri + relativeUri;
                _logger.Debug(string.Format("Performing POST to URL [{0}]...", uri));

                MemoryStream stream = Utility.SerializeToMemoryStream<T>(requestEntity, useDataContractSerializer);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);

                PrepareCredentials(request);
                PrepareHeaders(request);

                request.Timeout = Timeout;
                request.Method = "POST";
                request.ContentType = "text/xml";
                request.ContentLength = stream.Length;

                // allows for validation of SSL conversations
                ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(
                  ValidateRemoteCertificate
                );

                request.GetRequestStream().Write(stream.ToArray(), 0, (int)stream.Length);

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream responseStream = response.GetResponseStream();

                if (typeof(R) == typeof(String))
                {
                    StreamReader reader = new StreamReader(responseStream);
                    string responseStr = reader.ReadToEnd();
                    reader.Close();

                    return (R)Convert.ChangeType(responseStr, typeof(R));
                }
                else
                {
                    return Utility.DeserializeFromStream<R>(responseStream.ToMemoryStream(), useDataContractSerializer);
                }
            }
            catch (Exception e)
            {
                String error = String.Empty;

                if (e.GetType() == typeof(WebException))
                {
                    HttpWebResponse response = ((HttpWebResponse)((WebException)e).Response);
                    Stream responseStream = response.GetResponseStream();
                    error = Utility.SerializeFromStream(responseStream);
                }
                else
                {
                    error = e.ToString();
                }

                string uri = _baseUri + relativeUri;
                throw new Exception("Error with HTTP POST from URI [" + uri + "]. " + error);
            }
        }

        public R Post<T, R>(string relativeUri, T requestEntity, string format, bool useDataContractSerializer)
        {
            try
            {
                string uri = _baseUri + relativeUri;
                _logger.Debug(string.Format("Performing POST to URL [{0}]...", uri));

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);

                request.Timeout = Timeout;
                request.Method = "POST";

                MemoryStream stream = null;

                if (format == null || format.ToLower() == "xml")
                {
                    stream = Utility.SerializeToMemoryStream<T>(requestEntity, useDataContractSerializer);
                    request.ContentType = "text/xml";
                    request.ContentLength = stream.Length;
                }
                else if (format.ToLower() == "json")
                {
                    stream = Utility.SerializeToStreamJSON<T>(requestEntity, useDataContractSerializer);
                    request.ContentType = "application/json";
                    request.ContentLength = stream.Length;
                }
                else
                {
                    throw new Exception("Format " + format + " not allowed.");
                }

                PrepareCredentials(request);
                PrepareHeaders(request);

                // allows for validation of SSL conversations
                ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(
                  ValidateRemoteCertificate
                );

                request.GetRequestStream().Write(stream.ToArray(), 0, (int)stream.Length);

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode == HttpStatusCode.Accepted)
                {
                    string statusUrl = response.Headers["location"];
                    return (R)Convert.ChangeType(statusUrl, typeof(R));
                }
                else
                {
                    Stream responseStream = response.GetResponseStream();

                    if (typeof(R) == typeof(String))
                    {
                        StreamReader reader = new StreamReader(responseStream);
                        string responseStr = reader.ReadToEnd();
                        reader.Close();

                        return (R)Convert.ChangeType(responseStr, typeof(R));
                    }
                    else
                    {
                        return Utility.DeserializeFromStream<R>(responseStream.ToMemoryStream(), useDataContractSerializer);
                    }
                }
            }
            catch (Exception e)
            {
                String error = String.Empty;

                if (e.GetType() == typeof(WebException))
                {
                    HttpWebResponse response = ((HttpWebResponse)((WebException)e).Response);
                    Stream responseStream = response.GetResponseStream();
                    error = Utility.SerializeFromStream(responseStream);
                }
                else
                {
                    error = e.ToString();
                }

                string uri = _baseUri + relativeUri;
                throw new Exception("Error with HTTP POST from URI [" + uri + "]. " + error);
            }
        }

        public T PostMessage<T>(string relativeUri, string requestMessage, bool useDataContractSerializer)
        {
            try
            {
                string uri = _baseUri + relativeUri;
                _logger.Debug(string.Format("Performing POST to URL [{0}]...", uri));

                MemoryStream stream = new MemoryStream();
                StreamWriter writer = new StreamWriter(stream);
                writer.Write(requestMessage);
                writer.Flush();
                byte[] bytes = stream.ToArray();

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);

                PrepareCredentials(request);
                PrepareHeaders(request);

                request.Timeout = Timeout;
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
                Stream responseStream = response.GetResponseStream();

                T responseEntity = Utility.DeserializeFromStream<T>(responseStream, useDataContractSerializer);
                return responseEntity;
            }
            catch (Exception exception)
            {
                string uri = _baseUri + relativeUri;

                throw new Exception("Error while executing HTTP POST request on " + uri + ".", exception);
            }
        }

        public string PutStream(string relativeUri, Stream stream)
        {
            try
            {
                string uri = _baseUri + relativeUri;
                _logger.Debug(string.Format("Performing PUT to URL [{0}]...", uri));

                byte[] bytes = ((MemoryStream)stream).ToArray();

                if (String.IsNullOrEmpty(_contentType))
                {
                    _contentType = "application/octet-stream";
                }

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);

                PrepareCredentials(request);
                PrepareHeaders(request);

                request.Timeout = Timeout;
                request.Method = "PUT";
                request.ContentType = _contentType;
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

                string responseMessage = Utility.SerializeFromStream(response.GetResponseStream());

                return responseMessage;
            }
            catch (Exception exception)
            {
                string uri = _baseUri + relativeUri;

                throw new Exception("Error while executing HTTP PUT request on " + uri + ".", exception);
            }
        }

        public string PutJson(string relativeUri, string requestMessage)
        {
            try
            {
                string uri = _baseUri + relativeUri;
                _logger.Debug(string.Format("Performing PUT to URL [{0}]...", uri));

                MemoryStream stream = new MemoryStream();
                StreamWriter writer = new StreamWriter(stream);
                writer.Write(requestMessage);
                writer.Flush();
                byte[] bytes = stream.ToArray();

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);

                PrepareCredentials(request);
                PrepareHeaders(request);

                request.Timeout = Timeout;
                request.Method = "PUT";
                request.ContentType = "application/json";
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

                string responseMessage = Utility.SerializeFromStream(response.GetResponseStream());

                return responseMessage;
            }
            catch (Exception exception)
            {
                string uri = _baseUri + relativeUri;

                throw new Exception("Error while executing HTTP POST request on " + uri + ".", exception);
            }
        }

        public string PostStream(string relativeUri, Stream stream)
        {
            try
            {
                string uri = _baseUri + relativeUri;
                _logger.Debug(string.Format("Performing POST stream to URL [{0}]...", uri));

                byte[] bytes = ((MemoryStream)stream).ToArray();
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);

                PrepareCredentials(request);
                PrepareHeaders(request);

                request.Timeout = Timeout;
                request.Method = "POST";
                request.ContentType = "application/octet-stream";
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
                string responseMessage;

                if (response.StatusCode == HttpStatusCode.Accepted)
                {
                    responseMessage = response.Headers["location"];
                }
                else
                {
                    Stream responseStream = response.GetResponseStream();
                    responseMessage = Utility.SerializeFromStream(responseStream);
                }

                return responseMessage;
            }
            catch (Exception exception)
            {
                string uri = _baseUri + relativeUri;

                throw new Exception("Error while executing HTTP POST request on " + uri + ".", exception);
            }
        }

        /// <summary>
        ///  calling the service to post the content
        /// </summary>
        /// <param name="relativeUri">relative uri of service</param>
        /// <param name="stream">stream of file</param>
        /// <param name="filename">file name</param>
        /// <returns>response</returns>
        public string PostStreamUpload(string relativeUri, Stream stream, string filename)
        {
            try
            {
                string uri = _baseUri + relativeUri;
                _logger.Debug(string.Format("Performing POST stream to URL [{0}]...", uri));

                System.Net.ServicePointManager.Expect100Continue = false;

                // allows for validation of SSL conversations
                ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(
                  ValidateRemoteCertificate
                );

                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
                request.Method = "POST";
                request.ContentType = "application/octet-stream";
                request.KeepAlive = false;
                request.Timeout = Timeout;


                request.Headers.Add("FileName", filename);
                Stream serverStream = request.GetRequestStream();
                stream.Seek(0, SeekOrigin.Begin);

                const int bufferSize = 65536; // 64K

                byte[] buffer = new byte[bufferSize];
                int bytesRead = 0;
                while ((bytesRead = stream.Read(buffer, 0, bufferSize)) > 0)
                {
                    serverStream.Write(buffer, 0, bytesRead);
                }
                stream.Close();
                serverStream.Close();


                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                string responseMessage;


                if (response.StatusCode == HttpStatusCode.Accepted)
                {
                    responseMessage = response.Headers["location"];
                }
                else
                {
                    Stream responseStream = response.GetResponseStream();
                    responseMessage = Utility.SerializeFromStream(responseStream);
                }

                return responseMessage;
            }
            catch (Exception exception)
            {
                string uri = _baseUri + relativeUri;

                throw new Exception("Error while executing HTTP POST request on " + uri + ".", exception);
            }
        }

        public string PostMessage(string relativeUri, string requestMessage, bool useDataContractSerializer)
        {
            try
            {
                string uri = _baseUri + relativeUri;
                _logger.Debug(string.Format("Performing POST message to URL [{0}]...", uri));

                MemoryStream stream = new MemoryStream();
                StreamWriter writer = new StreamWriter(stream);
                writer.Write(requestMessage);
                writer.Flush();
                byte[] bytes = stream.ToArray();

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);

                PrepareCredentials(request);
                PrepareHeaders(request);

                request.Timeout = Timeout;
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

                string responseMessage = Utility.SerializeFromStream(response.GetResponseStream());

                return responseMessage;
            }
            catch (Exception exception)
            {
                string uri = _baseUri + relativeUri;

                throw new Exception("Error while executing HTTP POST request on " + uri + ".", exception);
            }
        }

        public string PostJson(string relativeUri, string requestMessage)
        {
            try
            {
                string uri = _baseUri + relativeUri;
                _logger.Debug(string.Format("Performing POST json to URL [{0}]...", uri));

                MemoryStream stream = new MemoryStream();
                StreamWriter writer = new StreamWriter(stream);
                writer.Write(requestMessage);
                writer.Flush();
                byte[] bytes = stream.ToArray();

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);

                PrepareCredentials(request);
                PrepareHeaders(request);

                request.Timeout = Timeout;
                request.Method = "POST";
                request.ContentType = "application/json";
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

                string responseMessage = Utility.SerializeFromStream(response.GetResponseStream());

                return responseMessage;
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
                string uri = _baseUri + relativeUri;
                _logger.Debug(string.Format("Performing PUT message to URL [{0}]...", uri));

                MemoryStream stream = Utility.SerializeToMemoryStream<T>(requestEntity, useDataContractSerializer);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);

                PrepareCredentials(request);
                PrepareHeaders(request);

                request.Timeout = Timeout;
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

        //public void PostMultipartMessage(string relativeUri, List<MultiPartMessage> requestMessages)
        //{
        //  try
        //  {
        //    // Encoding encoding = Encoding.UTF8;
        //    string uri = _baseUri + relativeUri;
        //    _logger.Debug(string.Format("Performing POST multipart message to URL [{0}]...", uri));

        //    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
        //    request.ContentType = "multipart/form-data; boundary=" + _boundary;
        //    request.Method = "POST";
        //    request.Timeout = Timeout;
        //    MemoryStream stream = new MemoryStream();
        //    string header = string.Empty;

        //    foreach (MultiPartMessage requestMessage in requestMessages)
        //    {
        //      if (requestMessage.type == MultipartMessageType.File)
        //      {
        //        header = string.Format("--{0}\r\nContent-Disposition: file; name=\"{1}\"; filename=\"{2}\";\r\nContent-Type: {3}\r\n\r\n", _boundary, requestMessage.name, requestMessage.fileName, requestMessage.mimeType);
        //        stream.Write(encoding.GetBytes(header), 0, header.Length);

        //        //byte[] fileData = (byte[])requestMessage.message;
        //        //stream.Write(fileData, 0, fileData.Length);

        //        Stream fileData = (Stream)requestMessage.message;
        //        fileData.CopyTo(stream);
        //        //add linefeed to enable multiple posts
        //        stream.Write(encoding.GetBytes("\r\n"), 0, 2);
        //      }
        //      else
        //      {
        //        header = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}\r\n", _boundary, requestMessage.name, requestMessage.message);
        //        stream.Write(encoding.GetBytes(header), 0, header.Length);
        //      }

        //    }

        //    header = string.Format("--{0}--\r\n", _boundary);
        //    stream.Write(encoding.GetBytes(header), 0, header.Length);

        //    PrepareCredentials(request);
        //    PrepareHeaders(request);

        //    request.ContentLength = stream.Length;
        //    request.GetRequestStream().Write(stream.ToArray(), 0, (int)stream.Length);
        //    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        //    stream.Close();

        //    string message = Utility.SerializeFromStream(response.GetResponseStream());
        //    //Handle the fact that dotNetRDF return 200 for parsing errors.
        //    if (response.StatusCode == HttpStatusCode.OK && message.StartsWith("Parsing Error"))
        //    {
        //      throw new Exception(message);
        //    }
        //  }
        //  catch (Exception exception)
        //  {
        //    string uri = _baseUri + relativeUri;

        //    throw new Exception("Error while executing HTTP POST request on " + uri + ".", exception);
        //  }
        //}

        public void PostMultipartMessage(string relativeUri, List<MultiPartMessage> requestMessages, ref string response)
        {
            try
            {
                // Encoding encoding = Encoding.UTF8;
                string uri = _baseUri + relativeUri;
                _logger.Debug(string.Format("Performing POST multipart message to URL [{0}]...", uri));

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.AllowWriteStreamBuffering = false;
                request.Method = "POST";
                request.Timeout = Timeout;
                MemoryStream stream = new MemoryStream();
                string header = String.Empty;
                bool isFormData = false;
                foreach (MultiPartMessage requestMessage in requestMessages)
                {
                    Stream fileData = null;
                    switch (requestMessage.type)
                    {
                        case MultipartMessageType.File:
                            header = string.Format("--{0}\r\nContent-Disposition: file; name=\"{1}\"; filename=\"{2}\";\r\nContent-Type: {3}\r\n\r\n", _boundary, requestMessage.name, requestMessage.fileName, requestMessage.mimeType);
                            stream.Write(encoding.GetBytes(header), 0, header.Length);

                            fileData = (Stream)requestMessage.message;
                            fileData.CopyTo(stream);
                            //add linefeed to enable multiple posts
                            stream.Write(encoding.GetBytes("\r\n"), 0, 2);
                            break;

                        case MultipartMessageType.FormData:
                            header = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}\r\n", _boundary, requestMessage.name, requestMessage.message);
                            stream.Write(encoding.GetBytes(header), 0, header.Length);
                            isFormData = true;
                            break;

                        case MultipartMessageType.Base64File:
                            header = string.Format("--{0}\r\nX-Filename: {1}\r\n\r\n{2}\r\n", _boundary, requestMessage.fileName, requestMessage.message);
                            stream.Write(encoding.GetBytes(header.ToString()), 0, header.Length);
                            break;

                        case MultipartMessageType.Data:
                            header = string.Format("--{0}\r\n\r\n{1}\r\n", _boundary, requestMessage.message);
                            stream.Write(encoding.GetBytes(header), 0, header.Length);
                            break;
                    }
                }

                header = string.Format("--{0}--\r\n", _boundary);
                stream.Write(encoding.GetBytes(header), 0, header.Length);

                if (isFormData)
                {
                    request.ContentType = "multipart/form-data; boundary=" + _boundary;
                }
                else
                {
                    request.ContentType = "multipart/mixed; boundary=" + _boundary;
                }

                PrepareCredentials(request);
                PrepareHeaders(request);
                
                request.ContentLength = stream.Length;
                request.GetRequestStream().Write(stream.ToArray(), 0, (int)stream.Length);

                HttpWebResponse httpResponse = null;
                try
                {
                    httpResponse = (HttpWebResponse)request.GetResponse();

                    response = Utility.SerializeFromStream(httpResponse.GetResponseStream());
                }
                catch (WebException e)
                {
                    httpResponse = (HttpWebResponse)e.Response;

                    response = Utility.SerializeFromStream(httpResponse.GetResponseStream());

                    throw e;
                }

                stream.Close();
            }
            catch (Exception exception)
            {
                string uri = _baseUri + relativeUri;

                throw new Exception("Error while executing HTTP POST request on " + uri + ".", exception);
            }
        }

        public string ForwardPost(string relativeUri, HttpRequestBase requestBase)
        {
            string uri = _baseUri + relativeUri;
            _logger.Debug(string.Format("Performing forward POST to URL [{0}]...", uri));

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            Stream webStream = null;

            try
            {
                request.ContentType = requestBase.ContentType;
                request.ContentLength = requestBase.ContentLength;
                request.Method = "POST";
                request.Timeout = Timeout;

                PrepareCredentials(request);
                PrepareHeaders(request);

                webStream = request.GetRequestStream();
                requestBase.InputStream.CopyTo(webStream);

            }
            catch (Exception exception)
            {
                throw new Exception("Error while executing HTTP POST request on " + uri + ".", exception);
            }
            finally
            {
                if (null != webStream)
                {
                    webStream.Flush();
                    webStream.Close();    // might need additional exception handling here
                }
            }

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                return Utility.SerializeFromStream(response.GetResponseStream());
            }
            catch (Exception exception)
            {
                throw new Exception("Error while executing HTTP POST request on " + uri + ".", exception);
            }

        }

        public string Post<T>(string relativeUri, T requestEntity, bool useDataContractSerializer)
        {
            try
            {
                string uri = _baseUri + relativeUri;
                _logger.Debug(string.Format("Performing POST to URL [{0}]...", uri));

                MemoryStream stream = Utility.SerializeToMemoryStream<T>(requestEntity, useDataContractSerializer);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);

                PrepareCredentials(request);
                PrepareHeaders(request);

                request.Timeout = Timeout;
                request.Method = "POST";
                request.ContentType = "application/xml";
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

        public void PostMultipartMessage(string relativeUri, List<org.iringtools.utility.MultiPartMessage> requestMessages)
        {
            throw new NotImplementedException();
        }
    }
}