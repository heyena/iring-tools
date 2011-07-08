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
using org.ids_adi.qmxf;
using VDS.RDF;

namespace iRINGTools.Web.Controllers
{
  public class MappingController : Controller
  {
    NamespaceMapper _nsMap = new NamespaceMapper();
    private NameValueCollection _settings = null;
    private RefDataRepository _refdata = null;
    private IMappingRepository _repository { get; set; }
    private string _keyFormat = "Mapping.{0}.{1}";
    private const string unMappedToken = "[unmapped]";
    private char[] delimiters = new char[] { '/' };
    private bool qn = false;
    private string qName = string.Empty;

    public MappingController()
      : this(new MappingRepository())
    { }
    public MappingController(IMappingRepository repository)
    {
      _settings = ConfigurationManager.AppSettings;
      _repository = repository;
      _refdata = new RefDataRepository();
      _nsMap.AddNamespace("eg", new Uri("http://example.org/data#"));
      _nsMap.AddNamespace("owl", new Uri("http://www.w3.org/2002/07/owl#"));
      _nsMap.AddNamespace("rdl", new Uri("http://rdl.rdlfacade.org/data#"));
      _nsMap.AddNamespace("tpl", new Uri("http://tpl.rdlfacade.org/data#"));
      _nsMap.AddNamespace("dm", new Uri("http://dm.rdlfacade.org/data#"));
      _nsMap.AddNamespace("p8dm", new Uri("http://standards.tc184-sc4.org/iso/15926/-8/data-model#"));
      _nsMap.AddNamespace("owl2xml", new Uri("http://www.w3.org/2006/12/owl2-xml#"));
      _nsMap.AddNamespace("p8", new Uri("http://standards.tc184-sc4.org/iso/15926/-8/template-model#"));
      _nsMap.AddNamespace("templates", new Uri("http://standards.tc184-sc4.org/iso/15926/-8/templates#"));
    }

    //
    // GET: /Mapping/

    public ActionResult Index()
    {
      return View();
    }

    private Mapping GetMapping(string scope, string application)
    {
      string key = string.Format(_keyFormat, scope, application);

      if (Session[key] == null)
      {
        Session[key] = _repository.GetMapping(scope, application);
      }

      return (Mapping)Session[key];
    }

