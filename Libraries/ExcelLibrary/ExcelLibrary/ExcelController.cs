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
using System.Text;

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

  public class ExcelController : Controller
  {

    private NameValueCollection _settings = null;

    IExcelRepository _excelRepository { get; set; }
    
    public ExcelController()
      : this(new ExcelRepository())
    {      
    }

    public ExcelController(IExcelRepository repository)            
    {
      _settings = ConfigurationManager.AppSettings;
      _excelRepository = repository;
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
      bool generate = false;

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

        if (form["Generate"] != null)
          generate = true;

        ExcelConfiguration configuration = new ExcelConfiguration()
        {
          Location = savedFileName,
          Generate = generate,
          Worksheets = generate ? null : new List<ExcelWorksheet>()
        };

        location = _settings["XmlPath"];

        if (!Path.IsPathRooted(location))
          location = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, location);

        savedFileName = Path.Combine(location, "excel-configuration." + form["Scope"] + "." + form["Application"] + ".xml");

        Utility.Write<ExcelConfiguration>(configuration, savedFileName, true);        

        break;
      }
                  
      return Json(new { success = true }, JsonRequestBehavior.AllowGet);      
    }
        
    public JsonResult GetNode(FormCollection form)
    {      
      List<JsonTreeNode> nodes = new List<JsonTreeNode>();

      if (_excelRepository != null)
      {        
        ExcelConfiguration configuration = _excelRepository.GetConfiguration(form["Scope"], form["Application"]);

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
                        id = column.Name,
                        text = column.Name,                        
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
                    text = worksheet.Name,                    
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

      return Json(nodes, JsonRequestBehavior.AllowGet);
    }

    public ActionResult Configure(FormCollection form)
    {      
      ExcelConfiguration configuration = _excelRepository.GetConfiguration(form["Scope"], form["Application"]);
      
      _excelRepository.Configure(configuration);
      
      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

  }
}
