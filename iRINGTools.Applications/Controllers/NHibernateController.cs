﻿using System;
using System.Collections.Generic;
using System.Web.Mvc;
using iRINGTools.Web.Helpers;
using iRINGTools.Web.Models;
using log4net;
using org.iringtools.library;
using Newtonsoft.Json;
using System.Collections.Specialized;
using org.iringtools.utility;
using System.IO;

namespace org.iringtools.web.controllers
{
    public class NHibernateController : BaseController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(NHibernateController));
        private const string ONS_PREFIX = "org.iringtools.adapter.datalayer.proj_";
        private AdapterRepository _repository;

        public NHibernateController() : this(new AdapterRepository()) { }

        public NHibernateController(AdapterRepository repository)
            : base()
        {
            _repository = repository;
            _repository.AuthHeaders = _authHeaders;
        }

        public ActionResult DBProviders()
        {
            NameValueList providers = new NameValueList();            

            try
            {
                foreach (Provider provider in System.Enum.GetValues(typeof(Provider)))
                {
                    string value = provider.ToString();
                    providers.Add(new ListItem() { Name = value, Value = value });
                }

                return Json(providers, JsonRequestBehavior.AllowGet);

            }
            catch (Exception e)
            {
                return Json(new { success = false, message = e.ToString() });
            }
        }

        public ActionResult TableNames(FormCollection form)
        {
            try
            {
                Dictionary<string, string> conElts = new Dictionary<string, string>();
                conElts.Add("dbProvider", form["dbProvider"]);
                conElts.Add("dbServer", form["dbServer"]);
                conElts.Add("dbInstance", form["dbInstance"]);
                conElts.Add("serName", form["serName"]);
                conElts.Add("dbName", form["dbName"]);
                conElts.Add("dbSchema", form["dbSchema"]);
                conElts.Add("dbUserName", form["dbUserName"]);
                conElts.Add("dbPassword", form["dbPassword"]);
                conElts.Add("portNumber", form["portNumber"]);

                List<string> tableNames = _repository.GetTableNames(form["scope"], form["app"], conElts);
                return Json(new {success = true, data = tableNames});
            }
            catch (Exception e)
            {
                return Json(new { success = false, error = e.ToString() });
            }
        }

        public ActionResult ObjectsTree(FormCollection form)
        {
            try
            {
                string scope = form["scope"];
                string app = form["app"];
                string selectedTables = form["selectedTables"];

                _repository.Session = Session;

                List<Node> objectsTree = new List<Node>();

                Node root = new Node()
                {
                    text = "Data Objects",
                    type = "dataObjects",
                    iconCls = "folder",
                    expanded = true,
                    children = new List<Node>(),
                    properties = new Dictionary<string, object>()
                };

                objectsTree.Add(root);

                DatabaseDictionary dictionary = _repository.GetDBDictionary(scope, app);
                
                Dictionary<string, string> conElts = null;
                if (dictionary != null && !string.IsNullOrEmpty(dictionary.ConnectionString))
                {
                    string dbProvider = dictionary.Provider;
                    string conStr = Utility.DecodeFrom64(dictionary.ConnectionString);

                    conElts = GetConnectionElements(dbProvider, conStr);
                    root.properties.Add("connectionInfo", conElts);
                }
                else
                {
                    conElts = new Dictionary<string, string>();
                    conElts.Add("dbProvider", form["dbProvider"]);
                    conElts.Add("dbServer", form["dbServer"]);
                    conElts.Add("dbInstance", form["dbInstance"]);
                    conElts.Add("serName", form["serName"]);
                    conElts.Add("dbName", form["dbName"]);
                    conElts.Add("dbSchema", form["dbSchema"]);
                    conElts.Add("dbUserName", form["dbUserName"]);
                    conElts.Add("dbPassword", form["dbPassword"]);
                    conElts.Add("portNumber", form["portNumber"]);
                }

                List<DataObject> dbObjects = null;
                if (!string.IsNullOrEmpty(conElts["dbServer"]))
                {
                    //
                    // get available tables
                    //
                    List<string> availTables = _repository.GetTableNames(scope, app, conElts);
                    root.properties.Add("tableNames", availTables);

                    //
                    // get selected tables metadata
                    //
                    List<string> configTables = null;

                    if (!string.IsNullOrEmpty(selectedTables))
                    {
                        string[] array = selectedTables.Split(',');
                        configTables = new List<string>(array);
                    }
                    else if (dictionary != null && dictionary.dataObjects.Count > 0)
                    {
                        configTables = new List<string>();

                        foreach (DataObject dataObject in dictionary.dataObjects)
                        {
                            configTables.Add(dataObject.tableName);
                        }
                    }                    

                    if (configTables != null && configTables.Count > 0)
                    {
                        dbObjects = _repository.GetDBObjects(scope, app, conElts, configTables);
                    }
                }

                if (dbObjects != null)
                {
                    bool checkDictionary = (dictionary != null && dictionary.dataObjects != null && dictionary.dataObjects.Count > 0);
                    List<Node> objectNodes = root.children;

                    foreach (DataObject dbObject in dbObjects)
                    {
                        Node keyPropertiesNode = new Node()
                        {
                            text = "Keys",
                            type = "keys",
                            iconCls = "folder",
                            expanded = true,
                            children = new List<Node>()
                        };

                        Node dataPropertiesNode = new Node()
                        {
                            text = "Properties",
                            type = "properties",
                            iconCls = "folder",
                            expanded = true,
                            children = new List<Node>()
                        };

                        Node relationshipsNode = new Node()
                        {
                            text = "Relationships",
                            type = "relationships",
                            iconCls = "folder",
                            expanded = true,
                            children = new List<Node>()
                        };

                        // create object node
                        Node dataObjectNode = new Node()
                        {
                            text = dbObject.tableName,
                            type = "dataObject",
                            iconCls = "treeObject",
                            children = new List<Node>() { keyPropertiesNode, dataPropertiesNode, relationshipsNode },
                            properties = new Dictionary<string, object>() { 
                                {"dataProperties", dbObject.dataProperties} 
                            }
                        };

                        DataObject dictObject = null;

                        //
                        // apply what has been configured in dictionary to db object
                        //
                        if (checkDictionary)
                        {
                            dictObject = dictionary.dataObjects.Find(x => x.tableName == dbObject.tableName);
                        }

                        if (dictObject == null)  // has not been configured
                        {
                            dataObjectNode.properties.Add("objectNamespace", ONS_PREFIX + scope + "." + app);
                            dataObjectNode.properties.Add("objectName", dbObject.objectName);
                            dataObjectNode.properties.Add("tableName", dbObject.tableName);
                            dataObjectNode.properties.Add("keyDelimiter", dbObject.keyDelimeter);
                            dataObjectNode.properties.Add("description", dbObject.description);
                        }
                        else  // has been configured, apply object configurations
                        {
                            dataObjectNode.properties.Add("objectNamespace", ONS_PREFIX + scope + "." + app);
                            dataObjectNode.properties.Add("objectName", dictObject.objectName);
                            dataObjectNode.properties.Add("tableName", dictObject.tableName);
                            dataObjectNode.properties.Add("keyDelimiter", dictObject.keyDelimeter);
                            dataObjectNode.properties.Add("description", dictObject.description);

                            // apply relationship configurations
                            foreach (DataRelationship relationship in dictObject.dataRelationships)
                            {
                                Node relationshipNode = new Node()
                                {
                                    text = relationship.relationshipName,
                                    type = "relationship",
                                    iconCls = "relationship",
                                    leaf = true,
                                    properties = new Dictionary<string, object>()
                                    {
                                        {"name", relationship.relationshipName},
                                        {"type", relationship.relationshipType.ToString() },
                                        {"sourceObject", dictObject.objectName},
                                        {"relatedObject", relationship.relatedObjectName},
                                        {"propertyMaps", relationship.propertyMaps}
                                    }
                                };

                                relationshipsNode.children.Add(relationshipNode);
                            }
                        }

                        foreach (DataProperty dbProperty in dbObject.dataProperties)
                        {
                            if (dictObject != null)
                            {
                                DataProperty dictProperty = dictObject.dataProperties.Find(x => x.columnName == dbProperty.columnName);

                                if (dictProperty != null)   // property has been configured
                                {
                                    Dictionary<string, object> properties = new Dictionary<string, object>()
                                    {
                                        {"columnName", dictProperty.columnName},
                                        {"propertyName", dictProperty.propertyName},
                                        {"dataType", dictProperty.dataType},
                                        {"dataLength", dictProperty.dataLength},
                                        {"isNullable", dictProperty.isNullable},
                                        {"keyType", dictProperty.keyType},
                                        {"numberOfDecimals", dictProperty.numberOfDecimals},
                                        {"isReadOnly", dictProperty.isReadOnly},
                                        {"showOnIndex", dictProperty.showOnIndex},
                                        {"showOnSearch", dictProperty.showOnSearch},
                                        {"isHidden", dictProperty.isHidden},
                                        {"isVirtual", dictProperty.isVirtual}
                                    };

                                    bool isKey = false;

                                    if (dictObject.isKeyProperty(dictProperty.columnName))
                                    {
                                        Node keyPropertyNode = new Node()
                                        {
                                            text = dictProperty.columnName,
                                            type = "keyProperty",
                                            iconCls = "treeKey",
                                            leaf = true,
                                            properties = properties
                                        };

                                        keyPropertiesNode.children.Add(keyPropertyNode);
                                        isKey = true;
                                    }
                                    else if (!isKey)
                                    {
                                        Node dataPropertyNode = new Node()
                                        {
                                            text = dictProperty.columnName,
                                            type = "dataProperty",
                                            iconCls = "treeProperty",
                                            leaf = true,
                                            properties = properties
                                        };

                                        dataPropertiesNode.children.Add(dataPropertyNode);
                                    }
                                }
                            }
                        }

                        objectNodes.Add(dataObjectNode);
                    }
                }

                return Json(objectsTree);
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = e.ToString() });
            }
        }

        public ActionResult SaveDBDictionary()
        {
            try
            {
                string scope = Request.Params["scope"];
                string app = Request.Params["app"];

                var reader = new StreamReader(Request.InputStream);
                var json = reader.ReadToEnd();

                DatabaseDictionary dictionary = Utility.FromJson<DatabaseDictionary>(json);
                Response response = _repository.SaveDBDictionary(scope, app, dictionary);

                if (response.Level == StatusLevel.Success)
                {
                    string dictKey = string.Format("Dictionary.{0}.{1}", scope, app);
                    Session[dictKey] = dictionary;
                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    string message = string.Join(" ", response.Messages);
                    return Json(new { success = false, message =  message});
                }
            }
            catch (Exception e)
            {
                return Json(new { success = true, message = e.ToString() });
            }
        }

        private string GetOracleConnectionElement(string conStr, string element)
        {
            string token = element + "=";
            int startIndex = conStr.IndexOf(token) + token.Length + 1;
            int endIndex = conStr.IndexOf(')', startIndex);
            string value = conStr.Substring(startIndex, endIndex - startIndex);
            return value;
        }

        // mssql --- "Data Source=server\\instance;Initial Catalog=dbName;User ID=user;Password=pwd"
        // oracle -- Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=host)(PORT=port))(CONNECT_DATA=(SERVICE_NAME|SID=instance)));User Id=user;Password=pwd"
        private Dictionary<string, string> GetConnectionElements(string dbProvider, string constr)
        {
            try
            {
                Dictionary<string, string> connStrElts = new Dictionary<string, string>();
                connStrElts.Add("dbProvider", dbProvider);
                connStrElts.Add("portNumber", "1433");
                connStrElts.Add("dbInstance", string.Empty);
                connStrElts.Add("dbName", string.Empty);
                connStrElts.Add("dbSchema", "dbo");
                connStrElts.Add("dbServer", string.Empty);
                connStrElts.Add("dbUserName", string.Empty);
                connStrElts.Add("dbPassword", string.Empty);
                connStrElts.Add("serName", string.Empty);

                string provider = dbProvider.ToUpper();

                string[] parts = constr.Split(';');
                foreach (string part in parts)
                {
                    string[] pair = part.Split('=');
                    string key = pair[0];
                    string value = pair[1];

                    switch (key.ToLower())
                    {
                        case "data source":
                            if (provider.IndexOf("MSSQL") > -1)
                            {
                                string[] serverInstance = value.Split('\\');

                                connStrElts["dbServer"] = serverInstance[0];
                                connStrElts["dbInstance"] = serverInstance[1];
                            }
                            else if (provider.IndexOf("ORACLE") > -1)
                            {
                                string server = GetOracleConnectionElement(value, "HOST");
                                connStrElts["dbServer"] = server;

                                string port = GetOracleConnectionElement(value, "PORT");
                                connStrElts["portNumber"] = port;

                                int serviceType = value.IndexOf("SERVICE_NAME");
                                if (serviceType != -1)
                                {
                                    connStrElts["serName"] = "SERVICE_NAME";

                                    string serviceName = GetOracleConnectionElement(value, "SERVICE_NAME");
                                    connStrElts["dbInstance"] = serviceName;
                                }
                                else
                                {
                                    connStrElts["serName"] = "SID";

                                    string serviceName = GetOracleConnectionElement(value, "SID");
                                    connStrElts["dbInstance"] = serviceName;
                                }
                            }
                            break;
                        case "initial catalog":
                            connStrElts["dbName"] = value;
                            break;
                        case "user id":
                            connStrElts["dbUserName"] = value;
                            break;
                        case "password":
                            connStrElts["dbPassword"] = value;
                            break;
                    }
                }

                return connStrElts;
            }
            catch (Exception e)
            {
                _logger.Error("Error parsing connection string [" + constr + "]: " + e.ToString());
                throw e;
            }
        }
    }
}
