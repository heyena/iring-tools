﻿using System;
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
using log4net;

using org.iringtools.library;
using org.iringtools.utility;
using org.iringtools.mapping;
using DocumentFormat.OpenXml.Packaging;

namespace org.iringtools.adapter.datalayer
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

  public class SpreadsheetController : Controller
  {

    private NameValueCollection _settings = null;
    private ISpreadsheetRepository _repository { get; set; }
    private string _keyFormat = "Configuration.{0}.{1}";
    private ILog _logger = LogManager.GetLogger(typeof(SpreadsheetController)); 

    public SpreadsheetController()
      : this(new SpreadsheetRepository())
    {
    }

    public SpreadsheetController(ISpreadsheetRepository repository)
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

    public JsonResult Upload(FormCollection form)
    {
      try
      {
       
        string datalayer = "org.iringtools.adapter.datalayer.SpreadsheetDataLayer, SpreadsheetLibrary";
        string savedFileName = string.Empty;

        HttpFileCollectionBase files = Request.Files;

        foreach (string file in files)
        {
          HttpPostedFileBase hpf = files[file] as HttpPostedFileBase;
          if (hpf.ContentLength == 0)
            continue;
          string fileLocation = string.Format(@"App_Data\SpreadsheetData.{0}.{1}.xlsx", form["Scope"], form["Application"]);

          SpreadsheetConfiguration configuration = new SpreadsheetConfiguration()
          {
            Location = fileLocation
          };

          if (form["Generate"] != null)
          {

            configuration = _repository.ProcessConfiguration(configuration, hpf.InputStream);
            hpf.InputStream.Flush();
            hpf.InputStream.Position = 0;
            _repository.Configure(form["Scope"], form["Application"], datalayer, configuration, hpf.InputStream);
          }
          else
          {
            configuration.Generate = false;
            configuration = _repository.ProcessConfiguration(configuration, hpf.InputStream);
          }

          SetConfiguration(form["Scope"], form["Application"], configuration);

          //break;
        }
      }
      catch (Exception ex)
      {
        _logger.Error("Error uploading file ..." + ex);
        return new JsonResult()
        {
          ContentType = "text/html",
          Data = new { success = false }
        };
      }
      return new JsonResult()
        {
          ContentType = "text/html",
          Data = new { success = true }
        };
    }

    private SpreadsheetConfiguration GetConfiguration(string scope, string application)
    {
      string key = string.Format(_keyFormat, scope, application);

      if (Session[key] == null)
      {
        Session[key] = _repository.GetConfiguration(scope, application);
      }

      return (SpreadsheetConfiguration)Session[key];
    }

    public ActionResult UpdateConfiguration(FormCollection form)
    {
      SpreadsheetConfiguration configuration = GetConfiguration(form["Scope"], form["Application"]);
      if (configuration != null)
      {
        foreach (SpreadsheetTable workSheet in configuration.Tables)
        {
          if (workSheet.Name == form["Name"])
            workSheet.Label = form["Label"];
          if (workSheet.Columns != null)
          {
            foreach (SpreadsheetColumn column in workSheet.Columns)
            {
              if (column.Name == form["Name"])
                column.Label = form["Label"];
            }
          }
        }
        _repository.Configure(form["scope"], form["application"], form["datalayer"], configuration,null);
        return Json(new { success = true }, JsonRequestBehavior.AllowGet);
      }
      else
      {
        return Json(new { success = false }, JsonRequestBehavior.AllowGet);
      }

    }
    private void SetConfiguration(string scope, string application, SpreadsheetConfiguration configuration)
    {
      string key = string.Format(_keyFormat, scope, application);

      Session[key] = configuration;
    }

    public JsonResult GetNode(FormCollection form)
    {
      List<JsonTreeNode> nodes = new List<JsonTreeNode>();

      if (_repository != null)
      {
        SpreadsheetConfiguration configuration = GetConfiguration(form["Scope"], form["Application"]);

        if (configuration != null)
        {

          switch (form["type"])
          {
            case "ExcelWorkbookNode":
              {
                List<SpreadsheetTable> worksheets = configuration.Tables;

                if (worksheets != null)
                {
                  foreach (SpreadsheetTable worksheet in worksheets)
                  {
                    List<JsonTreeNode> columnNodes = new List<JsonTreeNode>();
                    JsonTreeNode keyIdentifierNode = new JsonTreeNode()
                    {
                      text = "Identifier",
                      type = "Identifier",
                      expanded = true,
                      leaf = false,
                      children = new List<JsonTreeNode>()
                    };

                    JsonTreeNode dataPropertiesNode = new JsonTreeNode()
                    {
                      text = "Columns",
                      type = "columns",
                      expanded = true,
                      leaf = false,
                      children = new List<JsonTreeNode>()
                    };

                    JsonTreeNode dataObjectNode = new JsonTreeNode()
                    {
                      nodeType = "async",
                      type = "ExcelWorksheetNode",
                      icon = "Content/img/excelworksheet.png",
                      id = worksheet.Name,
                      text = worksheet.Name.Equals(worksheet.Label) ? worksheet.Name : string.Format("{0} [{1}]", worksheet.Name, worksheet.Label),
                      expanded = false,
                      leaf = false,
                      children = new List<JsonTreeNode>()
                                        {
                                        keyIdentifierNode, dataPropertiesNode
                                        },
                      record = worksheet
                    };

                    columnNodes.Add(dataPropertiesNode);

                    if (worksheet.Columns != null)
                    {
                      foreach (SpreadsheetColumn column in worksheet.Columns)
                      {
                        if (column.Name.ToUpper() == worksheet.Identifier.ToUpper())
                        {
                          JsonTreeNode keyNode = new JsonTreeNode
                          {
                            nodeType = "async",
                            type = "ExcelColumnNode",
                            icon = "Content/img/excelcolumn.png",
                            id = worksheet.Name + "/" + column.Name,
                            text = column.Name.Equals(column.Label) ? column.Name : string.Format("{0} [{1}]", column.Name, column.Label),
                            expanded = false,
                            leaf = true,
                            children = null,
                            record = new
                            {
                              Datatype = column.DataType.ToString(),
                              Index = column.ColumnIdx,
                              Label = column.Label.ToString(),
                              Name = column.Name.ToString()
                            }
                          };
                          keyIdentifierNode.children.Add(keyNode);
                        }
                        else
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
                            // record = column
                            record = new
                            {
                              Datatype = column.DataType.ToString(),
                              Index = column.ColumnIdx,
                              Label = column.Label.ToString(),
                              Name = column.Name.ToString()
                            }
                          };

                          dataPropertiesNode.children.Add(columnNode);
                        }
                      }
                      nodes.Add(dataObjectNode);
                    }
                  }
                }

                break;
              }
          }
        }
      }

      return Json(nodes, JsonRequestBehavior.AllowGet);
    }

    public JsonResult Configure(FormCollection form)
    {
      SpreadsheetConfiguration configuration = GetConfiguration(form["Scope"], form["Application"]);

      if (configuration != null)
      {
        _repository.Configure(form["Scope"], form["Application"], form["DataLayer"], configuration, null);
        return new JsonResult() //(6)
            {
                ContentType = "text/html",
                Data = new { success = true }
            };
        }
      
      else
      {
        return new JsonResult() //(6)
        {
          ContentType = "text/html",
          Data = new { success = false }
        };
      }
    }

    public JsonResult GetWorksheets(FormCollection form)
    {
      JsonContainer<List<WorksheetPart>> container = new JsonContainer<List<WorksheetPart>>();
      container.items = _repository.GetWorksheets(GetConfiguration(form["Scope"], form["Application"]));
      container.success = true;

      return Json(container, JsonRequestBehavior.AllowGet);
    }

    public JsonResult GetColumns(FormCollection form)
    {
      JsonContainer<List<SpreadsheetColumn>> container = new JsonContainer<List<SpreadsheetColumn>>();
      container.items = _repository.GetColumns(GetConfiguration(form["scope"], form["application"]), form["worksheet"]);
      container.success = true;

      return Json(container, JsonRequestBehavior.AllowGet);
    }

    public JsonResult AddWorksheets(FormCollection form)
    {
      SpreadsheetConfiguration configuration = GetConfiguration(form["scope"], form["application"]);
      List<WorksheetPart> worksheets = _repository.GetWorksheets(configuration);

      object worksheetNames = form["worksheets"];

      return Json(new { success = false }, JsonRequestBehavior.AllowGet);
    }

    public ActionResult AddColumns(FormCollection form)
    {
      return Json(new { success = false }, JsonRequestBehavior.AllowGet);
    }

  }

}

