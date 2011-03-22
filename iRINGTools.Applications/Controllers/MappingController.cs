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

using iRINGTools.Web.Helpers;
using iRINGTools.Web.Models;

namespace iRINGTools.Web.Controllers
{
  public class MappingController : Controller
  {

    NameValueCollection _settings = null;
    string _adapterServiceURI = String.Empty;
    string _refDataServiceURI = String.Empty;

    public MappingController()
    {
      _settings = ConfigurationManager.AppSettings;
      _adapterServiceURI = _settings["AdapterServiceUri"];
      _refDataServiceURI = _settings["ReferenceDataServiceUri"];
    }   

    //
    // GET: /Mapping/

    public ActionResult Index()
    {
      return View();
    }

    public JsonResult GetNode(FormCollection form) 
    {

      string format = String.Empty;
      string adapterServiceURI = _adapterServiceURI;

      if (Request.QueryString["format"] != null)
        format = Request.QueryString["format"].ToUpper();

      if (Request.QueryString["remote"] != null)
        adapterServiceURI = Request.QueryString["remote"] + "/adapter";

      WebHttpClient client = new WebHttpClient(adapterServiceURI);

      switch (form["type"]) {
        case "GraphNode" :
          {
            string context = form["node"];

            Mapping mapping = client.Get<Mapping>(String.Format("/{0}/{1}/mapping", context.Split('/')[0], context.Split('/')[1]), true);

            GraphMap graph = mapping.graphMaps.FirstOrDefault(o => o.name == context.Split('/')[2]);

            List<JsonTreeNode> nodes = new List<JsonTreeNode>();

            foreach (ClassTemplateMap template in graph.classTemplateMaps)
            {
              JsonTreeNode node = new JsonTreeNode
              {
                nodeType = "async",
                type = "ClassTemplateNode",
                icon = "Content/img/class-map.png",
                id = context + "/" + template.classMap.name,
                text = template.classMap.name,
                expanded = false,
                leaf = false,
                children = null
              };

              nodes.Add(node);
            }

            return Json(nodes, JsonRequestBehavior.AllowGet);
          }
        case "ClassTemplateNode":
          {
            string context = form["node"];

            Mapping mapping = client.Get<Mapping>(String.Format("/{0}/{1}/mapping", context.Split('/')[0], context.Split('/')[1]), true);

            GraphMap graph = mapping.graphMaps.FirstOrDefault(o => o.name == context.Split('/')[2]);
            ClassTemplateMap classTemplate = graph.classTemplateMaps.FirstOrDefault(o => o.classMap.name == context.Split('/')[3]);

            List<JsonTreeNode> nodes = new List<JsonTreeNode>();
                        
            foreach (TemplateMap template in classTemplate.templateMaps)
            {
              JsonTreeNode node = new JsonTreeNode
              {
                nodeType = "async",
                type = "TemplateNode",
                icon = "Content/img/valuelist.png",
                id = context + "/" + template.name,
                text = template.name,
                expanded = false,
                leaf = false,
                children = null
              };

              nodes.Add(node);
            }

            return Json(nodes, JsonRequestBehavior.AllowGet);
          }
        case "TemplateNode":
          {
            string context = form["node"];

            Mapping mapping = client.Get<Mapping>(String.Format("/{0}/{1}/mapping", context.Split('/')[0], context.Split('/')[1]), true);

            GraphMap graph = mapping.graphMaps.FirstOrDefault(o => o.name == context.Split('/')[2]);
            ClassTemplateMap classTemplate = graph.classTemplateMaps.FirstOrDefault(o => o.classMap.name == context.Split('/')[3]);
            TemplateMap template = classTemplate.templateMaps.FirstOrDefault(o => o.name == context.Split('/')[4]);

            List<JsonTreeNode> nodes = new List<JsonTreeNode>();

            foreach (RoleMap role in template.roleMaps)
            {
              JsonTreeNode node = new JsonTreeNode
              {
                nodeType = "async",
                type = "TemplateNode",
                icon = "Content/img/role.png",
                id = context + "/" + role.name,
                text = role.name,
                expanded = false,
                leaf = false,
                children = null
              };

              nodes.Add(node);
            }

            return Json(nodes, JsonRequestBehavior.AllowGet);
          }
        default: 
          {
            break;
          }
      }

      return Json(new { success = false }, JsonRequestBehavior.AllowGet);
    }

  }
}
