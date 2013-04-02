using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using iRINGTools.Web.Helpers;
using iRINGTools.Web.Models;

using org.iringtools.library;
using org.iringtools.mapping;
using log4net;
using System.Web;
using System.IO;
using org.iringtools.utility;
using System.Runtime.Serialization;

namespace org.iringtools.web.controllers
{
  public class DirectoryController : BaseController
  {
    private AdapterRepository _repository;
    private string _keyFormat = "Mapping.{0}.{1}";
    private static readonly ILog _logger = LogManager.GetLogger(typeof(DirectoryController));

    public DirectoryController()
      : this(new AdapterRepository())
    {
    }

    public DirectoryController(AdapterRepository repository)
    {
      _repository = repository;
    }

    public ActionResult Index()
    {
      return View(_repository.GetScopes());
    }

    public ActionResult GetNode(FormCollection form)
    {
      try
      {
        _logger.Debug("GetNode type: " + form["type"]);
        _repository.Session = Session;

        switch (form["type"])
        {
          case "ScopesNode":
            {
              System.Collections.IEnumerator ie = Session.GetEnumerator();
              while (ie.MoveNext())
              {
                Session.Remove(ie.Current.ToString());
                ie = Session.GetEnumerator();
              }

              var nodes = new List<JsonTreeNode>();
              var contexts = _repository.GetScopes();

              if (contexts != null)
              {
                foreach (var scope in contexts)
                {
                  var node = new JsonTreeNode
                    {
                      nodeType = "async",
                      type = "ScopeNode",
                      iconCls = "scope",
                      id = scope.Name,
                      text = scope.Name,
                      expanded = false,
                      leaf = false,
                      children = null,
                      record = scope,
                      property =
                        new Dictionary<string, string> {{"Name", scope.Name}, {"Description", scope.Description}}
                    };

                  nodes.Add(node);
                }
              }

              return Json(nodes, JsonRequestBehavior.AllowGet);
            }
          case "ScopeNode":
            {
              var context = _repository.GetScope(form["node"]);

              var nodes = (from endpoint in context.Applications
                           let dataLayer = _repository.GetDataLayer(context.Name, endpoint.Name)
                           where dataLayer != null
                           select new JsonTreeNode
                             {
                               nodeType = "async", 
                               type = "ApplicationNode", 
                               iconCls = "applications", 
                               id = context.Name + "/" + endpoint.Name, 
                               text = endpoint.Name, 
                               expanded = false, 
                               leaf = false, 
                               children = null, 
                               record = new
                                 {
                                   ContextName = context.Name, 
                                   Endpoint = endpoint.Name, 
                                   Description = endpoint.Description, 
                                   DataLayer = dataLayer.Name, 
                                   Assembly = dataLayer.Assembly, 
                                   Configuration = endpoint.Configuration
                                 }, property = new Dictionary<string, string>
                                   {
                                     {"contextName", context.Name}, 
                                     {"endpoint", endpoint.Name}, 
                                     {"Description", endpoint.Description}, 
                                     {"Data Layer", dataLayer.Name}
                                   }
                             }).ToList();

              ActionResult result = Json(nodes, JsonRequestBehavior.AllowGet);
              return result;
            }
          case "ApplicationNode":
            {
              var context = form["node"];

              var nodes = new List<JsonTreeNode>();

              var dataObjectsNode = new JsonTreeNode
              {
                nodeType = "async",
                type = "DataObjectsNode",
                iconCls = "folder",
                id = context + "/DataObjects",
                text = "Data Objects",
                expanded = false,
                leaf = false,
                children = null
              };

              var graphsNode = new JsonTreeNode
              {
                nodeType = "async",
                type = "GraphsNode",
                iconCls = "folder",
                id = context + "/Graphs",
                text = "Graphs",
                expanded = false,
                leaf = false,
                children = null
              };

              var ValueListsNode = new JsonTreeNode
              {
                nodeType = "async",
                type = "ValueListsNode",
                iconCls = "folder",
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
              var context = form["node"];
              var scopeName = context.Split('/')[0];
              var applicationName = context.Split('/')[1];

              var mapping = GetMapping(scopeName, applicationName);

              var nodes = mapping.valueListMaps.Select(valueList => new JsonTreeNode
                {
                  nodeType = "async", 
                  type = "ValueListNode", 
                  iconCls = "treeValuelist", 
                  id = context + "/ValueList/" + valueList.name, 
                  text = valueList.name, 
                  expanded = false, 
                  leaf = false, 
                  children = null,
                  record = valueList, 
                  property = new Dictionary<string, string> {{"Name", valueList.name}}
                }).ToList();

              return Json(nodes, JsonRequestBehavior.AllowGet);
            }
          case "ValueListNode":
            {
              var context = form["node"];
              var scopeName = context.Split('/')[0];
              var applicationName = context.Split('/')[1];
              var valueList = context.Split('/')[4];

              var nodes = new List<JsonTreeNode>();
              var mapping = GetMapping(scopeName, applicationName);
              var valueListMap = mapping.valueListMaps.Find(c => c.name == valueList);

              foreach (var valueMap in valueListMap.valueMaps)
              {
                var classLabel = String.Empty;

                if (!String.IsNullOrEmpty(valueMap.uri))
                {
                  var valueMapUri = valueMap.uri.Split(':')[1];

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
                }

                var node = new JsonTreeNode
                  {
                    nodeType = "async",
                    type = "ListMapNode",
                    iconCls = "treeValue",
                    id = context + "/ValueMap/" + valueMap.internalValue,
                    text = classLabel + " [" + valueMap.internalValue + "]",
                    expanded = false,
                    leaf = true,
                    children = null,
                    record = valueMap,
                    property =
                      new Dictionary<string, string> {{"Name", valueMap.internalValue}, {"Class Label", classLabel}}
                  };

                nodes.Add(node);
              }

              return Json(nodes, JsonRequestBehavior.AllowGet);
            }

          case "DataObjectsNode":
            {
              var context = form["node"];
              var contextName = context.Split('/')[0];
              var endpoint = context.Split('/')[1];
              var dataLayer = form["datalayer"];
              var refresh = form["refresh"];

              if (refresh == "true")
              {
                var response = _repository.Refresh(contextName, endpoint);
                _logger.Info(Utility.Serialize<Response>(response, true));
              }

              var nodes = new List<JsonTreeNode>();
              var dictionary = _repository.GetDictionary(contextName, endpoint);

              if (dictionary != null && dictionary.dataObjects != null)
              {
                foreach (var dataObject in dictionary.dataObjects)
                {
                  var node = new JsonTreeNode
                  {
                    nodeType = "async",
                    type = "DataObjectNode",
                    iconCls = "treeObject",
                    id = context + "/DataObject/" + dataObject.objectName,
                    text = dataObject.objectName,
                    expanded = false,
                    leaf = false,
                    children = null,
                    hidden = dataObject.isHidden,
                    record = new
                    {
                      Name = dataObject.objectName,
                      DataLayer = dataLayer
                    }
                  };

                  if (dataObject.isRelatedOnly)
                  {
                    node.hidden = true;
                  }

                  node.property = new Dictionary<string, string>
                    {
                      {"Name", dataObject.objectName},
                      {"ContextName", contextName},
                      {"Endpoint", endpoint},
                      {"BaseUrl", null}
                    };
                  nodes.Add(node);

                }
              }

              return Json(nodes, JsonRequestBehavior.AllowGet);
            }
          case "DataObjectNode":
            {
              string datatype;
              string keytype;
              var context = form["node"];
              var contextName = context.Split('/')[0];
              var endpoint = context.Split('/')[1];
              var dataObjectName = context.Split('/')[4];

              var dictionary = _repository.GetDictionary(contextName, endpoint);
              var dataObject = dictionary.dataObjects.FirstOrDefault(o => o.objectName == dataObjectName);

              var nodes = new List<JsonTreeNode>();

              foreach (var property in dataObject.dataProperties)
              {
                keytype = GetKeytype(property.propertyName, dataObject.dataProperties);
                datatype = GetDatatype(property.propertyName, dataObject.dataProperties);

                var node = new JsonTreeNode
                  {
                    nodeType = "async",
                    type =
                      (dataObject.isKeyProperty(property.propertyName)) ? "KeyDataPropertyNode" : "DataPropertyNode",
                    iconCls = (dataObject.isKeyProperty(property.propertyName)) ? "treeKey" : "treeProperty",
                    id = context + "/" + dataObject.objectName + "/" + property.propertyName,
                    text = property.propertyName,
                    expanded = true,
                    leaf = true,
                    children = new List<JsonTreeNode>(),
                    record = new
                      {
                        Name = property.propertyName,
                        Keytype = keytype,
                        Datatype = datatype
                      },
                    property =
                      new Dictionary<string, string> {
                        {"Name", property.propertyName}, 
                        {"Keytype", keytype}, 
                        {"Datatype", datatype}
                      }
                  };
                nodes.Add(node);
              }

              if (dataObject.dataRelationships.Count > 0)
              {
                nodes.AddRange(dataObject.dataRelationships.Select(relation => new JsonTreeNode
                  {
                    nodeType = "async", 
                    type = "RelationshipNode", 
                    iconCls = "treeRelation", 
                    id = context + "/" + dataObject.objectName + "/" + relation.relationshipName, 
                    text = relation.relationshipName,
                    expanded = false, 
                    leaf = false, 
                    children = null, 
                    record = new
                      {
                        Name = relation.relationshipName, Type = relation.relationshipType, Related = relation.relatedObjectName
                      }, property = new Dictionary<string, string> {
                          {"Name", relation.relationshipName}, 
                          {"Type", relation.relationshipType.ToString()}, 
                          {"Related", relation.relatedObjectName}
                        }
                  }));
              }

              return Json(nodes, JsonRequestBehavior.AllowGet);
            }

          case "RelationshipNode":
            {
              var context = form["node"];
              var related = form["related"];
              var nodes = new List<JsonTreeNode>();

              if (!String.IsNullOrEmpty(related))
              {
                var contextName = context.Split('/')[0];
                var endpoint = context.Split('/')[1];
                var dictionary = _repository.GetDictionary(contextName, endpoint);
                var dataObject = dictionary.dataObjects.FirstOrDefault(o => o.objectName.ToUpper() == related.ToUpper());

                nodes.AddRange(from property in dataObject.dataProperties
                               let keytype = GetKeytype(property.propertyName, dataObject.dataProperties)
                               let datatype = GetDatatype(property.propertyName, dataObject.dataProperties)
                               select new JsonTreeNode
                                 {
                                   nodeType = "async", 
                                   type = (dataObject.isKeyProperty(property.propertyName)) ? "KeyDataPropertyNode" : "DataPropertyNode", 
                                   iconCls = (dataObject.isKeyProperty(property.propertyName)) ? "treeKey" : "treeProperty", 
                                   id = context + "/" + property.propertyName, text = property.propertyName, 
                                   expanded = true, 
                                   leaf = true, 
                                   children = new List<JsonTreeNode>(), record = new 
                                   {
                                       Name = property.propertyName, 
                                       Keytype = keytype,
                                       Datatype = datatype
                                   }, property = new Dictionary<string, string>
                                       {
                                         {"Name", property.propertyName},
                                         {"Type", keytype}, 
                                         {"Related", datatype}
                                       }
                                 });
              }

              return Json(nodes, JsonRequestBehavior.AllowGet);
            }

          case "GraphsNode":
            {

              var context = form["node"];
              var contextName = context.Split('/')[0];
              var endpoint = context.Split('/')[1];

              var mapping = GetMapping(contextName, endpoint);

              var nodes = new List<JsonTreeNode>();

              foreach (var graph in mapping.graphMaps)
              {
                var node = new JsonTreeNode
                {
                  nodeType = "async",
                  type = "GraphNode",
                  iconCls = "treeGraph",
                  id = context + "/Graph/" + graph.name,
                  text = graph.name,
                  expanded = true,
                  leaf = true,
                  children = new List<JsonTreeNode>(),
                  record = graph
                };

                var classMap = graph.classTemplateMaps[0].classMap;

                node.property = new Dictionary<string, string>
                  {
                    {"context", contextName},
                    {"endpoint", endpoint},
                    {"Data Object Name", graph.dataObjectName},
                    {"Name", graph.name},
                    {"Identifier", string.Join(",", classMap.identifiers)},
                    {"Delimiter", classMap.identifierDelimiter},
                    {"Class Label", classMap.name}
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
      catch (Exception e)
      {
        _logger.Error(e.ToString());
        throw e;
      }
    }

    public ActionResult DataLayers()
    {
      var dataLayers = _repository.GetDataLayers();

      var container = new JsonContainer<DataLayers>
        {
          items = dataLayers,
          success = true, 
          total = dataLayers.Count
        };

      return Json(container, JsonRequestBehavior.AllowGet);
    }

    public string DataLayer(JsonTreeNode node, FormCollection form)
    {
      var files = Request.Files;
      var hpf = files[0] as HttpPostedFileBase;

      var dataLayerName = string.Empty;

      if (string.IsNullOrEmpty(form["Name"]))
      {
        var lastDot = hpf.FileName.LastIndexOf(".", System.StringComparison.Ordinal);
        dataLayerName = hpf.FileName.Substring(0, lastDot);
      }
      else
      {
        dataLayerName = form["Name"];
      }

      var dataLayer = new DataLayer()
      {
        Name = dataLayerName,
        Package = Utility.ToMemoryStream(hpf.InputStream)
      };

      var dataLayerStream = new MemoryStream();
      var serializer = new DataContractSerializer(typeof(DataLayer));
      serializer.WriteObject(dataLayerStream, dataLayer);
      dataLayerStream.Position = 0;

      var response = _repository.UpdateDataLayer(dataLayerStream);
      return Utility.ToJson<Response>(response);
    }

    public JsonResult Scope(FormCollection form)
    {
      var success = String.Empty;

      success = String.IsNullOrEmpty(form["contextName"]) ? _repository.AddScope(form["endpoint"], form["description"]) : _repository.UpdateScope(form["contextName"], form["endpoint"], form["description"]);

      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

    public JsonResult Application(FormCollection form)
    {
      var success = String.Empty;
      var context = form["contextValue"];
      library.Configuration configuration = new Configuration
      {
        AppSettings = new AppSettings
        {
          Settings = new List<Setting>()
        }
      };

      for (var i = 0; i < form.AllKeys.Length; i++)
      {
        if (form.GetKey(i).ToLower() != "contextValue" &&
          form.GetKey(i).ToLower() != "endpoint" &&
          form.GetKey(i).ToLower() != "description"
          && form.GetKey(i).ToLower() != "assembly" &&
          form.GetKey(i).ToLower() != "application" &&
          form.GetKey(i).ToLower().Substring(0, 3) != "val")
        {

          var key = form[i];
          if (i + 1 < form.AllKeys.Length)
          {
            var value = form[i + 1];
            configuration.AppSettings.Settings.Add(new Setting()
            {
              Key = key,
              Value = value
            });
          }
        }
      }

      var application = new ScopeApplication()
      {
        Name = form["endpoint"],
        Description = form["Description"],
        Assembly = form["assembly"],
        Configuration = configuration
      };

      success = String.IsNullOrEmpty(form["state"]) 
        ? _repository.AddApplication(context, application) 
        : _repository.UpdateApplication(context, form["endpoint"], application);

      var result = Json(new { success = true }, JsonRequestBehavior.AllowGet);
      return result;
    }

    public JsonResult DeleteScope(FormCollection form)
    {
      _repository.DeleteScope(form["nodeid"]);

      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

    public JsonResult DeleteApplication(FormCollection form)
    {
      var context = form["nodeid"];
      var scope = context.Split('/')[0];
      var application = context.Split('/')[1];

      _repository.DeleteApplication(scope, application);

      return Json(new { success = true }, JsonRequestBehavior.AllowGet);
    }

    #region Private Methods

    private Mapping GetMapping(string scope, string application)
    {
      var key = string.Format(_keyFormat, scope, application);

      if (Session[key] == null)
      {
        Session[key] = _repository.GetMapping(scope, application);
      }

      return (Mapping)Session[key];
    }

    private string GetClassLabel(string classId)
    {
      var dataEntity = _repository.GetClassLabel(classId);

      return Convert.ToString(dataEntity.Label);
    }

    private string GetKeytype(string name, List<DataProperty> properties)
    {
      var keyType = string.Empty;
      keyType = properties.FirstOrDefault(p => p.propertyName == name).keyType.ToString();

      return keyType;
    }
    private string GetDatatype(string name, List<DataProperty> properties)
    {
      var dataType = string.Empty;
      dataType = properties.FirstOrDefault(p => p.propertyName == name).dataType.ToString();

      return dataType;
    }
    #endregion
  }

}

