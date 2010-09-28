﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Configuration;
using System.Collections.Specialized;

using org.iringtools.library;
using org.iringtools.utility;
using org.iringtools.client.Models;
using System.Text;

namespace org.iringtools.client.Controllers
{
  public class ExchangeController : Controller
  {

    NameValueCollection _settings = null;
    string _adapterServiceURI = String.Empty;
    string _refDataServiceURI = String.Empty;

    public ExchangeController()
    {
      _settings = ConfigurationManager.AppSettings;
      _adapterServiceURI = _settings["AdapterServiceUri"];
      _refDataServiceURI = _settings["ReferenceDataServiceUri"];
    }

    //
    // GET: /Exchange

    public JsonResult Index()
    {
      return Json(null, JsonRequestBehavior.AllowGet);
    }

    //
    // GET: /Exchange/Methods

    public JsonResult Methods()
    {
      JsonContainer<JsonArray> container = new JsonContainer<JsonArray>();
      container.Items = new JsonArray();
      
      container.Items.Add("Name","Data Tranfer Object");
      container.Items.Add("Name","Reference Data Format");
      container.Total = container.Items.Count();

      return Json(container, JsonRequestBehavior.AllowGet);
    }

    //
    // POST: Exchange/Pull?scope={scope}&application={application}

    [HttpPost]
    public JsonResult Pull(FormCollection collection)
    {
      string scope = Request.QueryString["scope"];
      string application = Request.QueryString["application"];
      string graphName = collection["graph"];

      JsonContainer<List<Status>> container = new JsonContainer<List<Status>>();

      if (collection.Count > 0)
      {
        Uri address = new Uri(String.Format(_adapterServiceURI + "/{0}/{1}/{2}/pull?method=sparql", scope, application, graphName));

        WebCredentials credentials = new WebCredentials() {
          domain = collection["domain"],
          userName = collection["username"],
          password = collection["password"]
        };
        credentials.Encrypt();
                
        Request request = new Request();
        request.Add("targetEndpointUri", collection["targetEndpointUri"]);
        request.Add("targetGraphBaseUri", collection["targetGraphBaseUri"]);
        request.Add("targetCredentials", Utility.Serialize<WebCredentials>(credentials, true));        

        string postData = Utility.Serialize<Request>(request, true);

        WebHttpClient client = new WebHttpClient(_adapterServiceURI);
        string result = client.Post<Request>(String.Format("/{0}/{1}/{2}/pull?method=sparql", scope, application, graphName), request, true);

        //WebClient webClient = new WebClient();
        //string result = webClient.UploadString(address, "POST", postData);         

        Response response = result.DeserializeDataContract<Response>();
      }

      return Json(container, JsonRequestBehavior.AllowGet);
    }
  }
}


//http://adcrdlweb/Services/AdapterService/12345_000/EXCEL/EQUIPMENT