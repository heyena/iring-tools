using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.utility;
using System.Net;
using org.iringtools.library;
using System.Xml.Linq;

namespace org.iringtools.application
{
  class AdapterClient : ServiceClient
  {
    private ApplicationSettings _settings = null;

    public AdapterClient(ApplicationSettings settings)
      : base((ServiceSettings)settings, settings["AdapterServiceUri"])
    {
      _settings = settings;
    }

    public Response PostScopes(List<ScopeProject> scopes)
    {
      Response response = new Response();
      try
      {
        string relativeUri = "/scopes";

        response = _webHttpClient.Post<List<ScopeProject>, Response>(relativeUri, scopes);
      }
      catch (Exception ex)
      {
        throw new Exception("Error while calling adapter service to update the binding.", ex);
      }
      return response;
    }

    public Response PostBinding(XElement binding)
    {
      Response response = new Response();
      try
      {
        string relativeUri = String.Format("/{0}/{1}/binding",
          _settings["ProjectName"],
          _settings["ApplicationName"]
        );

        response = _webHttpClient.Post<XElement, Response>(relativeUri, binding);
      }
      catch (Exception ex)
      {
        throw new Exception("Error while calling adapter service to update the binding.", ex);
      }
      return response;
    }
  }
}
