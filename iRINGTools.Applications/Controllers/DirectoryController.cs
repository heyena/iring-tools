using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using iRINGTools.Web.Helpers;
using iRINGTools.Web.Models;

using org.iringtools.library;
using org.iringtools.mapping;

namespace iRINGTools.Web.Controllers
{
    public class DirectoryController : Controller
    {

        IAdapterRepository _repository;
        private string _keyFormat = "Mapping.{0}.{1}";

        public DirectoryController()
            : this(new AdapterRepository())
        {
        }

        public DirectoryController(IAdapterRepository repository)
        {
            _repository = repository;
        }

        public ActionResult Index()
        {
            return View(_repository.GetScopes());
        }

        public ActionResult GetNode(FormCollection form)
        {

            switch (form["type"])
            {
                case "ScopesNode":
                    {

                        List<JsonTreeNode> nodes = new List<JsonTreeNode>();

                        foreach (ScopeProject scope in _repository.GetScopes())
                        {

                            JsonTreeNode node = new JsonTreeNode
                            {
                                nodeType = "async",
                                type = "ScopeNode",
                                icon = "Content/img/system-file-manager.png",
                                id = scope.Name,
                                text = scope.Name,
                                expanded = false,
                                leaf = false,
                                children = null,
                                record = scope
                            };

                            nodes.Add(node);

                        }

                        return Json(nodes, JsonRequestBehavior.AllowGet);
                    }
                case "ScopeNode":
                    {

                        List<JsonTreeNode> nodes = new List<JsonTreeNode>();

                        ScopeProject scope = _repository.GetScope(form["node"]);

                        foreach (ScopeApplication application in scope.Applications)
                        {
                            DataLayer dataLayer = _repository.GetDataLayer(scope.Name, application.Name);

                            JsonTreeNode node = new JsonTreeNode
                            {
                                nodeType = "async",
                                type = "ApplicationNode",
                                icon = "Content/img/applications-internet.png",
                                id = scope.Name + "/" + application.Name,
                                text = application.Name,
                                expanded = false,
                                leaf = false,
                                children = null,
                                record = new
                                {
                                    Name = application.Name,
                                    Description = application.Description,
                                    DataLayer = dataLayer.Name,
                                    Assembly = dataLayer.Assembly
                                }
                            };

                            nodes.Add(node);

                        }

                        return Json(nodes, JsonRequestBehavior.AllowGet);
                    }
                case "ApplicationNode":
                    {
                        string context = form["node"];

                        List<JsonTreeNode> nodes = new List<JsonTreeNode>();

                        JsonTreeNode dataObjectsNode = new JsonTreeNode
                        {
                            nodeType = "async",
                            type = "DataObjectsNode",
                            icon = "Content/img/folder.png",
                            id = context + "/DataObjects",
                            text = "Data Objects",
                            expanded = false,
                            leaf = false,
                            children = null
                        };

                        JsonTreeNode graphsNode = new JsonTreeNode
                        {
                            nodeType = "async",
                            type = "GraphsNode",
                            icon = "Content/img/folder.png",
                            id = context + "/Graphs",
                            text = "Graphs",
                            expanded = false,
                            leaf = false,
                            children = null
                        };

                        JsonTreeNode ValueListsNode = new JsonTreeNode
                        {
                            nodeType = "async",
                            type = "ValueListsNode",
                            icon = "Content/img/folder.png",
                            id = context + "/ValueLists",
                            text = "ValueLists",
                            expanded = false,
                            leaf = false,
                            children = null
                        };

                        nodes.Add(dataObjectsNode);
                        nodes.Add(graphsNode);
                        nodes.Add(ValueListsNode);

                        return Json(nodes, JsonRequestBehavior.AllowGet);
                    }
                case "ValueListsNode":
                    {
                        string context = form["node"];
                        string scopeName = context.Split('/')[0];
                        string applicationName = context.Split('/')[1];

                        Mapping mapping = GetMapping(scopeName, applicationName);

                        List<JsonTreeNode> nodes = new List<JsonTreeNode>();

                        foreach (ValueListMap valueList in mapping.valueListMaps)
                        {
                            JsonTreeNode node = new JsonTreeNode
                            {
                                nodeType = "async",
                                type = "ValueListNode",
                                icon = "Content/img/valuelist.png",
                                id = context + "/ValueList/" + valueList.name,
                                text = valueList.name,
                                expanded = true,
                                leaf = true,
                                children = new List<JsonTreeNode>()
                            };

                            nodes.Add(node);
                        }

                        return Json(nodes, JsonRequestBehavior.AllowGet);
                    }
                case "DataObjectsNode":
                    {
                        string context = form["node"];
                        string scopeName = context.Split('/')[0];
                        string applicationName = context.Split('/')[1];

                        DataDictionary dictionary = _repository.GetDictionary(scopeName, applicationName);

                        List<JsonTreeNode> nodes = new List<JsonTreeNode>();

                        foreach (DataObject dataObject in dictionary.dataObjects)
                        {
                            JsonTreeNode node = new JsonTreeNode
                            {
                                nodeType = "async",
                                type = "DataObjectNode",
                                icon = "Content/img/object.png",
                                id = context + "/DataObject/" + dataObject.objectName,
                                text = dataObject.objectName,
                                expanded = false,
                                leaf = false,
                                children = null,
                                record = new
                                {
                                    Name = dataObject.objectName
                                }
                            };

                            nodes.Add(node);
                        }

                        return Json(nodes, JsonRequestBehavior.AllowGet);

                    }
                case "DataObjectNode":
                    {
                        string context = form["node"];
                        string scopeName = context.Split('/')[0];
                        string applicationName = context.Split('/')[1];
                        string dataObjectName = context.Split('/')[4];

                        DataDictionary dictionary = _repository.GetDictionary(scopeName, applicationName);
                        DataObject dataObject = dictionary.dataObjects.FirstOrDefault(o => o.objectName == dataObjectName);

                        List<JsonTreeNode> nodes = new List<JsonTreeNode>();

                        foreach (DataProperty property in dataObject.dataProperties)
                        {
                            JsonTreeNode node = new JsonTreeNode
                            {
                                nodeType = "async",
                                type = (dataObject.isKeyProperty(property.propertyName)) ? "KeyDataPropertyNode" : "DataPropertyNode",
                                icon = (dataObject.isKeyProperty(property.propertyName)) ? "Content/img/key.png" : "Content/img/property.png",
                                id = context + "/" + property.propertyName,
                                text = property.propertyName,
                                expanded = true,
                                leaf = true,
                                children = new List<JsonTreeNode>(),
                                record = new
                                {
                                    Name = property.propertyName,
                                    Keytype = getKeytype(property.propertyName, dataObject.dataProperties),
                                    Datatype= getDatatype(property.propertyName, dataObject.dataProperties)
                                }
                            };

                            nodes.Add(node);
                        }

                        return Json(nodes, JsonRequestBehavior.AllowGet);

                    }
                case "GraphsNode":
                    {

                        string context = form["node"];
                        string scopeName = context.Split('/')[0];
                        string applicationName = context.Split('/')[1];

                        Mapping mapping = GetMapping(scopeName, applicationName);

                        List<JsonTreeNode> nodes = new List<JsonTreeNode>();

                        foreach (GraphMap graph in mapping.graphMaps)
                        {
                            JsonTreeNode node = new JsonTreeNode
                            {
                                nodeType = "async",
                                type = "GraphNode",
                                icon = "Content/img/graph-map.png",
                                id = context + "/Graph/" + graph.name,
                                text = graph.name,
                                expanded = true,
                                leaf = true,
                                children = new List<JsonTreeNode>()
                            };

                            nodes.Add(node);
                        }

                        return Json(nodes, JsonRequestBehavior.AllowGet);

                    }
                default:
                    {
                        return Json(new { success = false }, JsonRequestBehavior.AllowGet);
                    }
            }

        }