    public JsonResult AddClassMap(FormCollection form)
    {
      JsonTreeNode nodes = new JsonTreeNode();
      try
      {

        string propertyCtx = form["objectName"];
        string mappingNode = form["mappingNode"];
        if (string.IsNullOrEmpty(propertyCtx)) throw new Exception("Object Name/Property Name has no value");
        string[] dataObjectVars = propertyCtx.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
        string[] mappingVars = mappingNode.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
        string scope = dataObjectVars[0];
        string application = dataObjectVars[1];
        string dataObject = dataObjectVars[4];
        string propertyName = dataObjectVars[5];
        string graphName = mappingVars[2];
        string templateName = mappingVars[3];
        string roleName = mappingVars[4];
        string classId = form["classUrl"];
        string classLabel = form["classLabel"];

        ClassMap classMap = new ClassMap();
        Mapping mapping = GetMapping(scope, application);
        GraphMap graphMap = mapping.FindGraphMap(graphName);

        foreach (var classTemplateMaps in graphMap.classTemplateMaps)
        {
          foreach (var tMap in classTemplateMaps.templateMaps)
          {
            if (tMap.name == templateName)
            {
              foreach (var role in tMap.roleMaps)
              {
                if (role.name == roleName)
                {
                  qn = _nsMap.ReduceToQName(classId, out qName);
                  role.type = RoleType.Reference;
                  role.value = qn ? qName : classId;
                  classMap.name = classLabel;
                  classMap.id = qn ? qName : classId;
                  classMap.identifiers = new Identifiers();
                  classMap.identifiers.Add(string.Format("{0}.{1}", dataObject, propertyName));
                  graphMap.AddClassMap(role, classMap);
                  role.classMap = classMap;
                  nodes.children.Add(GetClassNode(classMap, mappingNode));
                  break;
                }
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        return Json(nodes, JsonRequestBehavior.AllowGet);
      }
      return Json(nodes, JsonRequestBehavior.AllowGet);
    }

    public JsonResult MapReference(FormCollection form)
    {
      try
      {
        string qName = string.Empty;
        string templateName = string.Empty;
        string format = String.Empty;
        string propertyCtx = form["ctx"];
        string reference = string.Empty;
        bool qn = _nsMap.ReduceToQName(form["reference"], out reference);
        string classId = form["classId"];
        string label = form["label"];
        string roleName = form["roleName"];
        string roleId = form["roleId"];
        string[] dataObjectVars = propertyCtx.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
        string scope = dataObjectVars[0];
        string application = dataObjectVars[1];
        string graph = dataObjectVars[2];
        ///find the correct template
        for (int i = dataObjectVars.Length - 1; i >= 0; i--)
        {
          if (dataObjectVars[i] == roleName)
          {
            templateName = dataObjectVars[i - 1];
            continue;
          }
        }
        Mapping mapping = GetMapping(scope, application);
        GraphMap graphMap = mapping.FindGraphMap(graph);

        ClassTemplateMap cmap = graphMap.GetClassTemplateMap(classId);

        TemplateMap tmap = cmap.templateMaps.FirstOrDefault(c => c.name == templateName);
        RoleMap roleMap = tmap.roleMaps.FirstOrDefault(c => c.name == roleName);
        roleMap.type = RoleType.Reference;
        roleMap.value = reference;


      }
      catch
      {
        return Json(new { success = false }, JsonRequestBehavior.AllowGet);
      }
      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

    public JsonResult AddTemplateMap(FormCollection form)
    {
      JsonTreeNode nodes = new JsonTreeNode();
      try
      {
        string qName = string.Empty;

        string format = String.Empty;
        string propertyCtx = form["ctx"];
        string nodeType = form["nodetype"];
        string parentType = form["parentType"];
        string parentId = form["parentId"];
        string identifier = form["id"];

        if (string.IsNullOrEmpty(propertyCtx)) throw new Exception("ObjectName has no value");

        string[] dataObjectVars = propertyCtx.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
        string scope = dataObjectVars[0];
        string application = dataObjectVars[1];
        string graph = dataObjectVars[2];
        string context = string.Format("{0}/{1}", scope, application);
        ClassMap selectedClassMap = null;
        Mapping mapping = GetMapping(scope, application);
        GraphMap graphMap = mapping.FindGraphMap(graph);
        ClassMap graphClassMap = graphMap.classTemplateMaps.FirstOrDefault().classMap;
        QMXF newtemplate = _refdata.GetTemplate(identifier);
        if (parentType == "GraphMapNode")
        {
          selectedClassMap = graphMap.classTemplateMaps.FirstOrDefault().classMap;
        }
        else if (parentType == "ClassMapNode")
        {
          foreach (var classTemplateMap in graphMap.classTemplateMaps)
          {
            if (classTemplateMap.classMap.id == parentId)
            {
              selectedClassMap = classTemplateMap.classMap;
              break;
            }
          }

        }
        object template = null;
        TemplateMap templateMap = new TemplateMap();
        if (newtemplate.templateDefinitions.Count > 0)
        {
          // graphMap.AddTemplateMap(
          foreach (var defs in newtemplate.templateDefinitions)
          {
            template = defs;
            templateMap.id = defs.identifier;
            templateMap.name = defs.name[0].value;
            templateMap.type = TemplateType.Definition;
            GetRoleMaps(selectedClassMap.id, template, templateMap);
          }
        }
        else
        {
          foreach (var quals in newtemplate.templateQualifications)
          {
            template = quals;
            templateMap.id = quals.identifier;
            templateMap.name = quals.name[0].value;
            templateMap.type = TemplateType.Qualification;
            GetRoleMaps(selectedClassMap.id, template, templateMap);
          }
        }
        graphMap.AddTemplateMap(selectedClassMap, templateMap);
      }
      catch (Exception ex)
      {
        return Json(nodes, JsonRequestBehavior.AllowGet);
      }
      return Json(nodes, JsonRequestBehavior.AllowGet);
    }


    public JsonResult GetNode(FormCollection form)
    {
      GraphMap graphMap = null;
      ClassMap graphClassMap = null;
      string format = String.Empty;
      string context = form["node"];
      string[] formgraph = form["graph"].Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
      string[] variables = context.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
      string scope = variables[0];
      string application = variables[1];
      string graphName = formgraph[formgraph.Count() - 1];
      string key = string.Format(_keyFormat, scope, application);

      Mapping mapping = GetMapping(scope, application);

      List<JsonTreeNode> nodes = new List<JsonTreeNode>();
      if (!string.IsNullOrEmpty(graphName))
        graphMap = mapping.graphMaps.FirstOrDefault<GraphMap>(o => o.name == graphName);

      if (graphMap != null)
      {
        graphClassMap = graphMap.classTemplateMaps.FirstOrDefault().classMap;

        switch (form["type"])
        {
          case "MappingNode":
            foreach (var graph in mapping.graphMaps)
            {
              if (graphMap != null && graphMap.name != graph.name) continue;
              JsonTreeNode graphNode = GetGraphNode(graph, context);

              nodes.Add(graphNode);
            }
            break;
          case "GraphMapNode":
            if (graphMap != null)
            {

              foreach (var templateMaps in graphMap.classTemplateMaps)
              {
                if (templateMaps.classMap.name != graphClassMap.name) continue;
                foreach (var templateMap in templateMaps.templateMaps)
                {

                  JsonTreeNode templateNode = GetTemplateNode(templateMap, context);
                  nodes.Add(templateNode);
                }
              }
            }
            break;
          case "ClassMapNode":
            var classMapId = form["id"];
            if (graphMap != null)
            {
              foreach (var classTemplateMap in graphMap.classTemplateMaps)
              {
                if (classTemplateMap.classMap.id == classMapId)
                {
                  foreach (var templateMap in classTemplateMap.templateMaps)
                  {
                    JsonTreeNode templateNode = GetTemplateNode(templateMap, context);
                    nodes.Add(templateNode);
                  }
                  break;
                }
              }
            }
            break;
          case "TemplateMapNode":
            var templateId = form["id"];
            if (graphMap != null)
            {
              string className = graphClassMap.name;
              if (variables.Count() > 4)
              {
                className = variables[variables.Count() - 2];
              }

              ClassTemplateMap classTemplateMap =
                graphMap.classTemplateMaps.Find(ctm => ctm.classMap.name == className);

              if (classTemplateMap == null) break;
              TemplateMap templateMap =
                classTemplateMap.templateMaps.Find(tm => tm.id == templateId);

              if (templateMap == null) break;
              foreach (var role in templateMap.roleMaps)
              {
                JsonTreeNode roleNode = GetRoleNode(role, context);
                nodes.Add(roleNode);
                if (role.classMap != null && role.classMap.id != graphClassMap.id)
                {
                  JsonTreeNode classNode = GetClassNode(role.classMap, context);
                  if (roleNode.children == null)
                    roleNode.children = new List<JsonTreeNode>();
                  roleNode.children.Add(classNode);
                }
                else
                {
                  roleNode.leaf = true;
                }
              }
            }
            break;
          case "RoleMapNode":
            break;
        }
      }
      return Json(nodes, JsonRequestBehavior.AllowGet);
    }

    public JsonResult DeleteClassMap(FormCollection form)
    {
      try
      {
        string mappingNode = form["mappingNode"];

        string[] dataObjectVars = mappingNode.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
        string scope = dataObjectVars[0];
        string application = dataObjectVars[1];
        string graph = dataObjectVars[2];
        string templateName = dataObjectVars[3];
        string templateId = form["templateId"];
        string classId = form["classId"];
        string roleId = form["roleId"];
        string roleName = dataObjectVars[dataObjectVars.Count() - 2];
        string className = dataObjectVars[dataObjectVars.Count() - 1];
        Mapping mapping = GetMapping(scope, application);
        GraphMap graphMap = mapping.FindGraphMap(graph);
        TemplateMap tMap = null;
        RoleMap rMap = null;
        foreach (ClassTemplateMap ctm in graphMap.classTemplateMaps)
        {
          TemplateMap t = ctm.templateMaps.Find(tm => tm.id == templateId);
          if (t != null)
          {
            RoleMap r = t.roleMaps.Find(rm => rm.id == roleId);
            if (r.classMap != null && r.classMap.id == classId)
            {
              rMap = r;
              tMap = t;
              break;
            }
          }
        }
        if (tMap != null && rMap != null)
          graphMap.DeleteRoleMap(tMap, rMap.id);
      }
      catch (Exception ex)
      {
        return Json(new { success = false }, JsonRequestBehavior.AllowGet);
      }
      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

    public JsonResult ResetMapping(FormCollection form)
    {
      try
      {
        string roleId = form["roleId"];
        string templateId = form["templateId"];
        string classId = form["parentClassId"];
        string mappingNode = form["mappingNode"];
        string[] dataObjectVars = mappingNode.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
        string scope = dataObjectVars[0];
        string application = dataObjectVars[1];
        string graphName = dataObjectVars[2];
        Mapping mapping = GetMapping(scope, application);
        GraphMap graphMap = mapping.FindGraphMap(graphName);
        ClassTemplateMap ctMap = graphMap.GetClassTemplateMap(classId);
        TemplateMap tMap = ctMap.templateMaps.Find(t => t.id.Equals(templateId));
        RoleMap rm = tMap.roleMaps.Find(r => r.id.Equals(roleId));
        if (!string.IsNullOrEmpty(rm.propertyName) || rm.dataType.StartsWith("xsd"))
          rm.type = RoleType.DataProperty;
        else
          rm.type = RoleType.Reference;
        rm.propertyName = null;
        rm.value = null;
        rm.valueListName = null;
        rm.classMap = null;

        rm.type = RoleType.Reference;
      }
      catch
      {
        return Json(new { success = false }, JsonRequestBehavior.AllowGet);
      }
      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

    public JsonResult MakePossessor(FormCollection form)
    {
      List<JsonTreeNode> nodes = new List<JsonTreeNode>();

      try
      {
        string mappingNode = form["mappingNode"];

        string[] dataObjectVars = mappingNode.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
        string scope = dataObjectVars[0];
        string application = dataObjectVars[1];
        string graphName = dataObjectVars[2];
        string classId = form["classId"];
        string templateName = dataObjectVars[dataObjectVars.Count() - 2];
        string roleName = dataObjectVars[dataObjectVars.Count() - 1];
        string context = string.Format("{0}/{1}", scope, application);
        Mapping mapping = GetMapping(scope, application);
        GraphMap graphMap = mapping.FindGraphMap(graphName);
        foreach (var ctemplateMaps in graphMap.classTemplateMaps)
        {
          if (!ctemplateMaps.classMap.id.Equals(classId)) continue;
          TemplateMap tmap = ctemplateMaps.templateMaps.FirstOrDefault(c => c.name == templateName);
          RoleMap roleMap = tmap.roleMaps.FirstOrDefault(c => c.name == roleName);
          roleMap.type = RoleType.Possessor;
          roleMap.value = string.Empty;
          JsonTreeNode rolenode = GetRoleNode(roleMap, context);
          rolenode.text.Replace(unMappedToken, "");
          nodes.Add(rolenode);
        }
      }
      catch (Exception ex)
      {
        return Json(nodes, JsonRequestBehavior.AllowGet);
      }
      return Json(nodes, JsonRequestBehavior.AllowGet);
    }

    private JsonTreeNode GetRoleNode(RoleMap role, string context)
    {
      JsonTreeNode templateNode = new JsonTreeNode
      {
        nodeType = "async",
        type = "RoleMapNode",
        icon = "Content/img/role-map.png",
        id = context + "/" + role.name,
        text = role.IsMapped() ? string.Format("{0}{1}", role.name, "") :
                                 string.Format("{0}{1}", role.name, unMappedToken),
        expanded = false,
        leaf = false,
        children = null,
        record = role
      };
      return templateNode;
    }

    private JsonTreeNode GetClassNode(ClassMap classMap, string context)
    {
      JsonTreeNode classNode = new JsonTreeNode
      {
        identifier = classMap.id,
        nodeType = "async",
        type = "ClassMapNode",
        icon = "Content/img/class-map.png",
        id = context + "/" + classMap.name,
        text = classMap.name,
        expanded = false,
        leaf = false,
        children = null,
        record = classMap
      };
      return classNode;
    }

    private JsonTreeNode GetTemplateNode(TemplateMap templateMap, string context)
    {
      JsonTreeNode templateNode = new JsonTreeNode
      {
        nodeType = "async",
        identifier = templateMap.id,
        type = "TemplateMapNode",
        icon = "Content/img/template-map.png",
        id = context + "/" + templateMap.name,
        text = templateMap.name,
        expanded = false,
        leaf = false,
        children = null,
        record = templateMap
      };
      return templateNode;
    }

    private JsonTreeNode GetGraphNode(GraphMap graph, string context)
    {
      JsonTreeNode graphNode = new JsonTreeNode
      {
        nodeType = "async",
        identifier = graph.classTemplateMaps.FirstOrDefault().classMap.id,
        type = "GraphMapNode",
        icon = "Content/img/class-map.png",
        id = context + "/" + graph.name,
        text = graph.name,
        expanded = false,
        leaf = false,
        children = null,
        record = graph

      };
      return graphNode;
    }

		public ActionResult graphMap(FormCollection form)
		{
			List<JsonTreeNode> nodes = new List<JsonTreeNode>();
			try
			{
				string qName = string.Empty;
				string format = String.Empty;
				string oldGraphName = "";

				string[] mappingCtx = form["mappingNode"].Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
				if (mappingCtx.Length > 3)
					oldGraphName = mappingCtx[4];
				string propertyCtx = form["objectName"];
				if (string.IsNullOrEmpty(propertyCtx)) throw new Exception("ObjectName has no value");
				string[] dataObjectVars = propertyCtx.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
				string scope = dataObjectVars[0];
				string application = dataObjectVars[1];
				Mapping mapping = GetMapping(scope, application);
				string context = string.Format("{0}/{1}", scope, application);
				string newGraphName = form["graphName"];
				string classLabel = form["classLabel"];
				string keyProperty = dataObjectVars[5];
				string dataObject = dataObjectVars[4];
				string classId = form["classUrl"];

				bool qn = false;

				qn = _nsMap.ReduceToQName(classId, out qName);

				if (oldGraphName == "")
				{
					if (mapping.graphMaps == null)
						mapping.graphMaps = new GraphMaps();

					GraphMap graphMap = new GraphMap
					{
						name = newGraphName,
						dataObjectName = dataObject
					};
					ClassMap classMap = new ClassMap
					{
						name = classLabel,
						id = qn ? qName : classId
					};

					classMap.identifiers.Add(string.Format("{0}.{1}", dataObject, keyProperty));

					graphMap.AddClassMap(null, classMap);

					mapping.graphMaps.Add(graphMap);
					nodes.Add(GetGraphNode(graphMap, context));
				}
				else
				{
					GraphMap graphMap = mapping.FindGraphMap(oldGraphName);
					if (graphMap == null)
						graphMap = new GraphMap();

					graphMap.name = newGraphName;
					graphMap.dataObjectName = dataObject;

					ClassTemplateMaps ct = graphMap.classTemplateMaps;
					for (int i = 0; i < ct.Count; i++)
					{
						ct.RemoveAt(i);
					}

					ClassMap classMap = new ClassMap
					{
						name = classLabel,
						id = qn ? qName : classId
					};

					classMap.identifiers.Add(string.Format("{0}.{1}", dataObject, keyProperty));

					graphMap.AddClassMap(null, classMap);
					_repository.UpdateMapping(scope, application, mapping);
				}

			}
			catch (Exception ex)
			{
				return Json(nodes, JsonRequestBehavior.AllowGet);
			}
			return Json(new { success = true }, JsonRequestBehavior.AllowGet);
		}


    public JsonResult AddGraphMap(FormCollection form)
    {
      List<JsonTreeNode> nodes = new List<JsonTreeNode>();
      try
      {
        string qName = string.Empty;

        string format = String.Empty;
        string propertyCtx = form["objectName"];
        if (string.IsNullOrEmpty(propertyCtx)) throw new Exception("ObjectName has no value");
        string[] dataObjectVars = propertyCtx.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
        string scope = dataObjectVars[0];
        string application = dataObjectVars[1];
        Mapping mapping = GetMapping(scope, application);
        string context = string.Format("{0}/{1}", scope, application);
        string graphName = form["graphName"];
        string classLabel = form["classLabel"];
        string keyProperty = dataObjectVars[5];
        string dataObject = dataObjectVars[4];
        string classId = form["classUrl"];

        bool qn = false;

        qn = _nsMap.ReduceToQName(classId, out qName);

        GraphMap graphMap = new GraphMap
        {
          name = graphName,
          dataObjectName = dataObject
        };
        ClassMap classMap = new ClassMap
        {
          name = classLabel,
          id = qn ? qName : classId
        };

        classMap.identifiers.Add(string.Format("{0}.{1}", dataObject, keyProperty));

        graphMap.AddClassMap(null, classMap);
        if (mapping.graphMaps == null)
          mapping.graphMaps = new GraphMaps();
        mapping.graphMaps.Add(graphMap);
        nodes.Add(GetGraphNode(graphMap, context));
      }
      catch (Exception ex)
      {
        return Json(nodes, JsonRequestBehavior.AllowGet);
      }
      return Json(nodes, JsonRequestBehavior.AllowGet);
    }

    public JsonResult UpdateMapping(FormCollection form)
    {
      string scope = form["scope"];
      string application = form["application"];
      Mapping mapping = GetMapping(scope, application);
      try
      {
        _repository.UpdateMapping(scope, application, mapping);
      }
      catch (Exception ex)
      {
        return Json(new { success = false }, JsonRequestBehavior.AllowGet);
      }
      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

    public JsonResult DeleteGraphMap(FormCollection form)
    {

      try
      {
        string scope = form["scope"];
        string application = form["application"];
        Mapping mapping = GetMapping(scope, application);
        string graphName = form["mappingNode"].Split('/')[4];
        GraphMap graphMap = mapping.FindGraphMap(graphName);
        if (graphMap != null)
          mapping.graphMaps.Remove(graphMap);
      }
      catch (Exception ex)
      {
        return Json(new { success = false }, JsonRequestBehavior.AllowGet);
      }
      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

    public JsonResult MapProperty(FormCollection form)
    {
      try
      {
        string mappingNode = form["mappingNode"];
        string propertyName = form["propertyName"];
        string[] mappingCtx = mappingNode.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
        string scope = mappingCtx[0];
        string application = mappingCtx[1];
        string graphName = mappingCtx[2];
        string classId = form["classId"];
        string relatedObject = form["relatedObject"];
        string templateName = mappingCtx[mappingCtx.Count() - 2];
        string roleName = mappingCtx[mappingCtx.Count() - 1];

        Mapping mapping = GetMapping(scope, application);
        GraphMap graphMap = mapping.FindGraphMap(graphName);
        ClassTemplateMap ctMap = graphMap.GetClassTemplateMap(classId);
        TemplateMap tMap = ctMap.templateMaps.Find(t => t.name.Equals(templateName));
        RoleMap rm = tMap.roleMaps.Find(r => r.name.Equals(roleName));

        if (!string.IsNullOrEmpty(rm.dataType) && rm.dataType.StartsWith("xsd"))
        {
          if (relatedObject != "undefined" && relatedObject != "")
          {
            rm.propertyName = string.Format("{0}.{1}.{2}",
              graphMap.dataObjectName,
              relatedObject,
              propertyName);
          }
          else
          {
            rm.propertyName =
                string.Format("{0}.{1}", graphMap.dataObjectName, propertyName);
          }
        }
        else
        {
          throw new Exception("Please select a DataPRoperty or Property role...");
        }

      }
      catch (Exception ex)
      {
        return Json(new { success = false }, JsonRequestBehavior.AllowGet);
      }
      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }


    public JsonResult MapValueList(FormCollection form)
    {
      try
      {
        string mappingNode = form["mappingNode"];
        string propertyName = form["propertyName"];
        string objectNames = form["objectNames"];
        string[] propertyCtx = objectNames.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
        string[] mappingCtx = mappingNode.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
        string scope = propertyCtx[0];
        string classId = form["classId"];
        string application = propertyCtx[1];
        string graphName = mappingCtx[2];
        string templateName = mappingCtx[3];
        string roleName = mappingCtx[4];
        string valueListName = propertyCtx[propertyCtx.Length - 1];

        Mapping mapping = GetMapping(scope, application);
        GraphMap graphMap = mapping.FindGraphMap(graphName);

        foreach (var ctemplateMaps in graphMap.classTemplateMaps)
        {
          if (!ctemplateMaps.classMap.id.Equals(classId)) continue;
          TemplateMap tmap = ctemplateMaps.templateMaps.FirstOrDefault(c => c.name == templateName);
          RoleMap roleMap = tmap.roleMaps.FirstOrDefault(c => c.name == roleName);
          roleMap.valueListName = valueListName;
          roleMap.propertyName =
                string.Format("{0}.{1}", propertyName.Split(delimiters)[4], propertyName.Split(delimiters)[5]);
        }
      }
      catch (Exception ex)
      {
        return Json(new { success = false }, JsonRequestBehavior.AllowGet);
      }
      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

    public JsonResult DeleteTemplateMap(FormCollection form)
    {
      try
      {
        string scope = form["scope"];
        string application = form["application"];
        Mapping mapping = GetMapping(scope, application);
        string parentNode = form["mappingNode"].Split('/')[2];
        string templateId = form["identifier"];
        string parentClassId = form["parentIdentifier"];
        GraphMap graphMap = mapping.FindGraphMap(parentNode);

        ClassTemplateMap tmap = graphMap.GetClassTemplateMap(parentClassId);
        TemplateMap map = tmap.templateMaps.Find(t => t.id == templateId);
        tmap.templateMaps.Remove(map);

      }
      catch (Exception ex)
      {
        return Json(new { success = false }, JsonRequestBehavior.AllowGet);
      }
      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

    public JsonResult DeleteValueList(FormCollection form)
    {
      try
      {
        string[] context = form["mappingNode"].Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
        string scope = context[0];
        string application = context[1];
        Mapping mapping = GetMapping(scope, application);
        string deleteValueList = form["valueList"];
        var valueListMap = mapping.valueListMaps.Find(c => c.name == deleteValueList);
        if (valueListMap != null)
          mapping.valueListMaps.Remove(valueListMap);
        _repository.UpdateMapping(scope, application, mapping);
      }
      catch (Exception ex)
      {
        return Json(new { success = false }, JsonRequestBehavior.AllowGet);
      }
      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

   

    public JsonResult AddValueListMap(FormCollection form)
    {
      try
      {
        string[] context = form["mappingNode"].Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
        string scope = context[0];
        string valueList = context[4];
        string application = context[1];
        string internalName = form["internalName"];
        string classUrl = form["classUrl"];
        qn = _nsMap.ReduceToQName(classUrl, out qName);
        Mapping mapping = GetMapping(scope, application);
        ValueListMap valuelistMap = mapping.valueListMaps.Find(c => c.name == valueList);
        if (valueList != null)
        {
          ValueMap valueMap = new ValueMap
          {
            internalValue = internalName,
            uri = qName
          };
          if (valuelistMap.valueMaps == null)
            valuelistMap.valueMaps = new ValueMaps();
          valuelistMap.valueMaps.Add(valueMap);
          _repository.UpdateMapping(scope, application, mapping);
        }

      }
      catch (Exception ex)
      {
        return Json(new { success = false }, JsonRequestBehavior.AllowGet);
      }
      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }		

    public JsonResult AddValueList(FormCollection form)
    {
      try
      {
        string[] context = form["mappingNode"].Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
        string scope = context[0];
        string application = context[1];
        Mapping mapping = GetMapping(scope, application);
        string valueList = form["valueList"];
        var valueListMap = mapping.valueListMaps.Find(c => c.name == valueList);
        if (valueListMap == null)
        {
          ValueListMap valuelistMap = new ValueListMap
          {
            name = valueList
          };

          mapping.valueListMaps.Add(valuelistMap);
          _repository.UpdateMapping(scope, application, mapping);
        }
      }
      catch (Exception ex)
      {
        return Json(new { success = false }, JsonRequestBehavior.AllowGet);
      }
      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

		public JsonResult EditValuelist(FormCollection form)
		{
			try
			{
				string[] context = form["mappingNode"].Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
				string scope = context[0];
				string oldValueList = context[context.Count() - 1];
				string application = context[1];
				Mapping mapping = GetMapping(scope, application);
				string newvalueList = form["valueList"];
				var valueListMap = mapping.valueListMaps.Find(c => c.name == oldValueList);
				if (valueListMap != null)
				{
					valueListMap.name = newvalueList;
					_repository.UpdateMapping(scope, application, mapping);
				}

			}
			catch (Exception ex)
			{
				return Json(new { success = false }, JsonRequestBehavior.AllowGet);
			}
			return Json(new { success = true }, JsonRequestBehavior.AllowGet);
		}

		public ActionResult valueList(FormCollection form)
		{
			try
			{
				string oldValueList = "";
				ValueListMap valueListMap = null;

				string[] context = form["mappingNode"].Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
				string scope = context[0];
				
				if (context.Count() >= 1)
					oldValueList = context[context.Count() - 1];

				string application = context[1];
				Mapping mapping = GetMapping(scope, application);
				string newvalueList = form["valueList"];

				if (oldValueList != "")
					valueListMap = mapping.valueListMaps.Find(c => c.name == oldValueList);
				else
					valueListMap = mapping.valueListMaps.Find(c => c.name == newvalueList);

				if (valueListMap == null)
				{
					ValueListMap valuelistMap = new ValueListMap
					{
						name = newvalueList
					};

					mapping.valueListMaps.Add(valuelistMap);
					_repository.UpdateMapping(scope, application, mapping);
				}
				else
				{
					valueListMap.name = newvalueList;
					_repository.UpdateMapping(scope, application, mapping);
				}
			}
			catch (Exception ex)
			{
				return Json(new { success = false }, JsonRequestBehavior.AllowGet);
			}
			return Json(new { success = true }, JsonRequestBehavior.AllowGet);
		}

    public JsonResult EditGraphName(FormCollection form)
    {
      try
      {

        string[] mappingCtx = form["mappingNode"].Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
        string oldGraphName = mappingCtx[4];
        string newGraphName = form["graphName"];
        string scope = mappingCtx[0];
        string application = mappingCtx[1];
        Mapping mapping = GetMapping(scope, application);
        GraphMap graphMap = mapping.FindGraphMap(oldGraphName);
        graphMap.name = newGraphName;
        _repository.UpdateMapping(scope, application, mapping);
      }
      catch (Exception ex)
      {
        return Json(new { success = false }, JsonRequestBehavior.AllowGet);
      }
      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }
    public JsonResult GetLabels(FormCollection form)
    {
      JsonArray jsonArray = new JsonArray();
      try
      {

        string scope = form["Scope"];
        string application = form["Application"];
        string recordId = form["recordId"];
        string roleType = form["roleType"];
        string roleValue = form["roleValue"];
        if (!string.IsNullOrEmpty(recordId))
        {

          //   jsonArray.Add( "recordId", _refdata.GetClassLabel(recordId.Split(':')[1]));
        }
      }
      catch (Exception ex)
      {
        return Json(jsonArray, JsonRequestBehavior.AllowGet);
      }
      return Json(jsonArray, JsonRequestBehavior.AllowGet);
    }

    private void GetRoleMaps(string classId, object template, TemplateMap currentTemplateMap)
    {
      bool qn = false;
      string qName = string.Empty;
      if (currentTemplateMap.roleMaps == null)
        currentTemplateMap.roleMaps = new RoleMaps();

      if (template is TemplateDefinition)
      {
        TemplateDefinition templateDefinition = (TemplateDefinition)template;
        List<RoleDefinition> roleDefinitions = templateDefinition.roleDefinition;

        foreach (RoleDefinition roleDefinition in roleDefinitions)
        {

          string range = roleDefinition.range;
          RoleMap roleMap = new RoleMap();
          if (range != classId && range.StartsWith("xsd:"))
          {
            qn = _nsMap.ReduceToQName(range, out qName);
            roleMap.type = RoleType.DataProperty;
            roleMap.name = roleDefinition.name.FirstOrDefault().value;
            roleMap.dataType = qn ? qName : range;
            roleMap.propertyName = string.Empty;
            qn = _nsMap.ReduceToQName(roleDefinition.identifier, out qName);
            roleMap.id = qn ? qName : roleDefinition.identifier;

            currentTemplateMap.roleMaps.Add(roleMap);
          }
          else if (range != classId && !range.StartsWith("xsd:"))
          {

            roleMap.type = RoleType.ObjectProperty;
            roleMap.name = roleDefinition.name.FirstOrDefault().value;
            qn = _nsMap.ReduceToQName(range, out qName);
            roleMap.dataType = qn ? qName : range;
            roleMap.propertyName = string.Empty;
            qn = _nsMap.ReduceToQName(roleDefinition.identifier, out qName);
            roleMap.id = qn ? qName : roleDefinition.identifier;
            currentTemplateMap.roleMaps.Add(roleMap);
          }
          else if (range == classId)
          {
            roleMap.type = RoleType.Possessor;
            roleMap.name = roleDefinition.name.FirstOrDefault().value;
            qn = _nsMap.ReduceToQName(range, out qName);
            roleMap.dataType = qn ? qName : range;
            roleMap.propertyName = string.Empty;
            qn = _nsMap.ReduceToQName(roleDefinition.identifier, out qName);
            roleMap.id = qn ? qName : roleDefinition.identifier;
            currentTemplateMap.roleMaps.Add(roleMap);
          }
        }
      }
      if (template is TemplateQualification)
      {
        TemplateQualification templateQualification = (TemplateQualification)template;
        List<RoleQualification> roleQualifications = templateQualification.roleQualification;

        foreach (RoleQualification roleQualification in roleQualifications)
        {
          qn = _nsMap.ReduceToQName(roleQualification.qualifies, out qName);
          string range = roleQualification.range;
          RoleMap roleMap = new RoleMap();

          roleMap.name = roleQualification.name.FirstOrDefault().value;
          roleMap.id = qn ? qName : roleQualification.qualifies;

          if (roleQualification.value != null)  // fixed role
          {
            if (!String.IsNullOrEmpty(roleQualification.value.reference))
            {
              roleMap.type = RoleType.Reference;
              qn = _nsMap.ReduceToQName(roleQualification.value.reference, out qName);
              roleMap.value = qn ? qName : roleQualification.value.reference;
            }
            else if (!String.IsNullOrEmpty(roleQualification.value.text))  // fixed role is a literal
            {
              roleMap.type = RoleType.FixedValue;
              roleMap.value = roleQualification.value.text;
              roleMap.dataType = roleQualification.value.As;
            }

            currentTemplateMap.roleMaps.Add(roleMap);
          }
          else if (range != classId && range.StartsWith("xsd:")) // property role
          {
            roleMap.type = RoleType.DataProperty;
            qn = _nsMap.ReduceToQName(range, out qName);
            roleMap.dataType = qn ? qName : range;
            roleMap.propertyName = String.Empty;
            currentTemplateMap.roleMaps.Add(roleMap);
          }
          else if (range != classId && !range.StartsWith("xsd:"))
          {
            roleMap.type = RoleType.ObjectProperty;
            qn = _nsMap.ReduceToQName(range, out qName);
            roleMap.dataType = qn ? qName : range;
            roleMap.propertyName = String.Empty;
            currentTemplateMap.roleMaps.Add(roleMap);
          }

          else if (range == classId)    // class role
          {
            roleMap.type = RoleType.Possessor;

            qn = _nsMap.ReduceToQName(range, out qName);
            roleMap.dataType = qn ? qName : range;
            currentTemplateMap.roleMaps.Add(roleMap);
          }
        }
      }
    }
  }
}
