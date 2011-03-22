using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Configuration;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

using org.iringtools.library;
using org.iringtools.utility;
using org.iringtools.mapping;
using org.iringtools.datalayer.excel;

using iRINGTools.Web.Helpers;

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

    private void SetExcelSource(string scope, string application, string source)
    {
      string key = string.Format("ExcelSource.{0}.{1}", scope, application);

      Session[key] = source;
    }

    private string GetExcelSource(string scope, string application)
    {
      string key = string.Format("ExcelSource.{0}.{1}", scope, application);
      string value = null;


      if (Session[key] != null)
      {
        value = (string)Session[key];
      }

      return value;
    }

    //
    // GET: /ExcelLibrary/

    public ActionResult Index()
    {
      return View();
    }

    //
    // GET: /ExcelLibrary/Configure

    public JsonResult Source(FormCollection form)
    {
      string adapterServiceURI = _adapterServiceURI;
      string savedFileName = string.Empty;
      HttpFileCollectionBase files = Request.Files;
            
      foreach (string file in files)
      {
        HttpPostedFileBase hpf = files[file] as HttpPostedFileBase;
        if (hpf.ContentLength == 0)
          continue;

        string location = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _settings["App_Data"], form["Scope"], form["Application"]);
        savedFileName = Path.Combine(location, Path.GetFileName(hpf.FileName));

        Directory.CreateDirectory(location);
        hpf.SaveAs(savedFileName);
      }

      SetExcelSource(form["Scope"], form["Application"], savedFileName);
            
      return Json(new { success = true }, JsonRequestBehavior.AllowGet);      
    }

    public JsonResult GetNode(FormCollection form)
    {
      string format = String.Empty;
      string adapterServiceURI = _adapterServiceURI;

      if (Request.QueryString["format"] != null)
        format = Request.QueryString["format"].ToUpper();

      if (Request.QueryString["remote"] != null)
        adapterServiceURI = Request.QueryString["remote"] + "/adapter";

      List<JsonTreeNode> nodes = new List<JsonTreeNode>();

      ExcelProvider excelProvider = new ExcelProvider(_settings, GetExcelSource(form["scope"], form["application"]));

      if (excelProvider != null)
      {

        switch (form["type"])
        {
          case "ExcelWorkbookNode":
            {
              List<ExcelWorksheet> worksheets = excelProvider.GetWorksheets();

              if (worksheets != null)
              {
                foreach (ExcelWorksheet worksheet in worksheets)
                {
                  JsonTreeNode node = new JsonTreeNode
                  {
                    nodeType = "async",
                    type = "ExcelWorksheetNode",
                    //icon = "Content/img/system-file-manager.png",
                    id = worksheet.Name,
                    text = worksheet.Name,
                    @checked = false,
                    expanded = false,
                    leaf = false,
                    children = null                   
                  };

                  nodes.Add(node);
                }
              }

              break;
            }
          case "ExcelWorksheetNode":
            {

              List<ExcelColumn> columns = excelProvider.GetColumns(form["node"]);

              if (columns != null)
              {
                foreach (ExcelColumn column in columns)
                {
                  JsonTreeNode node = new JsonTreeNode
                  {
                    nodeType = "async",
                    type = "ExcelColumnNode",
                    //icon = "Content/img/system-file-manager.png",
                    id = column.Name,
                    text = column.Name,
                    @checked = false,
                    expanded = false,
                    leaf = true,
                    children = null
                  };

                  nodes.Add(node);
                }
              }

              break;
            }
        }
      }

      excelProvider = null;
      
      return Json(nodes, JsonRequestBehavior.AllowGet);
    }

    public JsonResult Configure(FormCollection form)
    { 
      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

  }
}
