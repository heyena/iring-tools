// Copyright (c) 2009, ids-adi.org /////////////////////////////////////////////
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Web;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Linq;
using org.ids_adi.qxf;
using org.w3.sparql_results;
using org.iringtools.library;
using org.iringtools.utility;

namespace DemoControlPanel.Web
{
    // NOTE: If you change the class name "DemoControlPanelService" here, you must also update the reference to "DemoControlPanelService" in Web.config.
    public class DemoControlPanelService : IDemoControlPanelService
    {
        private const string QXF_TO_RDF_FILENAME = "QXFToRDF.xsl";
        private const string SERVICE_URIS_FILENAME = "ServiceUris.xml";
        
        private string _xmlPath = String.Empty;
        private string _serviceUrisPath = String.Empty;
        private string _qxfToRDFPath = String.Empty;
        WebProxyCredentials _proxyCredentials = null;

        ConfiguredEndpoints _config;

        public DemoControlPanelService()
        {
            Directory.SetCurrentDirectory(System.AppDomain.CurrentDomain.BaseDirectory);

            _xmlPath = System.Configuration.ConfigurationManager.AppSettings["XmlPath"];
            _serviceUrisPath = _xmlPath + SERVICE_URIS_FILENAME;
            _qxfToRDFPath = _xmlPath + QXF_TO_RDF_FILENAME;

            string encryptedProxyToken = System.Configuration.ConfigurationManager.AppSettings["ProxyCredentialToken"];
            string proxyHost = System.Configuration.ConfigurationManager.AppSettings["ProxyHost"];
            string proxyPortString = System.Configuration.ConfigurationManager.AppSettings["ProxyPort"];
            int proxyPort = 0;
            Int32.TryParse(proxyPortString, out proxyPort);
            if (encryptedProxyToken == String.Empty)
            {
                _proxyCredentials = new WebProxyCredentials();
            }
            else
            {
                _proxyCredentials = new WebProxyCredentials(encryptedProxyToken, proxyHost, proxyPort);
                _proxyCredentials.Decrypt();
            }
        }

        public ConfiguredEndpoints GetConfiguredEndpoints()
        {
            try
            {
                _config = Utility.Read<ConfiguredEndpoints>(_xmlPath + "ConfiguredEndpoints.xml");

                return _config;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while getting adapter list.", ex.InnerException);
            }
        }

        public Response GetManifest(iRINGEndpoint endpoint, string projectName, string applicationName)
        {
            Response response = new Response();
            string relativeUri = String.Format("/{0}/{1}/manifest", projectName, applicationName);
            try
            {
                //this is adapter service uri
                string adapterServiceUri = endpoint.serviceUri;

                WebCredentials credentials = endpoint.credentials;
                if (credentials == null) credentials = new WebCredentials();
                if (credentials.isEncrypted) credentials.Decrypt();

                WebHttpClient webHttpClient = new WebHttpClient(adapterServiceUri, credentials.GetNetworkCredential(), _proxyCredentials.GetWebProxy());
                Manifest manifest = webHttpClient.Get<Manifest>(relativeUri, true);
                foreach (ManifestGraph manifestGraph in manifest.Graphs)
                {
                    response.Add(manifestGraph.GraphName);
                }
                response.Add("All");
            }
            catch (Exception ex)
            {
                response.Add(ex.ToString());
            }
            return response;
        }

        public Response GetSenderManifest(iRINGEndpoint endpoint, string projectName, string applicationName)
        {
            return GetManifest(endpoint, projectName, applicationName);
        }

        public Response GetReceiverManifest(iRINGEndpoint endpoint, string projectName, string applicationName)
        {
            return GetManifest(endpoint, projectName, applicationName);
        }
                
