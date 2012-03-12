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
    private const string USERID_KEY = "emailaddress";   

    public DirectoryController()
      : this(new AdapterRepository())
    {
      string user = GetUserId((IDictionary<string, string>)_allClaims);
      Session[user + "." + "directory"] = null;
      Session[user + "." + "resource"] = null;
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
        string securityRole = form["security"];

        switch (form["type"])
        {
          case "ScopesNode":
            {
              string user = GetUserId((IDictionary<string, string>)_allClaims);             
              Tree directoryTree = _repository.GetDirectoryTree(user); 
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
                iconCls = "folder",
                id = context + "/DataObjects",
                text = "Data Objects",
                //expanded = false,
                leaf = false,
                children = null,
                record = new
                {
                  securityRole = securityRole
                }
              };
              dataObjectsNode.property = new Dictionary<string, string>();
              AddContextEndpointtoNode(dataObjectsNode, form);

              TreeNode graphsNode = new TreeNode
              {
                nodeType = "async",
                type = "GraphsNode",
                iconCls = "folder",
                id = context + "/Graphs",
                text = "Graphs",
                //expanded = false,
                leaf = false,
                children = null,
                record = new
                {
                  securityRole = securityRole
                }
              };
              graphsNode.property = new Dictionary<string, string>();
              AddContextEndpointtoNode(graphsNode, form);

              TreeNode ValueListsNode = new TreeNode
              {
                nodeType = "async",
                type = "ValueListsNode",
                iconCls = "folder",
                id = context + "/ValueLists",
                text = "ValueLists",
                //expanded = false,
                leaf = false,
                children = null,
                record = new
                {
                  securityRole = securityRole
                }
              };
              ValueListsNode.property = new Dictionary<string, string>();
              AddContextEndpointtoNode(ValueListsNode, form);
              nodes.Add(dataObjectsNode);
              nodes.Add(graphsNode);
              nodes.Add(ValueListsNode);

              return Json(nodes, JsonRequestBehavior.AllowGet);
            }
          case "ValueListsNode":
            {
              string context = form["node"];
              string contextName = form["contextName"];
              string endpoint = form["endpoint"];

              Mapping mapping = GetMapping(contextName, endpoint);

              List<JsonTreeNode> nodes = new List<JsonTreeNode>();

              foreach (ValueListMap valueList in mapping.valueListMaps)
              {
                TreeNode node = new TreeNode
                {
                  nodeType = "async",
                  type = "ValueListNode",
                  iconCls = "valuelistmap",
                  id = context + "/ValueList/" + valueList.name,
                  text = valueList.name,
                  //expanded = false,
                  leaf = false,
                  record = new
                  {
                    securityRole = securityRole,
                    record = valueList
                  }
                };

                node.property = new Dictionary<string, string>();
                node.property.Add("Name", valueList.name);
                AddContextEndpointtoNode(node, form);
                nodes.Add(node);
              }

              return Json(nodes, JsonRequestBehavior.AllowGet);
            }
          case "ValueListNode":
            {
              string context = form["node"];
              string valueList = form["text"];
              string contextName = form["contextName"];
              string endpoint = form["endpoint"];

              List<JsonTreeNode> nodes = new List<JsonTreeNode>();
              Mapping mapping = GetMapping(contextName, endpoint);
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
                  iconCls = "valuemap",
                  id = context + "/ValueMap/" + valueMap.internalValue,
                  text = classLabel + " [" + valueMap.internalValue + "]",
                  //expanded = false,
                  leaf = true,
                  record = new
                  {
                    securityRole = securityRole,
                    record = valueMap
                  }

                };

                node.property = new Dictionary<string, string>();
                node.property.Add("Name", valueMap.internalValue);
                node.property.Add("Class Label", classLabel);
                AddContextEndpointtoNode(node, form);
                nodes.Add(node);
              }

              return Json(nodes, JsonRequestBehavior.AllowGet);
            }

          case "DataObjectsNode":
            {
              string contextName = form["contextName"];
              string endpoint = form["endpoint"];
              DataDictionary dictionary = _repository.GetDictionary(contextName, endpoint);
              List<JsonTreeNode> nodes = new List<JsonTreeNode>();

              foreach (DataObject dataObject in dictionary.dataObjects)
              {
                JsonTreeNode node = new JsonTreeNode
                {
                  nodeType = "async",
                  type = "DataObjectNode",
                  iconCls = "treeObject",
                  id = form["node"] + "/DataObject/" + dataObject.objectName,
                  text = dataObject.objectName,
                  //expanded = false,
                  leaf = false,

                  record = new
                  {
                    Name = dataObject.objectName,
                    securityRole = securityRole
                  }
                };
                node.property = new Dictionary<string, string>();
                node.property.Add("Name", dataObject.objectName);
                AddContextEndpointtoNode(node, form);
                nodes.Add(node);
              }
              return Json(nodes, JsonRequestBehavior.AllowGet);

            }
          case "DataObjectNode":
            {
              string datatype, keytype;
              string context = form["node"];
              string dataObjectName = form["text"];
              string contextName = form["contextName"];
              string endpoint = form["endpoint"];

              DataDictionary dictionary = _repository.GetDictionary(contextName, endpoint);
              DataObject dataObject = dictionary.dataObjects.FirstOrDefault(o => o.objectName == dataObjectName);

              List<JsonTreeNode> nodes = new List<JsonTreeNode>();

              foreach (DataProperty properties in dataObject.dataProperties)
              {
                keytype = GetKeytype(properties.propertyName, dataObject.dataProperties);
                datatype = GetDatatype(properties.propertyName, dataObject.dataProperties);
                TreeNode node = new TreeNode
                {
                  //nodeType = "async",
                  type = (dataObject.isKeyProperty(properties.propertyName)) ? "KeyDataPropertyNode" : "DataPropertyNode",
                  iconCls = (dataObject.isKeyProperty(properties.propertyName)) ? _repository.GetNodeIconCls("key") : _repository.GetNodeIconCls("property"),
                  id = context + "/" + properties.propertyName,
                  text = properties.propertyName,
                  expanded = true,
                  leaf = false,
                  children = new List<JsonTreeNode>(),
                  record = new
                  {
                    Name = properties.propertyName,
                    Keytype = keytype,
                    Datatype = datatype,
                    securityRole = securityRole
                  }
                };
                node.property = new Dictionary<string, string>();
                node.property.Add("Name", properties.propertyName);
                node.property.Add("Keytype", keytype);
                node.property.Add("Datatype", datatype);
                AddContextEndpointtoNode(node, form);
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
                    iconCls = "treeRelation",
                    id = context + "/" + dataObject.objectName + "/" + relation.relationshipName,
                    text = relation.relationshipName,
                    //expanded = false,
                    leaf = false,

                    record = new
                    {
                      Name = relation.relationshipName,
                      Type = relation.relationshipType,
                      Related = relation.relatedObjectName,
                      securityRole = securityRole
                    }
                  };
                  node.property = new Dictionary<string, string>();
                  node.property.Add("Name", relation.relationshipName);
                  node.property.Add("Type", relation.relationshipType.ToString());
                  node.property.Add("Related", relation.relatedObjectName);
                  AddContextEndpointtoNode(node, form);
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
              string contextName = form["contextName"];
              string endpoint = form["endpoint"];

              List<JsonTreeNode> nodes = new List<JsonTreeNode>();
              DataDictionary dictionary = _repository.GetDictionary(contextName, endpoint);
              DataObject dataObject = dictionary.dataObjects.FirstOrDefault(o => o.objectName.ToUpper() == related.ToUpper());
              foreach (DataProperty properties in dataObject.dataProperties)
              {
                keytype = GetKeytype(properties.propertyName, dataObject.dataProperties);
                datatype = GetDatatype(properties.propertyName, dataObject.dataProperties);
                TreeNode node = new TreeNode
                {
                  //nodeType = "async",
                  type = (dataObject.isKeyProperty(properties.propertyName)) ? "KeyDataPropertyNode" : "DataPropertyNode",
                  iconCls = (dataObject.isKeyProperty(properties.propertyName)) ? _repository.GetNodeIconCls("key") : _repository.GetNodeIconCls("property"),
                  id = context + "/" + properties.propertyName,
                  text = properties.propertyName,
                  expanded = true,
                  leaf = false,
                  children = new List<JsonTreeNode>(),
                  record = new
                  {
                    Name = properties.propertyName,
                    Keytype = keytype,
                    Datatype = datatype,
                    securityRole = securityRole
                  }
                };
                node.property = new Dictionary<string, string>();
                node.property.Add("Name", properties.propertyName);
                node.property.Add("Type", keytype);
                node.property.Add("Related", related);
                AddContextEndpointtoNode(node, form);
                nodes.Add(node);
              }
              return Json(nodes, JsonRequestBehavior.AllowGet);
            }

          case "GraphsNode":
            {
              string context = form["node"];
              string contextName = form["contextName"];
              string endpoint = form["endpoint"];
              Mapping mapping = GetMapping(contextName, endpoint);
              List<JsonTreeNode> nodes = new List<JsonTreeNode>();

              foreach (GraphMap graph in mapping.graphMaps)
              {
                TreeNode node = new TreeNode
                {
                  //nodeType = "async",
                  type = "GraphNode",
                  iconCls = "graphmap",
                  id = context + "/Graph/" + graph.name,
                  text = graph.name,
                  expanded = true,
                  leaf = false,
                  children = new List<JsonTreeNode>(),
                  record = new
                  {
                    securityRole = securityRole,
                    record = graph
                  }
                };
                node.property = new Dictionary<string, string>();
                node.property.Add("Data Object Name", graph.dataObjectName);
                node.property.Add("Name", graph.name);
                node.property.Add("Identifier", graph.classTemplateMaps[0].classMap.identifiers[0].Split('.')[1]);
                node.property.Add("Class Label", graph.classTemplateMaps[0].classMap.name);
                AddContextEndpointtoNode(node, form);
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

    private void AddContextEndpointtoNode(JsonTreeNode node, FormCollection form)
    {
      if (form["contextName"] != null)
        node.property.Add("context", form["contextName"]);
      if (form["endpoint"] != null)
        node.property.Add("endpoint", form["endpoint"]);
    }

    //public ActionResult DataLayers(string contextName, string endpoint)
    //{
    //  DataLayers dataLayers = _repository.GetDataLayers(contextName, endpoint);
    //  JsonContainer<DataLayers> container = new JsonContainer<DataLayers>();
    //  container.items = dataLayers;
    //  container.success = true;
    //  container.total = dataLayers.Count;
    //  return Json(container, JsonRequestBehavior.AllowGet);
    //}

    public ActionResult DataLayers()
    {
        DataLayers dataLayers = _repository.GetDataLayers();
        JsonContainer<DataLayers> container = new JsonContainer<DataLayers>();
        container.items = dataLayers;
        container.success = true;
        container.total = dataLayers.Count;
        return Json(container, JsonRequestBehavior.AllowGet);
    }


    public JsonResult Folder(FormCollection form)
    {
      string success;
      string user = GetUserId((IDictionary<string, string>)_allClaims);
      success = _repository.Folder(form["foldername"], form["description"], form["path"], form["state"], form["contextName"], form["oldContext"], user);
      
      if (success == "ERROR")
      {
        string msg = _repository.GetCombinationMsg();
        return Json(new { success = false } + msg, JsonRequestBehavior.AllowGet);
      }

      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

    public JsonResult Endpoint(FormCollection form)
    {
      string success;
      string user = GetUserId((IDictionary<string, string>)_allClaims);      

      success = _repository.Endpoint(form["endpoint"], form["path"], form["description"], form["state"], form["contextValue"], form["oldAssembly"], form["assembly"], form["baseUrl"], form["oldBaseUrl"], user);

      if (success == "ERROR")
      {
        string msg = _repository.GetCombinationMsg();
        return Json(new { success = false } + msg, JsonRequestBehavior.AllowGet);
      }

      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

    public JsonResult DeleteEntry(FormCollection form)
    {
      string user = GetUserId((IDictionary<string, string>)_allClaims);
      _repository.DeleteEntry(form["path"], form["type"], form["contextName"], form["baseUrl"], user);
      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

    public JsonResult RootSecurityRole()
    {
      string rootSecuirtyRole = _repository.GetRootSecurityRole();
      return Json(rootSecuirtyRole, JsonRequestBehavior.AllowGet);
    }

    public JsonResult UseLdap()
    {
      string ifUseLdap = _repository.GetUserLdap();
      return Json(ifUseLdap, JsonRequestBehavior.AllowGet);
    }

    public JsonResult EndpointBaseUrl()
    {
      BaseUrls baseUrls = _repository.GetEndpointBaseUrl();
      JsonContainer<BaseUrls> container = new JsonContainer<BaseUrls>();
      container.items = baseUrls;
      container.success = true;
      container.total = baseUrls.Count;
      return Json(container, JsonRequestBehavior.AllowGet);      
    }

    #region Private Methods

    private string GetUserId(IDictionary<string, string> claims)
    {
      foreach (var pair in claims)
      {
        if (pair.Key.ToLower() == USERID_KEY)
        {
          return pair.Value;
        }
      }

      return "guest";
    }
   
    private Mapping GetMapping(string contextName, string endpoint)
    {
      string key = string.Format(_keyFormat, contextName, endpoint);

      if (Session[key] == null)
      {
        Session[key] = _repository.GetMapping(contextName, endpoint);
      }

      return (Mapping)Session[key];
    }

    private string GetClassLabel(string classId)
    {
      Entity dataEntity = _repository.GetClassLabel(classId);
      return Convert.ToString(dataEntity.Label);
    }

    private string GetKeytype(string name, List<DataProperty> properties)
    {
      string keyType = string.Empty;
      keyType = properties.FirstOrDefault(p => p.propertyName == name).keyType.ToString();
      return keyType;
    }
    private string GetDatatype(string name, List<DataProperty> properties)
    {
      string dataType = string.Empty;
      dataType = properties.FirstOrDefault(p => p.propertyName == name).dataType.ToString();
      return dataType;
    }
    #endregion
  }

}

