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

using iRINGTools.Web.Helpers;
using iRINGTools.Web.Models;

namespace iRINGTools.Web.Controllers
{
  public class RefDataController : Controller
  {

    IRefDataRepository _refdataRepository = null;
    NameValueCollection _settings = null;
    string _adapterServiceURI = String.Empty;
    string _refDataServiceURI = String.Empty;

    public RefDataController()
      : this(new RefDataRepository())
    { }

    public RefDataController(IRefDataRepository repository)
    {
      _refdataRepository = repository;
    }

    public JsonResult Index(FormCollection collection)
    {
      string query = collection["query"];
      string start = collection["start"];
      string limit = collection["limit"];
      JsonContainer<List<Entity>> container = new JsonContainer<List<Entity>>();

      if (!string.IsNullOrEmpty(query))
      {
        RefDataEntities dataEntities = _refdataRepository.Search(query,start,limit);
        
        container.items = dataEntities.Entities.Values.ToList<Entity>();
        container.total = dataEntities.Total;
        container.success = true;
      }

      return Json(container, JsonRequestBehavior.AllowGet);
    }

    public JsonResult GetNode(FormCollection form)
    {
      string start = Request.QueryString["start"];
      string limit = Request.QueryString["limit"];
      string query = Request.QueryString["query"];
      string searchtype = Request.QueryString["type"];
      List<JsonTreeNode> nodes = new List<JsonTreeNode>();
      if (!string.IsNullOrEmpty(query))
      {
        RefDataEntities dataEntities = _refdataRepository.Search(query, start, limit);
        foreach (Entity entity in dataEntities.Entities.Values.ToList<Entity>())
        {
          JsonTreeNode node = new JsonTreeNode
          {
            nodeType = "async",
            type = searchtype,
            icon = "Content/img/class.png",
            id = (entity.Label+entity.Repository).GetHashCode().ToString(),
            text = entity.Label + '[' + entity.Repository + ']',
            expanded = false,
            leaf = false,
            children = null,
            record = entity
          };

          nodes.Add(node);
        }
      }
      return Json(nodes, JsonRequestBehavior.AllowGet);
    }

    public ActionResult SubClasses(FormCollection collection)
    {
      string classId = collection["id"];
      JsonContainer<List<Entity>> container = new JsonContainer<List<Entity>>();

      if (!string.IsNullOrEmpty(classId))
      {
        Entities dataEntities = _refdataRepository.GetSubClasses(classId);

        container.items = dataEntities.ToList<Entity>();
        container.total = dataEntities.Count;
        container.success = true;
      }
      
      return Json(container, JsonRequestBehavior.AllowGet);      
    }

    public ActionResult SuperClasses(FormCollection collection)
    {
      string classId = collection["id"];
      JsonContainer<List<Entity>> container = new JsonContainer<List<Entity>>();

      if (!string.IsNullOrEmpty(classId))
      {
        Entities dataEntities = _refdataRepository.GetSuperClasses(classId);

        container.items = dataEntities.ToList<Entity>();
        container.total = dataEntities.Count;
        container.success = true;
      }

      return Json(container, JsonRequestBehavior.AllowGet);
    }

    public ActionResult ClassesTemplates(FormCollection collection)
    {
      string classId = collection["id"];
      JsonContainer<List<Entity>> container = new JsonContainer<List<Entity>>();

      if (!string.IsNullOrEmpty(classId))
      {
        Entities dataEntities = _refdataRepository.GetClassTemplates(classId);

        container.items = dataEntities.ToList<Entity>();
        container.total = dataEntities.Count;
        container.success = true;
      }

      return Json(container, JsonRequestBehavior.AllowGet);
    }

  }
}

