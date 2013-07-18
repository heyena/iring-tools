using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using iRINGTools.Web.Helpers;
using iRINGTools.Web.Models;

using org.iringtools.library;
using org.iringtools.mapping;
using log4net;
using System.Web.Script.Serialization;


namespace org.iringtools.web.controllers
{
  public class DirectoryController : BaseController
  {

    IAdapterRepository _repository;
    private string _keyFormat = "Mapping.{0}.{1}";
    private static readonly ILog _logger = LogManager.GetLogger(typeof(DirectoryController));
    private System.Web.Script.Serialization.JavaScriptSerializer oSerializer;
    

    public DirectoryController()
      : this(new AdapterRepository())
    {
    }

    public DirectoryController(IAdapterRepository repository)
    {
      _repository = repository;
      oSerializer =
         new System.Web.Script.Serialization.JavaScriptSerializer();
    }

    public ActionResult Index()
    {
      return View(_repository.GetScopes());
    }

    public ActionResult GetNode(FormCollection form)
    {
      try
      {
        _logger.Debug("Starting Switch block");
        _logger.Debug(form["type"]);

        switch (form["type"])
        {
          case "ScopesNode":
          {
            Tree directoryTree = _repository.GetDirectoryTree();
            return Json(directoryTree.getNodes(), JsonRequestBehavior.AllowGet);
          }          
          case "ApplicationNode":
          {
            string context = form["node"];

            List<JsonTreeNode> nodes = new List<JsonTreeNode>();

            TreeNode dataObjectsNode = new TreeNode
            {
              nodeType = "async",
              type = "DataObjectsNode",
              icon = "Content/img/folder.png",
              id = context + "/DataObjects",
              text = "Data Objects",
              expanded = false,
              leaf = true,
              children = null
            };
            dataObjectsNode.property = new Dictionary<string, string>();
            dataObjectsNode.property.Add("BaseURI", form["baseURI"]);

            TreeNode graphsNode = new TreeNode
            {
              nodeType = "async",
              type = "GraphsNode",
              icon = "Content/img/folder.png",
              id = context + "/Graphs",
              text = "Graphs",
              expanded = false,
              leaf = true,
              children = null
            };
            graphsNode.property = new Dictionary<string, string>();
            graphsNode.property.Add("BaseURI", form["baseURI"]);

            TreeNode ValueListsNode = new TreeNode
            {
              nodeType = "async",
              type = "ValueListsNode",
              icon = "Content/img/folder.png",
              id = context + "/ValueLists",
              text = "ValueLists",
              expanded = false,
              leaf = true,
              children = null
            };
            ValueListsNode.property = new Dictionary<string, string>();
            ValueListsNode.property.Add("BaseURI", form["baseURI"]);

            nodes.Add(dataObjectsNode);
            nodes.Add(graphsNode);
            nodes.Add(ValueListsNode);

            return Json(nodes, JsonRequestBehavior.AllowGet);
          }
          case "ValueListsNode":
            {
              string context = form["node"];
              string baseUri = form["baseUri"];
              _repository.getAppScopeName(baseUri);

              Mapping mapping = GetMapping();

              List<JsonTreeNode> nodes = new List<JsonTreeNode>();

              foreach (ValueListMap valueList in mapping.valueListMaps)
              {
                TreeNode node = new TreeNode
                {
                  nodeType = "async",
                  type = "ValueListNode",
                  icon = "Content/img/valuelist.png",
                  id = context + "/ValueList/" + valueList.name,
                  text = valueList.name,
                  expanded = false,
                  leaf = true,                  
                  record = valueList
                };
                node.property = new Dictionary<string, string>();
                node.property.Add("Name", valueList.name);
                node.property.Add("BaseURI", form["baseURI"]);
                nodes.Add(node);
              }

              return Json(nodes, JsonRequestBehavior.AllowGet);
            }
          case "ValueListNode":
            {
              string context = form["node"];
              string baseUri = form["baseUri"];
              _repository.getAppScopeName(baseUri);
              string valueList = form["text"];

              List<JsonTreeNode> nodes = new List<JsonTreeNode>();
              Mapping mapping = GetMapping();
              ValueListMap valueListMap = mapping.valueListMaps.Find(c => c.name == valueList);

              foreach (var valueMap in valueListMap.valueMaps)
              {
                string valueMapUri = valueMap.uri.Split(':')[1];
                string classLabel = String.Empty;

                if (!String.IsNullOrEmpty(valueMap.label))
                {
                  classLabel = valueMap.label;
                }
                else if (Session[valueMapUri] != null)
                {
                  classLabel = (string)Session[valueMapUri];
                }
                else
                {
                  classLabel = GetClassLabel(valueMapUri);
                  Session[valueMapUri] = classLabel;
                }

                JsonTreeNode node = new JsonTreeNode
                {
                  //nodeType = "async",
                  type = "ListMapNode",
                  icon = "Content/img/value.png",
                  id = context + "/ValueMap/" + valueMap.internalValue,
                  text = classLabel + " [" + valueMap.internalValue + "]",
                  expanded = false,
                  leaf = true,                  
                  record = valueMap
                };

                node.property = new Dictionary<string, string>();
                node.property.Add("Name", valueMap.internalValue);
                node.property.Add("Class Label", classLabel);
                node.property.Add("BaseURI", form["baseURI"]);
                nodes.Add(node);
              }

              return Json(nodes, JsonRequestBehavior.AllowGet);
            }

          case "DataObjectsNode":
            {
              string baseUri = form["baseUri"];
              _repository.getAppScopeName(baseUri);
              DataDictionary dictionary = _repository.GetDictionary();

              List<JsonTreeNode> nodes = new List<JsonTreeNode>();

              foreach (DataObject dataObject in dictionary.dataObjects)
              {
                JsonTreeNode node = new JsonTreeNode
                {
                  nodeType = "async",
                  type = "DataObjectNode",
                  icon = "Content/img/object.png",
                  id = form["node"] + "/DataObject/" + dataObject.objectName,
                  text = dataObject.objectName,
                  expanded = false,
                  leaf = true,
                  
                  record = new
                  {
                    Name = dataObject.objectName
                  }
                };
                node.property = new Dictionary<string, string>();
                node.property.Add("Name", dataObject.objectName);
                node.property.Add("BaseURI", form["baseURI"]);
                nodes.Add(node);

              }
              return Json(nodes, JsonRequestBehavior.AllowGet);

            }
          case "DataObjectNode":
            {
              string datatype, keytype;
              string context = form["node"];
              string baseUri = form["baseUri"];
              _repository.getAppScopeName(baseUri);
              string dataObjectName = form["text"];

              DataDictionary dictionary = _repository.GetDictionary();
              DataObject dataObject = dictionary.dataObjects.FirstOrDefault(o => o.objectName == dataObjectName);

              List<JsonTreeNode> nodes = new List<JsonTreeNode>();

              foreach (DataProperty properties in dataObject.dataProperties)
              {
                keytype = getKeytype(properties.propertyName, dataObject.dataProperties);
                datatype = getDatatype(properties.propertyName, dataObject.dataProperties);
                TreeNode node = new TreeNode
                {
                  //nodeType = "async",
                  type = (dataObject.isKeyProperty(properties.propertyName)) ? "KeyDataPropertyNode" : "DataPropertyNode",
                  iconCls = (dataObject.isKeyProperty(properties.propertyName)) ? _repository.getNodeIconCls("key") : _repository.getNodeIconCls("property"),
                  id = context + "/" + properties.propertyName,
                  text = properties.propertyName,
                  expanded = true,
                  leaf = true,
                  children = new List<JsonTreeNode>(),
                  record = new
                  {
                    Name = properties.propertyName,
                    Keytype = keytype,
                    Datatype = datatype
                  }
                };
                node.property = new Dictionary<string, string>();
                node.property.Add("Name", properties.propertyName);
                node.property.Add("Keytype", keytype);
                node.property.Add("Datatype", datatype);
                node.property.Add("BaseURI", form["baseURI"]);
                nodes.Add(node);
              }
              if (dataObject.dataRelationships.Count > 0)
              {
                foreach (DataRelationship relation in dataObject.dataRelationships)
                {
                  TreeNode node = new TreeNode
                  {
                    nodeType = "async",
                    type = "RelationshipNode",
                    icon = "Content/img/relation.png",
                    id = context + "/" + dataObject.objectName + "/" + relation.relationshipName,
                    text = relation.relationshipName,
                    expanded = false,
                    leaf = true,
                    
                    record = new
                    {
                      Name = relation.relationshipName,
                      Type = relation.relationshipType,
                      Related = relation.relatedObjectName
                    }
                  };
                  node.property = new Dictionary<string, string>();
                  node.property.Add("Name", relation.relationshipName);
                  node.property.Add("Type", relation.relationshipType.ToString());
                  node.property.Add("Related", relation.relatedObjectName);
                  node.property.Add("BaseURI", form["baseURI"]);
                  nodes.Add(node);
                }

              }
              return Json(nodes, JsonRequestBehavior.AllowGet);

            }

          case "RelationshipNode":
            {
              string keytype, datatype;
              string context = form["node"];
              string related = form["related"];
              string baseUri = form["baseUri"];
              _repository.getAppScopeName(baseUri);

              List<JsonTreeNode> nodes = new List<JsonTreeNode>();
              DataDictionary dictionary = _repository.GetDictionary();
              DataObject dataObject = dictionary.dataObjects.FirstOrDefault(o => o.objectName.ToUpper() == related.ToUpper());
              foreach (DataProperty properties in dataObject.dataProperties)
              {
                keytype = getKeytype(properties.propertyName, dataObject.dataProperties);
                datatype = getDatatype(properties.propertyName, dataObject.dataProperties);
                TreeNode node = new TreeNode
                {
                  //nodeType = "async",
                  type = (dataObject.isKeyProperty(properties.propertyName)) ? "KeyDataPropertyNode" : "DataPropertyNode",
                  iconCls = (dataObject.isKeyProperty(properties.propertyName)) ? _repository.getNodeIconCls("key") : _repository.getNodeIconCls("property"),
                  id = context + "/" + properties.propertyName,
                  text = properties.propertyName,
                  expanded = true,
                  leaf = true,
                  children = new List<JsonTreeNode>(),
                  record = new
                  {
                    Name = properties.propertyName,
                    Keytype = keytype,
                    Datatype = datatype
                  }
                };
                node.property = new Dictionary<string, string>();
                node.property.Add("Name", properties.propertyName);
                node.property.Add("Type", keytype);
                node.property.Add("Related", related);
                node.property.Add("BaseURI", form["baseURI"]);
                nodes.Add(node);
              }
              return Json(nodes, JsonRequestBehavior.AllowGet);
            }

          case "GraphsNode":
            {
              string context = form["node"];
              string baseUri = form["baseUri"];
              _repository.getAppScopeName(baseUri);

              Mapping mapping = GetMapping();

              List<JsonTreeNode> nodes = new List<JsonTreeNode>();

              foreach (GraphMap graph in mapping.graphMaps)
              {
                TreeNode node = new TreeNode
                {
                  //nodeType = "async",
                  type = "GraphNode",
                  icon = "Content/img/graph-map.png",
                  id = context + "/Graph/" + graph.name,
                  text = graph.name,
                  expanded = true,
                  leaf = true,
                  children = new List<JsonTreeNode>(),
                  record = graph

                };
                node.property = new Dictionary<string, string>();
                node.property.Add("Data Object Name", graph.dataObjectName);
                node.property.Add("Name", graph.name);
                node.property.Add("Identifier", graph.classTemplateMaps[0].classMap.identifiers[0].Split('.')[1]);
                node.property.Add("Class Label", graph.classTemplateMaps[0].classMap.name);
                node.property.Add("BaseURI", form["baseURI"]);
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
      catch (Exception e)
      {
        _logger.Error(e.ToString());
        throw e;
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
      string success;      
      success = _repository.Folder(form["name"], form["description"], form["path"], form["state"]);
      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

    public JsonResult Application(FormCollection form)
    {
      string success;     
      success = _repository.Endpoint(form["Scope"], form["name"], form["Description"], form["assembly"], form["path"], form["state"]);
      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

    public JsonResult DeleteFolder(FormCollection form)
    {
      _repository.DeleteFolder(form["path"]);
      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

    public JsonResult DeleteApplication(FormCollection form)
    {
      _repository.DeleteEndpoint(form["scope"], form["path"]);
      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

    #region Private Methods

    private Mapping GetMapping()
    {
      string key = string.Format(_keyFormat, _repository.getScopeName(), _repository.getAppName());

      if (Session[key] == null)
      {
        Session[key] = _repository.GetMapping();
      }

      return (Mapping)Session[key];
    }

    private string GetClassLabel(string classId)
    {
      Entity dataEntity = _repository.GetClassLabel(classId);

      return Convert.ToString(dataEntity.Label);
    }

    private string getKeytype(string name, List<DataProperty> properties)
    {
      string keyType = string.Empty;
      keyType = properties.FirstOrDefault(p => p.propertyName == name).keyType.ToString();

      return keyType;
    }
    private string getDatatype(string name, List<DataProperty> properties)
    {
      string dataType = string.Empty;
      dataType = properties.FirstOrDefault(p => p.propertyName == name).dataType.ToString();

      return dataType;
    }
    #endregion
  }

}