        public Response Refresh(iRINGEndpoint endpoint, string projectName, string applicationName, string graphName, string identifier)
        {
            string relativeUri = String.Format("/{0}/{1}", projectName, applicationName);
            Response response = new Response();

            string adapterServiceUri = endpoint.serviceUri;

            WebCredentials credentials = endpoint.credentials;
            if (credentials == null) credentials = new WebCredentials();
            if (credentials.isEncrypted) credentials.Decrypt();
                        
            if (graphName != "All" && identifier.Length == 0)
            {
                relativeUri += "/" + graphName + "/refresh";
            }
            else if (graphName != "All" && identifier.Length > 0)
            {
                relativeUri += "/" + graphName + "/" + identifier + "/refresh";
            }
            else
            {
                relativeUri += "/refresh";
            }

            try
            {
                WebHttpClient webHttpClient = new WebHttpClient(adapterServiceUri, credentials.GetNetworkCredential(), _proxyCredentials.GetWebProxy());
                response = webHttpClient.Get<Response>(relativeUri, true);
            }
            catch (Exception ex)
            {
                response.Add(ex.Message);
            }

            return response;
        }

        public Response Reset(iRINGEndpoint endpoint, string projectName, string applicationName)
        {
            string relativeUri = String.Format("/{0}/{1}/reset", projectName, applicationName);
            Response response = new Response();

            string adapterServiceUri = endpoint.serviceUri;

            WebCredentials credentials = endpoint.credentials;
            if (credentials == null) credentials = new WebCredentials();
            if (credentials.isEncrypted) credentials.Decrypt();
                        
            try
            {
                WebHttpClient webHttpClient = new WebHttpClient(adapterServiceUri, credentials.GetNetworkCredential(), _proxyCredentials.GetWebProxy());
                response = webHttpClient.Get<Response>(relativeUri, true);
            }
            catch (Exception ex)
            {
                response.Add(ex.Message);
            }
            return response;
        }

        public Response Pull(iRINGEndpoint endpoint, iRINGEndpoint targetEnpoint, string projectName, string applicationName, string graphName)
        {
            string relativeUri = String.Format("/{0}/{1}/pull", projectName, applicationName);
            
            Response response = new Response();
            try
            {
                WebCredentials credentials = endpoint.credentials;
                if (credentials == null) credentials = new WebCredentials();
                if (credentials.isEncrypted) credentials.Decrypt();

                WebCredentials targetCredentials = targetEnpoint.credentials;
                if (targetCredentials == null) targetCredentials = new WebCredentials();
                if (targetCredentials.isEncrypted) targetCredentials.Decrypt();
                string targetCredentialsXML = Utility.Serialize<WebCredentials>(targetCredentials, true);
                //endpoint is adapter service uri
                WebHttpClient client = new WebHttpClient(endpoint.serviceUri, credentials.GetNetworkCredential(), _proxyCredentials.GetWebProxy());
                Request request = new Request();
                //This is interface service uri
                request.Add("targetUri", targetEnpoint.serviceUri);
                request.Add("targetCredentials", targetCredentialsXML);                
                request.Add("graphName", graphName);
                request.Add("filter", "");
                response = client.Post<Request, Response>(relativeUri, request, true);

            }
            catch (Exception ex)
            {
                response.Add("Error pulling data from server " + endpoint.serviceUri + " " + ex.Message);
            }
            return response;
        }

        public Response Export(iRINGEndpoint endpoint, string projectName, string applicationName, string graphName)
        {
            string relativeUri = String.Format("/{0}/{1}", projectName, applicationName);
            Response response = new Response();
            QXF qxf = null;

            string adapterServiceUri = endpoint.serviceUri;

            WebCredentials credentials = endpoint.credentials;
            if (credentials == null) credentials = new WebCredentials();
            if (credentials.isEncrypted) credentials.Decrypt();

            relativeUri += "/" + graphName;
            try
            {
                WebHttpClient webHttpClient = new WebHttpClient(adapterServiceUri, credentials.GetNetworkCredential(), _proxyCredentials.GetWebProxy());
                qxf = webHttpClient.Get<QXF>(relativeUri, false);
                response.Add(graphName + " RDF Exported Successfully");
            }
            catch (Exception ex)
            {
                response.Add(ex.Message);
            }
            return response;
        }

        public Response Import(iRINGEndpoint endpoint, string projectName, string applicationName)
        {
            string relativeUri = String.Format("/{0}/{1}", projectName, applicationName);
            Response response = new Response();

            string adapterServiceUri = endpoint.serviceUri;

            WebCredentials credentials = endpoint.credentials;
            if (credentials == null) credentials = new WebCredentials();
            if (credentials.isEncrypted) credentials.Decrypt();

            relativeUri = "/import";
            try
            {
                WebHttpClient webHttpClient = new WebHttpClient(adapterServiceUri, credentials.GetNetworkCredential(), _proxyCredentials.GetWebProxy());
                response = webHttpClient.Get<Response>(relativeUri, true);
            }
            catch (Exception ex)
            {
                response.Add(ex.Message);
            }
            return response;
        }
        
