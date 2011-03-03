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

      _settings["App_Data"] = "./App_Data/";

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

      if (Request.QueryString["remote"] != null)
        adapterServiceURI = Request.QueryString["remote"] + "/adapter";
      
      foreach (string file in Request.Files)
      {
        HttpPostedFileBase hpf = Request.Files[file] as HttpPostedFileBase;
        if (hpf.ContentLength == 0)
          continue;
        string savedFileName = Path.Combine(
           AppDomain.CurrentDomain.BaseDirectory, 
           _settings["App_Data"],
           Path.GetFileName(hpf.FileName));
        hpf.SaveAs(savedFileName);
      }

      ExcelConfiguration excel = new ExcelConfiguration();
      excel.Location = form[""];

      XElement dataLayerAssem = new XElement("dataLayerName");
      dataLayerAssem.Value = form["dataLayer"];

      XElement dataLayerConfig = new XElement("dataLayerConfiguration");
      dataLayerConfig.Value = Utility.Serialize<ExcelConfiguration>(excel, true);

      XElement configuration = new XElement("root");
      configuration.Add(dataLayerAssem);
      configuration.Add(dataLayerConfig);

      WebHttpClient client = new WebHttpClient(adapterServiceURI);
      client.Post<XElement>("/configure", configuration, true);

      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

  }
}
