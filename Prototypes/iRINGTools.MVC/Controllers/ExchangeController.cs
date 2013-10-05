using System;
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

      JsonArrayItem item1 = new JsonArrayItem();
      item1.Add("Name", "Data Tranfer Object");
      item1.Add("Uri", "AdapterService");
      item1.Add("IdentificationByTag", "900001-000");
      container.Items.Add(item1);

      JsonArrayItem item2 = new JsonArrayItem();
      item2.Add("Name", "Reference Data Format");
      item2.Add("Uri", "InterfaceService/query");
      container.Items.Add(item2);
            
      container.Total = container.Items.Count();
      container.success = true;
      return Json(container, JsonRequestBehavior.AllowGet);
    }

    //
    // POST: Exchange/Pull?scope={scope}&application={application}
        
    public JsonResult Pull(FormCollection collection)
    {
      string sourceScope = Request.QueryString["scope"];
      string sourceApp = Request.QueryString["application"];
      string sourceGraph = Request.QueryString["graph"];

      //string targetScope = collection["targetScope"];
      //string targetApp = collection["targetApplication"];
      //string targetGraph = collection["targetGraph"];       

      JsonContainer<List<Status>> container = new JsonContainer<List<Status>>();

      if (collection.Count > 0)
      {        
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
        string result = client.Post<Request>(String.Format("/{0}/{1}/{2}/pull?method=sparql", sourceScope, sourceApp, sourceGraph), request, true);

        //WebClient webClient = new WebClient();
        //string result = webClient.UploadString(address, "POST", postData);         

        Response response = result.DeserializeDataContract<Response>();

        foreach (Status stat in response.StatusList)
        {

          container.Message += String.Join(" ", stat.Messages.ToArray());
        }
        container.success = true;
        container.Items = response.StatusList;
      }

      return Json(container, JsonRequestBehavior.AllowGet);
    }
  }
}


//http://adcrdlweb/Services/AdapterService/12345_000/EXCEL/EQUIPMENT