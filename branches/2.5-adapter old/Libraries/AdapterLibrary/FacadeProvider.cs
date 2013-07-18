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
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL + exEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Xml.Linq;
using log4net;
using Ninject;
using Ninject.Extensions.Xml;
using org.iringtools.library;
using org.iringtools.utility;
using org.iringtools.adapter;
using StaticDust.Configuration;
using VDS.RDF.Query;
using org.iringtools.mapping;
using System.ServiceModel;
using System.Security.Principal;
using org.iringtools.adapter.identity;
using System.Collections;


namespace org.iringtools.facade
{
  public class FacadeProvider : BaseProvider
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(FacadeProvider));
    private IDataLayer2 _dataLayer = null;
    private ISemanticLayer _semanticEngine = null;
    private GraphMap _graphMap = null;
    private List<IDataObject> _dataObjects = new List<IDataObject>();
    private IProjectionLayer _projectionEngine = null;

    [Inject]
    public FacadeProvider(NameValueCollection settings) : base(settings) {}
    
    public Response Delete(string scope, string app, string graph)
    {
      Response response = new Response();
      response.Level = StatusLevel.Success;

      Status status = new Status();
      status.Messages = new Messages();

      try
      {
        status.Identifier = String.Format("{0}.{1}.{2}", scope, app, graph);

        InitializeScope(scope, app);
        _semanticEngine = _kernel.Get<ISemanticLayer>("dotNetRDF");
        response.Append(_semanticEngine.Delete(graph));
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error deleting {0} graphs: {1}", graph, ex));

        status.Level = StatusLevel.Error;
        status.Messages.Add(string.Format("Error deleting all graphs: {0}", ex));
      }

      response.Append(status);
      return response;
    }

    public Response Pull(string scope, string app, string graph, Request request)
    {
      Response response = new Response();
      response.Level = StatusLevel.Success;

      Status status = new Status();
      status.Messages = new Messages();

      try
      {
        status.Identifier = String.Format("{0}.{1}", scope, app);

        InitializeScope(scope, app);

        if (_settings["ReadOnlyDataLayer"] != null && _settings["ReadOnlyDataLayer"].ToString().ToLower() == "true")
        {
          string message = "Can not perform post on read-only data layer of [" + scope + "." + app + "].";
          _logger.Error(message);

          status.Level = StatusLevel.Error;
          status.Messages.Add(message);
        }
        else
        {
          InitializeDataLayer();

          DateTime startTime = DateTime.Now;

          #region move this portion to dotNetRdfEngine?
          if (!request.ContainsKey("targetEndpointUri"))
            throw new Exception("Target Endpoint Uri is required");

          string targetEndpointUri = request["targetEndpointUri"];

          if (!request.ContainsKey("targetGraphBaseUri"))
            throw new Exception("Target graph uri is required");

          string targetGraphBaseUri = request["targetGraphBaseUri"];
          _settings["TargetGraphBaseUri"] = targetGraphBaseUri;

          if (targetGraphBaseUri.ToLower() == "[default graph]")
            targetGraphBaseUri = String.Empty;

          SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri(targetEndpointUri), targetGraphBaseUri);

          if (request.ContainsKey("targetCredentials"))
          {
            string targetCredentialsXML = request["targetCredentials"];
            WebCredentials targetCredentials = Utility.Deserialize<WebCredentials>(targetCredentialsXML, true);

            if (targetCredentials.isEncrypted)
              targetCredentials.Decrypt();

            endpoint.SetCredentials(targetCredentials.GetNetworkCredential().UserName, targetCredentials.GetNetworkCredential().Password, targetCredentials.GetNetworkCredential().Domain);
          }

          string proxyHost = _settings["ProxyHost"];
          string proxyPort = _settings["ProxyPort"];
          string proxyCredsToken = _settings["ProxyCredentialToken"];

          if (!String.IsNullOrEmpty(proxyHost) && !String.IsNullOrEmpty(proxyPort) && !String.IsNullOrEmpty(proxyCredsToken))
          {
            WebProxyCredentials proxyCreds = _settings.GetWebProxyCredentials();
            endpoint.Proxy = proxyCreds.GetWebProxy() as WebProxy;
            endpoint.ProxyCredentials = proxyCreds.GetNetworkCredential();
          }

          VDS.RDF.IGraph resultGraph = endpoint.QueryWithResultGraph("CONSTRUCT {?s ?p ?o} WHERE {?s ?p ?o}");          
          #endregion

          if (resultGraph != null && resultGraph.Triples.Count > 0)
          {
            // call RdfProjectionEngine to fill data objects from a given graph
            _projectionEngine = _kernel.Get<IProjectionLayer>("rdf");

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            TextWriter textWriter = new StringWriter(sb);
            VDS.RDF.Writing.RdfXmlWriter rdfWriter = new VDS.RDF.Writing.RdfXmlWriter();
            rdfWriter.Save(resultGraph, textWriter);
            XDocument xDocument = XDocument.Parse(sb.ToString());

            if (xDocument != null && xDocument.Root != null)
            {
              _logger.Debug(xDocument.Root.ToString());
              _dataObjects = _projectionEngine.ToDataObjects(graph, ref xDocument);

              if (_dataObjects != null && _dataObjects.Count > 0)
              {
                status.Messages.Add("Query target endpoint completed successfully.");
                status.Messages.Add(String.Format("Number of data objects created [{0}].", _dataObjects.Count));
                
                // post data objects to data layer
                response.Append(_dataLayer.Post(_dataObjects));

                DateTime endTime = DateTime.Now;
                TimeSpan duration = endTime.Subtract(startTime);

                status.Messages.Add(String.Format("Execution time [{0}:{1}.{2}] minutes.",
                  duration.Minutes, duration.Seconds, duration.Milliseconds));
              }
              else
              {
                status.Messages.Add(string.Format("No data objects being created."));
              }
            }
            else
            {
              throw new Exception("Facade document is empty.");
            }
          }
          else
          {
            throw new Exception("Facade graph is empty.");
          }
        }
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Pull(): ", ex);

        status.Level = StatusLevel.Error;
        status.Messages.Add(string.Format("Error pulling graph: {0}", ex));
      }

      response.Append(status);
      return response;
    }

    public Response Refresh(string scope, string app, string graph)
    {
      Response response = new Response();
      response.Level = StatusLevel.Success;

      Status status = new Status();
      status.Messages = new Messages();

      try
      {
        status.Identifier = String.Format("{0}.{1}", scope, app);

        InitializeScope(scope, app);
        InitializeDataLayer();

        response.Append(Refresh(graph));
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in Refresh: {0}", ex));

        status.Level = StatusLevel.Error;
        status.Messages.Add(string.Format("Error refreshing graph [{0}]: {1}", graph, ex));
      }

      response.Append(status);
      return response;
    }

    private Response Refresh(string graphName)
    {
      _semanticEngine = _kernel.Get<ISemanticLayer>("dotNetRDF");
      _projectionEngine = _kernel.Get<IProjectionLayer>("rdf");

      LoadDataObjectSet(graphName, null);
      XDocument rdf = _projectionEngine.ToXml(graphName, ref _dataObjects);

      return _semanticEngine.Refresh(graphName, rdf);
    }

    public Response RefreshAll(string projectName, string applicationName)
    {
      Response response = new Response();
      response.Level = StatusLevel.Success;

      Status status = new Status();
      status.Messages = new Messages();

      try
      {
        status.Identifier = String.Format("{0}.{1}", projectName, applicationName);

        InitializeScope(projectName, applicationName);
        InitializeDataLayer();

        DateTime start = DateTime.Now;

        foreach (GraphMap graphMap in _mapping.graphMaps)
        {
          response.Append(Refresh(graphMap.name));
        }

        DateTime end = DateTime.Now;
        TimeSpan duration = end.Subtract(start);

        status.Messages.Add(String.Format("RefreshAll() completed in [{0}:{1}.{2}] minutes.",
          duration.Minutes, duration.Seconds, duration.Milliseconds));
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error in RefreshAll: {0}", ex));

        status.Level = StatusLevel.Error;
        status.Messages.Add(string.Format("Error refreshing all graphs: {0}", ex));
      }

      response.Append(status);
      return response;
    }

    private void LoadDataObjectSet(string graphName, List<string> identifiers)
    {
      _graphMap = _mapping.FindGraphMap(graphName);

      _dataObjects.Clear();

      DataObject objectType = _dictionary.dataObjects.Find(
        x => x.objectName.ToLower() == _graphMap.dataObjectName.ToLower());

      if (identifiers != null)
        _dataObjects = _dataLayerGateway.Get(objectType, identifiers);
      else
        _dataObjects = _dataLayerGateway.Get(objectType, null);
    }

    public Response DeleteAll(string projectName, string applicationName)
    {
      Response response = new Response();
      response.Level = StatusLevel.Success;

      Status status = new Status();
      status.Messages = new Messages();

      try
      {
        status.Identifier = String.Format("{0}.{1}", projectName, applicationName);

        InitializeScope(projectName, applicationName);

        _semanticEngine = _kernel.Get<ISemanticLayer>("dotNetRDF");

        foreach (GraphMap graphMap in _mapping.graphMaps)
        {
          response.Append(_semanticEngine.Delete(graphMap.name));
        }
      }
      catch (Exception ex)
      {
        _logger.Error(string.Format("Error deleting all graphs: {0}", ex));

        status.Level = StatusLevel.Error;
        status.Messages.Add(string.Format("Error deleting all graphs: {0}", ex));
      }

      response.Append(status);
      return response;
    }

    public VersionInfo GetVersion()
    {
      System.Version version = this.GetType().Assembly.GetName().Version;

      return new VersionInfo()
      {
        Major = version.Major,
        Minor = version.Minor,
        Build = version.Build,
        Revision = version.Revision
      };
    }

  }
}
