using System;
using System.Text;
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

namespace org.iringtools.datalayer.excel
{
  public class JsonTreeNode
  {
    public string id { get; set; }
    public string text { get; set; }
    public string icon { get; set; }
    public bool leaf { get; set; }
    public bool expanded { get; set; }
    public List<JsonTreeNode> children { get; set; }
    public string type { get; set; }
    public string nodeType { get; set; }
    public object @checked { get; set; }
    public object record { get; set; }
  }

  public class JsonContainer<T>
  {
    public T items { get; set; }
    public string message { get; set; }
    public Boolean success { get; set; }
    public int total { get; set; }
    public string errors { get; set; }
  }

  public class ExcelController : Controller
  {

    private NameValueCollection _settings = null;
    private IExcelRepository _repository { get; set; }
    private string _keyFormat = "Configuration.{0}.{1}";
    
    public ExcelController()
      : this(new ExcelRepository())
    {      
    }

    public ExcelController(IExcelRepository repository)            
    {
      _settings = ConfigurationManager.AppSettings;
      _repository = repository;
    }    

    //
    // GET: /Excel/

    public ActionResult Index()
    {
      return View();
    }

    public ActionResult Upload(FormCollection form)
    {
      string savedFileName = string.Empty;
      
      HttpFileCollectionBase files = Request.Files;
            
      foreach (string file in files)
      {
        HttpPostedFileBase hpf = files[file] as HttpPostedFileBase;
        if (hpf.ContentLength == 0)
          continue;

        string location = _settings["Upload"];
        
        if (!Path.IsPathRooted(location))
          location = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, location);

        savedFileName = Path.Combine(location, Path.GetFileName(hpf.FileName));        
        
        Directory.CreateDirectory(location);
        hpf.SaveAs(savedFileName);

        ExcelConfiguration configuration = new ExcelConfiguration()
        {
          Location = savedFileName
        };

        if (form["Generate"] != null)
        {
          configuration = _repository.ProcessConfiguration(configuration);
        }
        else
        {
          configuration.Generate = false;
          configuration.Worksheets = new List<ExcelWorksheet>();
        }

        SetConfiguration(form["Scope"], form["Application"], configuration);
        
        break;
      }
                  
      return Json(new { success = true }, JsonRequestBehavior.AllowGet);      
    }
    
    private ExcelConfiguration GetConfiguration(string scope, string application)
    {
      string key = string.Format(_keyFormat, scope, application);

      if (Session[key] == null) 
      {
        Session[key] = _repository.GetConfiguration(scope, application);
      }

      return (ExcelConfiguration)Session[key];
    }

    private void SetConfiguration(string scope, string application, ExcelConfiguration configuration)
    {
      string key = string.Format(_keyFormat, scope, application);

      Session[key] = configuration;
    }
        
    public JsonResult GetNode(FormCollection form)
    {      
      List<JsonTreeNode> nodes = new List<JsonTreeNode>();

      if (_repository != null)
      {        
        ExcelConfiguration configuration = GetConfiguration(form["Scope"], form["Application"]);

        if (configuration != null)
        {
          switch (form["type"])
          {
            case "ExcelWorkbookNode":
              {
                List<ExcelWorksheet> worksheets = configuration.Worksheets;

                if (worksheets != null)
                {
                  foreach (ExcelWorksheet worksheet in worksheets)
                  {
                    List<JsonTreeNode> columnNodes = new List<JsonTreeNode>();

                    if (worksheet.Columns != null)
                    {
                      foreach (ExcelColumn column in worksheet.Columns)
                      { 
                        JsonTreeNode columnNode = new JsonTreeNode
                        {
                          nodeType = "async",
                          type = "ExcelColumnNode",
                          icon = "Content/img/excelcolumn.png",
                          id = worksheet.Name + "/" + column.Name,
                          text = column.Name.Equals(column.Label) ? column.Name : string.Format("{0} [{1}]", column.Name, column.Label),
                          expanded = false,
                          leaf = true,
                          children = null,
                          record = column
                        };

                        columnNodes.Add(columnNode);
                      }
                    }

                    JsonTreeNode node = new JsonTreeNode
                    {
                      nodeType = "async",
                      type = "ExcelWorksheetNode",
                      icon = "Content/img/excelworksheet.png",
                      id = worksheet.Name,
                      text = worksheet.Name.Equals(worksheet.Label) ? worksheet.Name : string.Format("{0} [{1}]", worksheet.Name, worksheet.Label),
                      expanded = false,
                      leaf = false,
                      children = columnNodes,
                      record = worksheet
                    };

                    nodes.Add(node);
                  }
                }

                break;
              }
          }
        }
      }

      return Json(nodes, JsonRequestBehavior.AllowGet);
    }

    public ActionResult Configure(FormCollection form)
    {
      ExcelConfiguration configuration = GetConfiguration(form["Scope"], form["Application"]);

      if (configuration != null)
      {
        _repository.Configure(form["scope"], form["application"], form["datalayer"], configuration);
        return Json(new { success = true }, JsonRequestBehavior.AllowGet);
      }
      else
      {
        return Json(new { success = false }, JsonRequestBehavior.AllowGet);
      }
    }

    public JsonResult GetWorksheets(FormCollection form)
    {
      JsonContainer<List<ExcelWorksheet>> container = new JsonContainer<List<ExcelWorksheet>>();
      container.items = _repository.GetWorksheets(GetConfiguration(form["scope"], form["application"]));
      container.success = true;      

      return Json(container, JsonRequestBehavior.AllowGet);
    }

    public JsonResult GetColumns(FormCollection form)
    {
      JsonContainer<List<ExcelColumn>> container = new JsonContainer<List<ExcelColumn>>();
      container.items = _repository.GetColumns(GetConfiguration(form["scope"], form["application"]), form["worksheet"]);
      container.success = true;

      return Json(container, JsonRequestBehavior.AllowGet);
    }

    public JsonResult AddWorksheets(FormCollection form)
    {
      ExcelConfiguration configuration = GetConfiguration(form["scope"], form["application"]);
      List<ExcelWorksheet> worksheets = _repository.GetWorksheets(configuration);

      List<string> worksheetNames = new List<string>(); //Utility.Deserialize<List<string>>(form["worksheets"], true);

      foreach(string worksheetName in worksheetNames) {
        ExcelWorksheet worksheet = worksheets.FirstOrDefault<ExcelWorksheet>(o => o.Name == worksheetName);
        //ExcelWorksheet worksheet = worksheets.FirstOrDefault<ExcelWorksheet>(o => o.Name == worksheetName);
        //if (worksheet != null && configuration.Worksheets.Exists(Pred)
        {
          configuration.Worksheets.Add(worksheet);
        }
      }
      
      return Json(new { success = false }, JsonRequestBehavior.AllowGet);
    }

    public ActionResult AddColumns(FormCollection form)
    {
      return Json(new { success = false }, JsonRequestBehavior.AllowGet);
    }

  }
}