        public ActionResult DataLayers()
        {
            DataLayers dataLayers = _repository.GetDataLayers();

            JsonContainer<DataLayers> container = new JsonContainer<DataLayers>();
            container.items = dataLayers;
            container.success = true;
            container.total = dataLayers.Count;

            return Json(container, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Scope(FormCollection form)
        {
            string success = _repository.UpdateScope(form["scope"], form["name"], form["description"]);

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Application(FormCollection form)
        {
            #region Works in case of New Application
            string name = string.Empty;
            if (form["Application"] == string.Empty)
            {
                name = form["Name"];
            }
            else
            {
                name = form["Application"];
            }
            #endregion

            string success = _repository.UpdateApplication(form["Scope"], name, form["Name"], form["Description"], form["assembly"]);

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteScope(FormCollection form)
        {
            _repository.DeleteScope(form["nodeid"]);

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteApplication(FormCollection form)
        {
            string context = form["nodeid"];
            string scope = context.Split('/')[0];
            string application = context.Split('/')[1];

            _repository.DeleteApplication(scope, application);

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        #region Private Methods

        private Mapping GetMapping(string scope, string application)
        {
          string key = string.Format(_keyFormat, scope, application);

          if (Session[key] == null)
          {
            Session[key] = _repository.GetMapping(scope, application);
          }

          return (Mapping)Session[key];
        }

        private string getKeytype(string name, List<DataProperty> properties)
        {
            string keyType = string.Empty;
            keyType = properties.FirstOrDefault(p => p.columnName == name).keyType.ToString();

            return keyType;
        }
        private string getDatatype(string name, List<DataProperty> properties)
        {
            string dataType = string.Empty;
            dataType = properties.FirstOrDefault(p => p.columnName == name).dataType.ToString();

            return dataType;
        }
        #endregion
    }

}

