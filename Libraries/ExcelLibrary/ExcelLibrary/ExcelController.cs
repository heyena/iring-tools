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

    IExcelRepository _excelRepository { get; set; }

    NameValueCollection _settings = null;
    
    public ExcelController()
      : this(new ExcelRepository())
    {
    }

    public ExcelController(IExcelRepository repository)            
    {
      _settings = ConfigurationManager.AppSettings;      
      _settings["App_Data"] = ".\\App_Data\\";

      _excelRepository = repository;
    }    

    //
    // GET: /Excel/

    public ActionResult Index()
    {
      return View();
    }

    //
    // GET: /Excel/Source

    public JsonResult Source(FormCollection form)
    {      
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
                  
      return Json(new { success = true }, JsonRequestBehavior.AllowGet);      
    }

    public JsonResult GetNode(FormCollection form)
    {
      List<JsonTreeNode> nodes = new List<JsonTreeNode>();

      if (_excelRepository != null)
      {
        string sourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _settings["App_Data"], form["scope"], form["application"], form["filename"]);

        switch (form["type"])
        {
          case "ExcelWorkbookNode":
            {
              List<ExcelWorksheet> worksheets = _excelRepository.GetWorksheets(sourcePath);

              if (worksheets != null)
              {
                foreach (ExcelWorksheet worksheet in worksheets)
                {
                  List<JsonTreeNode> columnNodes = new List<JsonTreeNode>();                

                  foreach (ExcelColumn column in worksheet.Columns)
                  {
                    JsonTreeNode columnNode = new JsonTreeNode
                    {
                      nodeType = "async",
                      type = "ExcelColumnNode",
                      //icon = "Content/img/system-file-manager.png",
                      id = column.Name,
                      text = column.Name,
                      @checked = false,
                      expanded = false,
                      leaf = true,
                      children = null,
                      record = column
                    };

                    columnNodes.Add(columnNode);
                  }
                  
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

    public JsonResult Configure(FormCollection form)
    { 
      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

  }
}