        public Collection<Collection<SPARQLBinding>> Query(iRINGEndpoint endpoint, string projectName, string applicationName, string query)
        {
            Collection<Collection<SPARQLBinding>> list = new Collection<Collection<SPARQLBinding>>();

            string relativeUri = "/sparql";

            //this is interface service uri
            string interfaceServiceUri = endpoint.serviceUri;

            WebCredentials credentials = endpoint.credentials;
            if (credentials == null) credentials = new WebCredentials();
            if (credentials.isEncrypted) credentials.Decrypt();

            WebHttpClient webHttpClient = new WebHttpClient(interfaceServiceUri, credentials.GetNetworkCredential(), _proxyCredentials.GetWebProxy());

            string message = "query=" + HttpUtility.UrlEncode(query);
            SPARQLResults sparqlResults = webHttpClient.PostMessage<SPARQLResults>(relativeUri, message, false);

            foreach (SPARQLResult sparqlResult in sparqlResults.resultsElement.results)
            {
                Collection<SPARQLBinding> bindings = new Collection<SPARQLBinding>();
                foreach (SPARQLBinding binding in sparqlResult.bindings)
                {
                    bindings.Add(binding);
                }
                list.Add(bindings);
            }

            return list;
        }

        public Response Update(iRINGEndpoint endpoint, string projectName, string applicationName, string query)
        {
            Response response = new Response();
            try
            {
                string result = string.Empty;
                string relativeUri = String.Format("/{0}/{1}/sparql", projectName, applicationName);

                //this is interface service uri
                string interfaceServiceUri = endpoint.serviceUri;

                WebCredentials credentials = endpoint.credentials;
                if (credentials == null) credentials = new WebCredentials();
                if (credentials.isEncrypted) credentials.Decrypt();

                MultiPartMessage requestMessage = new MultiPartMessage
                {
                    name = "update",
                    type = MultipartMessageType.FormData,
                    message = query,
                };

                List<MultiPartMessage> requestMessages = new List<MultiPartMessage>
                {
                    requestMessage
                };

                WebHttpClient webHttpClient = new WebHttpClient(interfaceServiceUri, credentials.GetNetworkCredential(), _proxyCredentials.GetWebProxy());
                webHttpClient.PostMultipartMessage(relativeUri, requestMessages);
                response.Add("SPARQL update completed ......");
            }
            catch (Exception ex)
            {
                response.Add("SPARQL update failed.......");
                response.Add(ex.Message);
            }
            return response;
        }

        public Response GetQXF(Stream stream)
        {

            Response response = new Response();
            try
            {
                XmlReader xr = XmlReader.Create(stream);
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(xr);
                QXF qxf = Utility.Deserialize<QXF>(xDoc.InnerXml, false);
                XmlDocument rdf = Utility.Transform<QXF, XmlDocument>(qxf, _qxfToRDFPath, false);
                response.Add(rdf.InnerXml);
            }
            catch (Exception ex)
            {
                response.Add(ex.InnerException.Message);
            }
            return response;
        }

        public Collection<ScopeProject> GetScopes(iRINGEndpoint endpoint)
        {
            string relativeUri = "/scopes";
            Collection<ScopeProject> scopes = new Collection<ScopeProject>();

            string adapterServiceUri = endpoint.serviceUri;

            WebCredentials credentials = endpoint.credentials;
            if (credentials == null) credentials = new WebCredentials();
            if (credentials.isEncrypted) credentials.Decrypt();

            try
            {
                WebHttpClient webHttpClient = new WebHttpClient(adapterServiceUri, credentials.GetNetworkCredential(), _proxyCredentials.GetWebProxy());
                scopes = webHttpClient.Get<Collection<ScopeProject>>(relativeUri, true);
            }
            catch (Exception ex)
            {
                //response.Add(ex.Message);
            }
            return scopes;       
        }

        public Collection<ScopeProject> GetInterfaceScopes(iRINGEndpoint endpoint)
        {
            return GetScopes(endpoint);
        }

    }
}
