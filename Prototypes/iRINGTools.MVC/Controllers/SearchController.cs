using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Configuration;
using System.Collections.Specialized;

using org.iringtools.library;
using org.iringtools.utility;
using org.iringtools.client.Models;

namespace org.iringtools.client.Controllers
{
  public class SearchController : Controller
  {
    NameValueCollection _settings = null;
    string _adapterServiceURI = String.Empty;
    string _refDataServiceURI = String.Empty;

    public SearchController()
    {
      _settings = ConfigurationManager.AppSettings;
      _adapterServiceURI = _settings["AdapterServiceUri"];
      _refDataServiceURI = _settings["ReferenceDataServiceUri"];
    }

    //
    // POST: /Search/
    [HttpPost]
    public JsonResult Index(FormCollection collection)
    {
      string query = collection["query"];
      string start = collection["start"];
      string limit = collection["limit"];
      JsonContainer<List<Entity>> container = new JsonContainer<List<Entity>>();

      if (query != null && !query.Equals(String.Empty))
      {

        Uri address = new Uri(String.Format(_refDataServiceURI + "/search/{0}/{1}/{2}", query, start, limit));

        WebClient webClient = new WebClient();
        string result = webClient.DownloadString(address);

        RefDataEntities dataEntities = result.DeserializeDataContract<RefDataEntities>();
        
        container.Items = dataEntities.Entities.Values.ToList<Entity>();
        container.Total = dataEntities.Total;

      }

      return Json(container, JsonRequestBehavior.AllowGet);
    }
  }
}
