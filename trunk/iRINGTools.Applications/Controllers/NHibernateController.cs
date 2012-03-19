using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using org.iringtools.web.controllers;
using org.iringtools.adapter.security;
using System.Collections;
using log4net;
using iRINGTools.Web.Helpers;
using org.iringtools.library;
using org.iringtools.web.Models;

namespace org.iringtools.web.Controllers
{
    public class NHibernateController : BaseController
    {
        //protected IAuthenticationLayer _authenticationLayer = new OAuthProvider();
        //protected IDictionary _allClaims = new Dictionary<string, string>();
        //protected string _oAuthToken = String.Empty;
        //protected IAuthorizationLayer _authorizationLayer = new LdapAuthorizationProvider();
        private static readonly ILog _logger = LogManager.GetLogger(typeof(BaseController));
        private NHibernateRopsitory _repository;
        private string _keyFormat = "Datadictionary.{0}.{1}";

        public NHibernateController() : this(new NHibernateRopsitory()) { }

        public NHibernateController(NHibernateRopsitory repository)
            : base()
        {
            _repository = repository;
        }

        public ActionResult Index()
        {
            return View();
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
                    case "DATAOBJECTS":
                        {
                            string contextName = form["contextName"];
                            string endpoint = form["endpoint"];
                            DatabaseDictionary dbDictionary =  GetDbDictionary(form["contextName"], form["endpoint"]);
                            List<JsonTreeNode> nodes = new List<JsonTreeNode>();

                            foreach (DataObject dataObject in dbDictionary.dataObjects)
                            {
                                JsonTreeNode node = new JsonTreeNode
                                {
                                    nodeType = "async",
                                    type = "DATAOBJECT",
                                    iconCls = "treeObject",
                                    id = form["node"] + "/DataObject/" + dataObject.objectName,
                                    text = dataObject.objectName,
                                    expanded = false,
                                    leaf = false,

                                    record = new
                                    {
                                        Name = dataObject.objectName,
                                        securityRole = securityRole
                                    }
                                };
                                node.property = new Dictionary<string, string>();
                                node.property.Add("tableName", dataObject.tableName.Trim());
                                node.property.Add("objectNamespace", dataObject.objectNamespace.Trim());
                                node.property.Add("objectName", dataObject.objectName.Trim());
                                node.property.Add("keyDelimiter", dataObject.keyDelimeter.Trim());

                            //    AddContextEndpointtoNode(node, form);
                                nodes.Add(node);
                            }
                            return Json(nodes, JsonRequestBehavior.AllowGet);

                        }
                    case "DATAOBJECT":
                        {
                            List<JsonTreeNode> nodes = new List<JsonTreeNode>();
                            JsonTreeNode keysNode = new JsonTreeNode
                            {
                                type = "KEYS",
                                iconCls = "folder",
                                id = form["node"] + "/DataObject/KEYS",
                                text = "Keys",
                                expanded = false,
                                leaf = false,
                                record = new 
                                {
                                    Name = form["id"]
                                }
                            };
                            nodes.Add(keysNode);
                            JsonTreeNode propsNode = new JsonTreeNode
                            {
                                type = "PROPERTIES",
                                iconCls = "folder",
                                id = form["node"] + "/DataObject/PROPERTIES",
                                text = "Properties",
                                expanded = false,
                                leaf = false,
                                record = new
                                {
                                    Name = form["id"]
                                }
                            };
                            nodes.Add(propsNode);
                            JsonTreeNode relsNode = new JsonTreeNode
                            {
                                type = "RELATIONSHIPS",
                                iconCls = "folder",
                                id = form["node"] + "/DataObject/RELATIONSHIPS",
                                text = "Relationships",
                                expanded = false,
                                leaf = false,
                                record = new
                                {
                                    Name = form["id"]
                                }
                            };
                            nodes.Add(relsNode);
                            return Json(nodes, JsonRequestBehavior.AllowGet);
                        }
                    case "KEYS":
                        {
                            string contextName = form["contextName"];
                            string endpoint = form["endpoint"];
                            string dbobject = form["id"];
                            List<JsonTreeNode> nodes = new List<JsonTreeNode>();
                            DatabaseDictionary dictionary = GetDbDictionary(form["contextName"], form["endpoint"]);
                            var dbobj = dictionary.dataObjects.Where(d => d.objectName == dbobject).FirstOrDefault();
                            var keys = dbobj.keyProperties.ToList();
                            foreach (var k in keys)
                            {
                                JsonTreeNode keynode = new JsonTreeNode
                                {
                                    type = "KEYPROPERTY",
                                    iconCls = _repository.GetNodeIconCls("key"),
                                    text = k.keyPropertyName,
                                    leaf = true
                                };
                                nodes.Add(keynode);
                            }
                            return Json(nodes, JsonRequestBehavior.AllowGet);
                        }
                    case "PROPERTIES":
                        {
                            string contextName = form["contextName"];
                            string endpoint = form["endpoint"];
                            string dbobject = form["id"];
                            List<JsonTreeNode> nodes = new List<JsonTreeNode>();
                            DatabaseDictionary dictionary = GetDbDictionary(form["contextName"], form["endpoint"]);
                            var dbobj = dictionary.dataObjects.Where(d => d.objectName == dbobject).FirstOrDefault();
                            var keyprops = dbobj.keyProperties.ToList();
                            var dataprops = dbobj.dataProperties.ToList();
                            foreach (var p in dataprops)
                            {
                                if (keyprops.Any(k=>k.keyPropertyName.Equals(p.propertyName))) continue;
                                JsonTreeNode datanode = new JsonTreeNode
                                {
                                    type = "DATAPROPERTY",
                                    iconCls = _repository.GetNodeIconCls("property"),
                                    text = p.propertyName,
                                    leaf = true
                                };
                                nodes.Add(datanode);
                            }
                            return Json(nodes, JsonRequestBehavior.AllowGet);
                        }
                    case "RELATIONSHIPS":
                        {
                            List<JsonTreeNode> nodes = new List<JsonTreeNode>();
                            return Json(nodes, JsonRequestBehavior.AllowGet);
                        }
                            //string datatype, keytype;
                            //string context = form["node"];
                            //string dataObjectName = form["text"];
                            //string contextName = form["contextName"];
                            //string endpoint = form["endpoint"];

                            //DataDictionary dictionary = _repository.GetDictionary(contextName, endpoint);
                            //DataObject dataObject = dictionary.dataObjects.FirstOrDefault(o => o.objectName == dataObjectName);

                            //List<JsonTreeNode> nodes = new List<JsonTreeNode>();

                            //foreach (DataProperty properties in dataObject.dataProperties)
                            //{
                            //    keytype = GetKeytype(properties.propertyName, dataObject.dataProperties);
                            //    datatype = GetDatatype(properties.propertyName, dataObject.dataProperties);
                            //    TreeNode node = new TreeNode
                            //    {
                            //        //nodeType = "async",
                            //        type = (dataObject.isKeyProperty(properties.propertyName)) ? "KeyDataPropertyNode" : "DataPropertyNode",
                            //        iconCls = (dataObject.isKeyProperty(properties.propertyName)) ? _repository.GetNodeIconCls("key") : _repository.GetNodeIconCls("property"),
                            //        id = context + "/" + properties.propertyName,
                            //        text = properties.propertyName,
                            //        expanded = true,
                            //        leaf = false,
                            //        children = new List<JsonTreeNode>(),
                            //        record = new
                            //        {
                            //            Name = properties.propertyName,
                            //            Keytype = keytype,
                            //            Datatype = datatype,
                            //            securityRole = securityRole
                            //        }
                            //    };
                            //    node.property = new Dictionary<string, string>();
                            //    node.property.Add("Name", properties.propertyName);
                            //    node.property.Add("Keytype", keytype);
                            //    node.property.Add("Datatype", datatype);
                            //    AddContextEndpointtoNode(node, form);
                            //    nodes.Add(node);
                        //    }
                        //    if (dataObject.dataRelationships.Count > 0)
                        //    {
                        //        foreach (DataRelationship relation in dataObject.dataRelationships)
                        //        {
                        //            TreeNode node = new TreeNode
                        //            {
                        //                nodeType = "async",
                        //                type = "RelationshipNode",
                        //                iconCls = "treeRelation",
                        //                id = context + "/" + dataObject.objectName + "/" + relation.relationshipName,
                        //                text = relation.relationshipName,
                        //                expanded = false,
                        //                leaf = false,

                        //                record = new
                        //                {
                        //                    Name = relation.relationshipName,
                        //                    Type = relation.relationshipType,
                        //                    Related = relation.relatedObjectName,
                        //                    securityRole = securityRole
                        //                }
                        //            };
                        //            node.property = new Dictionary<string, string>();
                        //            node.property.Add("Name", relation.relationshipName);
                        //            node.property.Add("Type", relation.relationshipType.ToString());
                        //            node.property.Add("Related", relation.relatedObjectName);
                        //            AddContextEndpointtoNode(node, form);
                        //            nodes.Add(node);
                        //        }
                        //    }
                        //    return Json(nodes, JsonRequestBehavior.AllowGet);

                        //}

                    case "RELATIONSHIP":
                        {
                            string keytype, datatype;
                            string context = form["node"];
                            string related = form["related"];
                            string contextName = form["contextName"];
                            string endpoint = form["endpoint"];

                            List<JsonTreeNode> nodes = new List<JsonTreeNode>();
                            DatabaseDictionary dictionary = GetDbDictionary(form["contextName"], form["endpoint"]);
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

        private DatabaseDictionary GetDbDictionary(string contextName, string endpoint)
        {
            string key = string.Format(_keyFormat, contextName, endpoint);

            if (Session[key] == null)
            {
                Session[key] = _repository.GetDBDictionary(contextName, endpoint);
            }

            return (DatabaseDictionary)Session[key];
        }

        public ActionResult UpdateKeyProperties(FormCollection form)
        {
            try
            {
            }
            catch
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult AvailableProperties(FormCollection form)
       {
             try
            {
            }
            catch
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
      }

        public ActionResult SelectedProperties(FormCollection form)
        {
             try
            {
            }
            catch
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateDataProperties(FormCollection form)
        {
            try
            {
            }
            catch
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateDbObject(FormCollection form)
        {
            try
            {
                var dbTableName = form["tableName"];
                var objNameSpace = form["objectNamespace"];
                var objName = form["objectName"];
                var keyDelim = form["keyDelimeter"];
                var context = form["contextName"];
                var endpoint = form["endpoint"];
                var dbDict = GetDbDictionary(context, endpoint);
                DataObject dataObject = dbDict.dataObjects.Where(d=>d.tableName.Equals(dbTableName)).FirstOrDefault();
                if (dataObject != null)
                {
                    dataObject.objectName = objName;
                    dataObject.objectNamespace = objNameSpace;
                    dataObject.keyDelimeter = keyDelim;
                }
            }
            catch
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DBObjects(FormCollection form)
        {
            try
            {
                List<JsonTreeNode> dbObjects = _repository.GetDBObjects(
                  form["scope"], form["app"], form["dbProvider"], form["dbServer"], form["dbInstance"],
                  form["dbName"], form["dbSchema"], form["dbUserName"], form["dbPassword"], form["tableNames"], form["portNumber"], form["serName"]);

                return Json(dbObjects, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw e;
            }
        }

        public ActionResult Trees(FormCollection form)
        {
            try
            {
                string response = string.Empty;

                response = _repository.SaveDBDictionary(form["scope"], form["app"], form["tree"]);

                if (response != null && response.ToUpper().Contains("ERROR"))
                {
                    int inds = response.ToUpper().IndexOf("<MESSAGE>");
                    int inde = response.ToUpper().IndexOf(";");
                    string msg = response.Substring(inds + 9, inde - inds - 13);
                    return Json(new { success = false } + msg, JsonRequestBehavior.AllowGet);
                }
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw e;
            }
        }

        public ActionResult DataType()
        {
            try
            {
                Dictionary<String, String> dataTypeNames = new Dictionary<String, String>();

                foreach (DataType dataType in Enum.GetValues(typeof(DataType)))
                {
                    dataTypeNames.Add(((int)dataType).ToString(), dataType.ToString());
                }

                return Json(dataTypeNames, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw e;
            }
        }

        public JsonResult RegenAll()
        {
            Response response = _repository.RegenAll();
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DBProviders()
        {
            JsonContainer<List<DBProvider>> container = new JsonContainer<List<DBProvider>>();

            try
            {
                DataProviders dataProviders = _repository.GetDBProviders();
                

                List<DBProvider> providers = new List<DBProvider>();
                foreach (Provider dataProvider in dataProviders.Distinct().ToList())
                {
                   
                        providers.Add(new DBProvider() { Provider = dataProvider.ToString() });
                   
                }

                container.items = providers;
                container.success = true;
                container.total = providers.Count;

            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw e;
            }

            return Json(container, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DBDictionary(FormCollection form)
        {
            try
            {
                DatabaseDictionary dbDict = GetDbDictionary(form["scope"], form["app"]);
                return Json(dbDict, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw e;
            }
        }

        public ActionResult ParseConnectionString(FormCollection form)
        {
          string connStr = string.Empty;
          string dbProvider = form["dbProvider"];
          string dbServer = form["dbServer"];
          string dbInstance = form["dbInstance"];
          string dbName = form["dbName"];
          string dbUserName = form["dbUserNAme"];
          string dbPassword = form["dbPassword"];
          string portNumber = form["portNumber"];
          string serName = form["serName"];
          try
          {
            if (dbProvider.ToUpper().Contains("MSSQL"))
              connStr = String.Format("Data Source={0}\\{1};Initial Catalog={2};User ID={3};Password={4}",
                dbServer, dbInstance, dbName, dbUserName, dbPassword);
            else
              connStr = String.Format("Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST={0})(PORT={1})))(CONNECT_DATA=(SERVER=DEDICATED)({2}={3})));User ID={4};Password={5}",
                dbServer, portNumber, serName, dbInstance, dbUserName, dbPassword);

          }
          catch (Exception e)
          {
            _logger.Error(e.ToString());
            throw e;
          }
          return Json(connStr, JsonRequestBehavior.AllowGet);
        }

        public ActionResult TableNames(FormCollection form)
        {
            JsonContainer<List<string>> container = new JsonContainer<List<string>>();

            try
            {
                List<string> dataObjects = _repository.GetTableNames(
                  form["scope"], form["app"], form["dbProvider"], form["dbServer"], form["dbInstance"],
                  form["dbName"], form["dbSchema"], form["dbUserName"], form["dbPassword"], form["portNumber"], form["serName"]);


                container.items = dataObjects;
                container.success = true;
                container.total = dataObjects.Count;
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
                throw e;
            }

            return Json(container, JsonRequestBehavior.AllowGet);
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

        private void AddContextEndpointtoNode(JsonTreeNode node, FormCollection form)
        {
            if (form["contextName"] != null)
                node.property.Add("context", form["contextName"]);
            if (form["endpoint"] != null)
                node.property.Add("endpoint", form["endpoint"]);
        }
    }

    public class DBProvider
    {
        public string Provider { get; set; }
    }
}
