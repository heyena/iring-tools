using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Configuration;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Xml;

using org.iringtools.library;
using org.iringtools.utility;
using org.iringtools.mapping;

using iRINGTools.Web.Models;
using System.Xml.Linq;

using org.iringtools.excel;
using System.IO;

namespace iRINGTools.Web.Controllers
{
  public class ExcelLibraryController : Controller
  {

    NameValueCollection _settings = null;

    string _adapterServiceURI = String.Empty;
    string _refDataServiceURI = String.Empty;

    public ExcelLibraryController()
    {
      _settings = ConfigurationManager.AppSettings;
      _adapterServiceURI = _settings["AdapterServiceUri"];
      _refDataServiceURI = _settings["ReferenceDataServiceUri"];
      
      _settings["App_Data"] = ".\\App_Data\\";
    } 

    //
    // GET: /ExcelLibrary/

    public ActionResult Index()
    {
      return View();
    }

    //
    // GET: /ExcelLibrary/Configure

    public JsonResult Configure(FormCollection form)
    {
      string adapterServiceURI = _adapterServiceURI;
      string savedFileName = string.Empty;

      string configurationUri = string.Format("/{0}/{1}/configure", form["Scope"], form["Application"] );

      if (Request.QueryString["remote"] != null)
        adapterServiceURI = Request.QueryString["remote"] + "/adapter";
            
      WebHttpClient client = new WebHttpClient(_adapterServiceURI);
      client.ForwardPost(configurationUri, Request);

      return Json(new { success = true }, JsonRequestBehavior.AllowGet);

      //if (!string.IsNullOrEmpty(savedFileName))
      //{
      //  ExcelConfiguration excel = new ExcelConfiguration();
      //  excel.Location = savedFileName;
      //  DataLayerConfig configuration = new DataLayerConfig();
      //  configuration.DataLayerName = "org.iringtools.adapter.datalayer.ExcelDataLayer, ExcelLibrary";
      //  configuration.DataLayerConfiguration = XElement.Parse(Utility.Serialize<ExcelConfiguration>(excel, System.Text.Encoding.UTF8, true));
        
      //  WebHttpClient client = new WebHttpClient(adapterServiceURI);
      //  client.Post<DataLayerConfig>(configurationUri, configuration, true);        
        
      //  return Json(new { success = true }, JsonRequestBehavior.AllowGet);
      //}
      //else
      //{
      //  return Json(new { success = false }, JsonRequestBehavior.AllowGet);
      //}
    }

  }
}
