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
using org.ids_adi.qmxf;

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
      string start = form["start"];
      string limit = form["limit"];
      string query = form["query"];
      string searchtype = form["type"];
      List<JsonTreeNode> nodes = new List<JsonTreeNode>();
      if (!string.IsNullOrEmpty(query))
      {
        RefDataEntities dataEntities = _refdataRepository.Search(query, start, limit);
        foreach (Entity entity in dataEntities.Entities.Values.ToList<Entity>())
        {
          JsonTreeNode node = new JsonTreeNode
          {
            nodeType = "async",
            type = "classNode",
            icon = "Content/img/class.png",
            identifier = entity.Uri.Split('#')[1],
            id = (entity.Label+entity.Repository).GetHashCode().ToString(),
            text = entity.Label + '[' + entity.Repository + ']',
            expanded = false,
            leaf = false,
            children = new List<JsonTreeNode>(),
            record = entity
          };
          JsonTreeNode clasifNode = new JsonTreeNode {
            id = ("Classifications" + node.text).GetHashCode().ToString(), 
            children = null, 
            leaf = false, 
            text = "Classifications", 
            expanded = false 
          };
          node.children.Add(clasifNode);
          JsonTreeNode supersNode = new JsonTreeNode {
            id = ("Superclasses" + node.text).GetHashCode().ToString(), 
            children = null, 
            leaf = false, 
            text = "Superclasses", 
            expanded = false 
          };
          node.children.Add(supersNode);
          JsonTreeNode subsNode = new JsonTreeNode {
            id = ("Subclasses" + node.text).GetHashCode().ToString(), 
            children = null, 
            leaf = false, 
            text = "Subclasses", 
            expanded = false 
          };
          node.children.Add(subsNode); 
          JsonTreeNode tempsNode = new JsonTreeNode {
            id = ("Templates" + node.text).GetHashCode().ToString(), 
            children = null, 
            leaf = false, 
            text = "Templates", 
            expanded = false 
          };
          node.children.Add(tempsNode);
          nodes.Add(node);
        }
      }
      return Json(nodes, JsonRequestBehavior.AllowGet);
    }

    public JsonResult SubClasses(FormCollection form)
    {
      string classId = form["id"];
      string searchtype = Request.QueryString["type"];
      List<JsonTreeNode> nodes = new List<JsonTreeNode>();
      

      if (!string.IsNullOrEmpty(classId))
      {
        Entities dataEntities = _refdataRepository.GetSubClasses(classId);
        foreach (var entity in dataEntities)
        {
          JsonTreeNode node = new JsonTreeNode
          {
            nodeType = "async",
            type = "subclassNode",
            icon = "Content/img/class.png",
            identifier = entity.Uri.Split('#')[1],
            id = (entity.Label + entity.Repository).GetHashCode().ToString(),
            text = entity.Label + '[' + entity.Repository + ']',
            expanded = false,
            leaf = false,
            children = new List<JsonTreeNode>(),
            record = entity
          };
          JsonTreeNode clasifNode = new JsonTreeNode
          {
            id = ("Classifications" + node.text).GetHashCode().ToString(),
            children = null,
            leaf = false,
            text = "Classifications",
            expanded = false
          };
          node.children.Add(clasifNode);
          JsonTreeNode supersNode = new JsonTreeNode
          {
            id = ("Superclasses" + node.text).GetHashCode().ToString(),
            children = null,
            leaf = false,
            text = "Superclasses",
            expanded = false
          };
          node.children.Add(supersNode);
          JsonTreeNode subsNode = new JsonTreeNode
          {
            id = ("Subclasses" + node.text).GetHashCode().ToString(),
            children = null,
            leaf = false,
            text = "Subclasses",
            expanded = false
          };
          node.children.Add(subsNode);
          JsonTreeNode tempsNode = new JsonTreeNode
          {
            id = ("Templates" + node.text).GetHashCode().ToString(),
            children = null,
            leaf = false,
            text = "Templates",
            expanded = false
          };
          node.children.Add(tempsNode);
          nodes.Add(node);
        }
      }
      
      return Json(nodes, JsonRequestBehavior.AllowGet);      
    }

    public JsonResult SuperClasses(FormCollection form)
    {

      string searchtype = Request.QueryString["type"];
      List<JsonTreeNode> nodes = new List<JsonTreeNode>();
      string classId = form["id"];

      if (!string.IsNullOrEmpty(classId))
      {
        Entities dataEntities = _refdataRepository.GetSuperClasses(classId);
        foreach (var entity in dataEntities)
        {
          JsonTreeNode node = new JsonTreeNode
          {
            nodeType = "async",
            type = "superClassNode",
            icon = "Content/img/class.png",
            identifier = entity.Uri.Split('#')[1],
            id = (entity.Label + entity.Repository).GetHashCode().ToString(),
            text = entity.Label + '[' + entity.Repository + ']',
            expanded = false,
            leaf = false,
            children = new List<JsonTreeNode>(),
            record = entity
          };
          JsonTreeNode clasifNode = new JsonTreeNode
          {
            id = ("Classifications" + node.text).GetHashCode().ToString(),
            children = null,
            leaf = false,
            text = "Classifications",
            expanded = false
          };
          node.children.Add(clasifNode);
          JsonTreeNode supersNode = new JsonTreeNode
          {
            id = ("Superclasses" + node.text).GetHashCode().ToString(),
            children = null,
            leaf = false,
            text = "Superclasses",
            expanded = false
          };
          node.children.Add(supersNode);
          JsonTreeNode subsNode = new JsonTreeNode
          {
            id = ("Subclasses" + node.text).GetHashCode().ToString(),
            children = null,
            leaf = false,
            text = "Subclasses",
            expanded = false
          };
          node.children.Add(subsNode);
          JsonTreeNode tempsNode = new JsonTreeNode
          {
            id = ("Templates" + node.text).GetHashCode().ToString(),
            children = null,
            leaf = false,
            text = "Templates",
            expanded = false
          };
          node.children.Add(tempsNode);
          nodes.Add(node);
        }
      }
      return Json(nodes, JsonRequestBehavior.AllowGet);
    }

    public JsonResult Templates(FormCollection form)
    {
      string searchtype = Request.QueryString["type"];
      string classId = form["id"];
      List<JsonTreeNode> nodes = new List<JsonTreeNode>();

      if (!string.IsNullOrEmpty(classId))
      {
        Entities dataEntities = _refdataRepository.GetClassTemplates(classId);
        foreach (var entity in dataEntities)
        {
          JsonTreeNode node = new JsonTreeNode
          {
            nodeType = "async",
            type = "templateNode",
            icon = "Content/img/template.png",
            id = (entity.Label + entity.Repository).GetHashCode().ToString(),
            text = entity.Label + '[' + entity.Repository + ']',
            expanded = false,
            leaf = true,
            children = null,
            record = entity
          };

          nodes.Add(node);
        }
      }

      return Json(nodes, JsonRequestBehavior.AllowGet);
    }

    public JsonResult Classes(FormCollection form)
    {
      string searchtype = Request.QueryString["type"];
      string classId = form["id"];
      List<JsonTreeNode> nodes = new List<JsonTreeNode>();
      if (!string.IsNullOrEmpty(classId))
      {
        QMXF dataEntities = _refdataRepository.GetClasses(classId);
        foreach (var entity in dataEntities.classDefinitions)
        {
          JsonTreeNode node = new JsonTreeNode
          {
            nodeType = "async",
            type = searchtype,
            icon = "Content/img/class.png",
            identifier = entity.identifier.Split('#')[1],
            id = (entity.name[0].value).GetHashCode().ToString(),
            text = entity.name[0].value,
            expanded = false,
            leaf = false,
            children = new List<JsonTreeNode>(),
            record = entity
          };
          JsonTreeNode clasifNode = new JsonTreeNode
          {
            id = ("Classifications" + node.text).GetHashCode().ToString(),
            children = null,
            leaf = false,
            text = "Classifications",
            expanded = false
          };
          node.children.Add(clasifNode);
          JsonTreeNode supersNode = new JsonTreeNode
          {
            id = ("Superclasses" + node.text).GetHashCode().ToString(),
            children = null,
            leaf = false,
            text = "Superclasses",
            expanded = false
          };
          node.children.Add(supersNode);
          JsonTreeNode subsNode = new JsonTreeNode
          {
            id = ("Subclasses" + node.text).GetHashCode().ToString(),
            children = null,
            leaf = false,
            text = "Subclasses",
            expanded = false
          };
          node.children.Add(subsNode);
          JsonTreeNode tempsNode = new JsonTreeNode
          {
            id = ("Templates" + node.text).GetHashCode().ToString(),
            children = null,
            leaf = false,
            text = "Templates",
            expanded = false
          };
          node.children.Add(tempsNode);
          nodes.Add(node);
        }
      }
      return Json(nodes, JsonRequestBehavior.AllowGet);
    }
  }
}

