using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Collections.Specialized;

using org.iringtools.library;
using DocumentFormat.OpenXml.Packaging;
using log4net;

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

    private ServiceSettings _settings = null;
    private ISpreadsheetRepository _repository { get; set; }
    private string _keyFormat = "adpmgr-Configuration.{0}.{1}";
    private string _appData = string.Empty;
    private static readonly ILog _logger = LogManager.GetLogger(typeof(SpreadsheetController));

    public SpreadsheetController()
      : this(new SpreadsheetRepository())
    {
    }

    public SpreadsheetController(ISpreadsheetRepository repository)
    {
      var settings = ConfigurationManager.AppSettings;
      _settings = new ServiceSettings();
      _settings.AppendSettings(settings);
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
        var datalayer = "org.iringtools.adapter.datalayer.SpreadsheetDatalayer, SpreadsheetDatalayer";
        var savedFileName = string.Empty;

        var files = Request.Files;

        foreach (string file in files)
        {
          var hpf = files[file] as HttpPostedFileBase;
          if (hpf.ContentLength == 0)
            continue;
          var fileLocation = string.Format(@"{0}SpreadsheetData.{1}.{2}.xlsx",_settings["AppDataPath"], form["context"], form["endpoint"]);


          var configuration = new SpreadsheetConfiguration()
          {
            Location = fileLocation
          };

          if (form["Generate"] != null)
          {
              configuration.Generate = true;
            configuration = _repository.ProcessConfiguration(configuration, hpf.InputStream);
            hpf.InputStream.Flush();
            hpf.InputStream.Position = 0;
            _repository.Configure(form["context"], form["endpoint"], datalayer, configuration, hpf.InputStream);
          }
          else
          {
            configuration.Generate = false;
            configuration = _repository.ProcessConfiguration(configuration, hpf.InputStream);
          }

          SetConfiguration(form["context"], form["endpoint"], configuration);

          //break;
        }
      }
      catch (Exception ex)
      {
        ;
        return new JsonResult()
        {
          ContentType = "text/html",
          Data = PrepareErrorResponse(ex)
        };
      }
      return new JsonResult()
        {
          ContentType = "text/html",
          Data = new { success = true }
        };
    }

    public ActionResult Export(string scope, string application)
    {
      try
      {        
        var bytes = _repository.getExcelFile(scope, application);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", string.Format("SpreadsheetData.{0}.{1}.xlsx", scope, application));
      }
      catch (Exception ioEx)
      {
        _logger.Error(ioEx.Message);
        throw ioEx;
      }
   }

    private SpreadsheetConfiguration GetConfiguration(string context, string endpoint)
    {
      var key = string.Format(_keyFormat, context, endpoint);


      if (Session[key] == null)
      {
        Session[key] = _repository.GetConfiguration(context, endpoint);
      }

      return (SpreadsheetConfiguration)Session[key];
    }

    public ActionResult UpdateConfiguration(FormCollection form)
    {
      var configuration = GetConfiguration(form["context"], form["endpoint"]);
      if (configuration == null)
      {
        return Json(new {success = false}, JsonRequestBehavior.AllowGet);
      }
      else
      {
        foreach (var workSheet in configuration.Tables)
        {
          if (workSheet.Name == form["Name"])
            workSheet.Label = form["Label"];
          if (workSheet.Columns == null) continue;
          foreach (var column in workSheet.Columns.Where(column => column.Name == form["Name"]))
          {
            column.Label = form["Label"];
          }
        }
        _repository.Configure(form["context"], form["endpoint"], form["datalayer"], configuration, null);
        return Json(new {success = true}, JsonRequestBehavior.AllowGet);
      }
    }

    private void SetConfiguration(string context, string endpoint, SpreadsheetConfiguration configuration)
    {
      var key = string.Format(_keyFormat, context, endpoint);

      Session[key] = configuration;
    }

    public JsonResult GetNode(FormCollection form)
    {
      var nodes = new List<JsonTreeNode>();

      if (_repository != null)
      {
        var configuration = GetConfiguration(form["context"], form["endpoint"]);

        if (configuration != null)
        {
          switch (form["type"])
          {
            case "ExcelWorkbookNode":
              {
                var worksheets = configuration.Tables;

                if (worksheets != null)
                {
                  foreach (var worksheet in worksheets)
                  {
                    var columnNodes = new List<JsonTreeNode>();
                    var keyIdentifierNode = new JsonTreeNode()
                    {
                      text = "Identifier",
                      type = "Identifier",
                      expanded = true,
                      leaf = false,
                      children = new List<JsonTreeNode>()
                    };

                    var dataPropertiesNode = new JsonTreeNode()
                    {
                      text = "Columns",
                      type = "columns",
                      expanded = true,
                      leaf = false,
                      children = new List<JsonTreeNode>()
                    };

                    var dataObjectNode = new JsonTreeNode()
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

                    if (worksheet.Columns == null) continue;
                    foreach (var column in worksheet.Columns)
                    {
                      if (column.Name.ToUpper() == worksheet.Identifier.ToUpper())
                      {
                        var keyNode = new JsonTreeNode
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
                                Label = column.Label,
                                Name = column.Name
                              }
                          };
                        keyIdentifierNode.children.Add(keyNode);
                      }
                      else
                      {

                        var columnNode = new JsonTreeNode
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

                break;
              }
          }
        }
      }

      return Json(nodes, JsonRequestBehavior.AllowGet);
    }

    public JsonResult Configure(FormCollection form)
    {
      var configuration = GetConfiguration(form["context"], form["endpoint"]);

      if (configuration != null)
      {
        _repository.Configure(form["context"], form["endpoint"], form["DataLayer"], configuration, null);
        return new JsonResult() //(6)
            {
                ContentType = "text/html",
                Data = new { success = true }
            };
        }

      return new JsonResult() //(6)
        {
          ContentType = "text/html",
          Data = new { success = false }
        };
    }

    public JsonResult GetWorksheets(FormCollection form)
    {
      var container = new JsonContainer<List<WorksheetPart>>
        {items = _repository.GetWorksheets(GetConfiguration(form["context"], form["endpoint"])), success = true};

      return Json(container, JsonRequestBehavior.AllowGet);
    }

    public JsonResult GetColumns(FormCollection form)
    {
      var container = new JsonContainer<List<SpreadsheetColumn>>
        {
          items = _repository.GetColumns(GetConfiguration(form["context"], form["endpoint"]), form["worksheet"]),
          success = true
        };

      return Json(container, JsonRequestBehavior.AllowGet);
    }

    private Response PrepareErrorResponse(Exception ex)
    {
      var response = new Response {Level = StatusLevel.Error, Messages = new Messages {ex.Message, ex.StackTrace}};
      return response;
    }

  }

}

