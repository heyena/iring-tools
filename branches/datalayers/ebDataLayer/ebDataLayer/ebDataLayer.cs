using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using eB.Service.Client;
using eB.Data;
using System.Xml;
using org.iringtools.library;
using org.iringtools.adapter;
using Ninject;
using org.iringtools.utility;
using System.Data;
using System.Text.RegularExpressions;
using eB.Common.Enum;
using log4net;
using org.iringtools.adaper.datalayer.eb;
using org.iringtools.adaper.datalayer.eb.config;
using System.Xml.Linq;
using StaticDust.Configuration;

namespace org.iringtools.adapter.datalayer.eb
{
  public class ebDataLayer : BaseDataLayer
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(ebDataLayer));

    private string _dataPath = string.Empty;
    private string _scope = string.Empty;
    private string _dictionaryPath = string.Empty;
    private DataDictionary _dictionary = null;

    private string _server = string.Empty;
    private string _dataSource = string.Empty;
    private string _userName = string.Empty;
    private string _password = string.Empty;
    private string _filteredClasses = string.Empty;
    private string _keyDelimiter = string.Empty;

    private Proxy _proxy = null;
    private Session _session = null;
    private Configuration _config = null;
    private Rules _rules = null;
    private GroupTypes _groupTypes = null;

    [Inject]
    public ebDataLayer(AdapterSettings settings)
      : base(settings)
    {
      try
      {
        _dataPath = settings["DataLayerPath"];
        if (_dataPath == null)
        {
          _dataPath = settings["AppDataPath"];
        }

        _scope = _settings["ProjectName"] + "." + _settings["ApplicationName"];
        
        //
        // Load AppSettings
        //
        string appSettingsPath = string.Format("{0}{1}.config", _dataPath, _scope);
        if (!System.IO.File.Exists(appSettingsPath))
        {
          _dataPath += "App_Data\\";
          appSettingsPath = string.Format("{0}{1}.config", _dataPath, _scope);
        }
        _settings.AppendSettings(new AppSettingsReader(appSettingsPath));

        _dictionaryPath = string.Format("{0}DataDictionary.{1}.xml", _dataPath, _scope);

        _server = _settings["ebServer"];
        _dataSource = _settings["ebDataSource"];
        _userName = _settings["ebUserName"];
        _password = _settings["ebPassword"];
        _filteredClasses = _settings["ebFilteredClasses"];

        _keyDelimiter = _settings["ebKeyDelimiter"];
        if (_keyDelimiter == null)
        {
          _keyDelimiter = string.Empty;
        }

        Connect();

        int docId = int.Parse(_settings["ebDocId"]);
        string communityName = _settings["ebCommunityName"];

        EqlClient eqlClient = new EqlClient(_session);
        string templateName = eqlClient.GetDocumentTemplate(docId);

        _groupTypes = Utility.Read<GroupTypes>(_dataPath + "GroupTypes.xml", false);
        _config = Utility.Read<Configuration>(_dataPath + templateName + "_" + communityName + ".xml", false);
        _rules = Utility.Read<Rules>(_dataPath + "Rules_" + communityName + ".xml", false);
      }
      catch (Exception e)
      {
        _logger.Error("Error initializing ebDataLayer: " + e.Message);
      }
      finally
      {
        Disconnect();
      }
    }

    public override DataDictionary GetDictionary()
    {
      if (_dictionary != null)
        return _dictionary;

      if (System.IO.File.Exists(_dictionaryPath))
      {
        _dictionary = Utility.Read<DataDictionary>(_dictionaryPath);
        return _dictionary;
      }

      try
      {
        Connect();

        _dictionary = new DataDictionary();

        StringBuilder cosQuery = new StringBuilder("SELECT * FROM class_objects");

        if (!string.IsNullOrEmpty(_filteredClasses))
        {
          string[] classCodes = _filteredClasses.Split(',');
          cosQuery.Append(" WHERE ");

          for (int i = 0; i < classCodes.Length; i++)
          {
            string[] parts = classCodes[i].Trim().Split('.');
            GroupType groupType = _groupTypes.Single(x => x.Name == parts[0]);

            if (groupType == null)
            {
              throw new Exception("Group [" + parts[0] + "] not configured.");
            }

            string code = parts[1];

            if (i > 0)
            {
              cosQuery.Append(" or ");
            }

            cosQuery.Append(string.Format("(group_id = {0} AND name = '{1}')", groupType.Value, code));
          }
        }

        XmlDocument cosDoc = new XmlDocument();
        int status = 0;
        string cosResults = _proxy.query(cosQuery.ToString(), ref status);
        cosDoc.LoadXml(cosResults);

        foreach (XmlNode coNode in cosDoc.DocumentElement.ChildNodes)
        {
          string classCode = coNode.SelectSingleNode("code").InnerText;
          string classId = coNode.SelectSingleNode("class_id").InnerText;
          string className = coNode.SelectSingleNode("name").InnerText;
          int groupId = int.Parse(coNode.SelectSingleNode("group_id").InnerText);
          GroupType group = null;

          try
          {
            group = _groupTypes.Single<org.iringtools.adaper.datalayer.eb.GroupType>(x => x.Value == groupId);
          }
          catch (Exception e)
          {
            _logger.Error("Group type not supported: " + e.Message);
            throw new Exception(e.Message);
          }

          string objectName = className + "(" + group.Name + ")";
          if (_dictionary.dataObjects.Find(x => x.objectName == objectName) != null)
            continue;

          EqlClient eqlClient = new EqlClient(_session);
          List<string> subClassIds = eqlClient.GetSubClassIds(className);

          DataObject objDef = new DataObject();
          objDef.objectNamespace = group.Name;
          objDef.objectName = objectName;
          objDef.tableName = (subClassIds.Count == 0)
            ? classId : string.Join(",", subClassIds.ToArray());
          objDef.keyDelimeter = _keyDelimiter;

          Map codeMap = _config.Mappings.ToList<Map>().Find(x => x.Destination == (int)Destination.Code);
          if (codeMap == null)
          {
            throw new Exception("No mapping configured for key property.");
          }

          objDef.keyProperties = new List<KeyProperty>()
          {
            new KeyProperty() { keyPropertyName = codeMap.Column }
          };

          string metadataQuery = _settings["ebMetadataQuery." + group.Name];

          if (string.IsNullOrEmpty(metadataQuery))
            throw new Exception("No metadata query configured for group [" + group.Name + "]");

          int objectType = (int)Enum.Parse(typeof(ObjectType), group.Name);
          string attrsQuery = string.Format(metadataQuery, objectType);

          XmlDocument attrsDoc = new XmlDocument();
          string attrsResults = _proxy.query(attrsQuery, ref status);
          attrsDoc.LoadXml(attrsResults);

          foreach (XmlNode attrNode in attrsDoc.DocumentElement.ChildNodes)
          {
            DataProperty dataProp = new DataProperty();
            dataProp.columnName = attrNode.SelectSingleNode("char_name").InnerText;

            string propertyName = Utilities.ToPropertyName(dataProp.columnName);
            if (objDef.dataProperties.Find(x => x.propertyName == propertyName) != null)
              continue;

            dataProp.propertyName = propertyName;
            dataProp.dataType = Utilities.ToCSharpType(attrNode.SelectSingleNode("char_data_type").InnerText);
            dataProp.dataLength = Int32.Parse(attrNode.SelectSingleNode("char_length").InnerText);

            if (attrNode.SelectSingleNode("is_system_char").InnerText == "1")
            {
              dataProp.columnName += Utilities.SYSTEM_ATTRIBUTE_TOKEN;
            }
            else
            {
              dataProp.columnName += Utilities.USER_ATTRIBUTE_TOKEN;
            }

            objDef.dataProperties.Add(dataProp);
          }

          // add related properties
          foreach (Map m in _config.Mappings.Where(x => x.Destination == (int)Destination.Relationship).Select(m => m))
          {
            DataProperty dataProp = new DataProperty();            
            string propertyName = Utilities.ToPropertyName(m.Column);
            DataProperty checkProp = objDef.dataProperties.Find(x => x.propertyName == propertyName);

            if (checkProp != null)  // property already exists, update its column name
            {
              checkProp.columnName = m.Column + Utilities.RELATED_ATTRIBUTE_TOKEN;
            }
            else
            {
              dataProp.columnName = m.Column + Utilities.RELATED_ATTRIBUTE_TOKEN;
              dataProp.propertyName = propertyName;
              dataProp.dataType = DataType.String;
              objDef.dataProperties.Add(dataProp);
            }
          }

          // add other properties
          foreach (Map m in _config.Mappings.Where(x => x.Destination != (int)Destination.Relationship &&
            x.Destination != (int)Destination.Attribute && x.Destination != (int)Destination.None).Select(m => m))
          {
            DataProperty dataProp = new DataProperty();
            string propertyName = Utilities.ToPropertyName(m.Column);
            DataProperty checkProp = objDef.dataProperties.Find(x => x.propertyName == propertyName);

            if (checkProp != null)  // property already exists, update its column name
            {
              checkProp.columnName = m.Column + Utilities.OTHER_ATTRIBUTE_TOKEN;
            }
            else
            {
              dataProp.columnName = m.Column + Utilities.OTHER_ATTRIBUTE_TOKEN;
              dataProp.propertyName = propertyName;
              dataProp.dataType = DataType.String;
              objDef.dataProperties.Add(dataProp);
            }
          }
          
          _dictionary.dataObjects.Add(objDef);
        }

        Utility.Write<DataDictionary>(_dictionary, _dictionaryPath);
        return _dictionary;
      }
      catch (Exception e)
      {
        throw e;
      }
      finally
      {
        Disconnect();
      }
    }

    public override long GetCount(string objectType, DataFilter filter)
    {
      try
      {
        DataDictionary dictionary = GetDictionary();
        DataObject objDef = dictionary.dataObjects.Find(x => x.objectName.ToLower() == objectType.ToLower());

        if (objDef != null)
        {
          Connect();

          int objType = (int)_config.Template.ObjectType;
          string eql = string.Empty;

          if (objType == (int)ObjectType.Tag)
          {
            eql = string.Format("START WITH Tag WHERE Class.Id IN ({0})", objDef.tableName);
          }
          else if (objType == (int)ObjectType.Document)
          {
            eql = string.Format("START WITH Document WHERE Class.Id IN ({0})", objDef.tableName);
          }
          else
          {
            throw new Exception(string.Format("Object type [{0}] not supported.", objectType));
          }

          string whereClause = Utilities.ToSqlWhereClause(filter, objDef);
          if (!string.IsNullOrEmpty(whereClause))
          {
            eql += whereClause.Replace(" WHERE ", " AND ");
          }

          EqlClient eqlClient = new EqlClient(_session);
          DataTable dt = eqlClient.RunQuery(eql);
          return Convert.ToInt64(dt.Rows.Count);
        }
        else
        {
          throw new Exception(string.Format("Object type [{0}] not found.", objectType));
        }
      }
      catch (Exception e)
      {
        _logger.Error(string.Format("Error getting object count for [{0}]: {1}", objectType, e.Message));
        throw e;
      }
      finally
      {
        Disconnect();
      }
    }

    public override IList<IDataObject> Get(string objectType, DataFilter filter, int pageSize, int startIndex)
    {
      IList<IDataObject> dataObjects = new List<IDataObject>();

      try
      {
        Connect();

        DataDictionary dictionary = GetDictionary();
        DataObject objDef = dictionary.dataObjects.Find(x => x.objectName.ToLower() == objectType.ToLower());

        if (objDef != null)
        {
          string classObject = objDef.objectNamespace;
          string classCodes = "'" + string.Join("','", objDef.tableName.Split(',')) + "'";

          if (classObject.ToLower() == "document" || classObject.ToLower() == "tag")
          {
            string eql = "START WITH {0} SELECT {1} WHERE Class.Id IN ({2})";
            StringBuilder builder = new StringBuilder();

            foreach (DataProperty dataProp in objDef.dataProperties)
            {
              string item = Utilities.ToQueryItem(dataProp);

              if (!string.IsNullOrEmpty(item))
              {
                if (builder.Length > 0)
                  builder.Append(",");

                builder.Append(item);       
              }
            }

            eql = string.Format(eql, classObject, builder.ToString(), classCodes);

            string whereClause = Utilities.ToSqlWhereClause(filter, objDef);
            if (!string.IsNullOrEmpty(whereClause))
            {
              eql += whereClause.Replace(" WHERE ", " AND ");
            }

            EqlClient eqlClient = new EqlClient(_session);
            DataTable result = eqlClient.Search(_session, eql, new object[0], startIndex, pageSize);

            dataObjects = ToDataObjects(result, objDef);
          }
          else
          {
            throw new Exception("Class object [" + classObject + "] not supported.");
          }
        }
        else
        {
          throw new Exception("Object type " + objectType + " not found.");
        }
      }
      finally
      {
        Disconnect();
      }

      return dataObjects;
    }

    public override IList<string> GetIdentifiers(string objectType, DataFilter filter)
    {
      try
      {
        IList<IDataObject> dataObjects = Get(objectType, filter, 0, -1);

        DataDictionary dictionary = GetDictionary();
        DataObject objDef = dictionary.dataObjects.Find(x => x.objectName.ToLower() == objectType.ToLower());

        IList<string> identifiers = new List<string>();

        foreach (IDataObject dataObject in dataObjects)
        {
          identifiers.Add(Convert.ToString(dataObject.GetPropertyValue(objDef.keyProperties.First().keyPropertyName)));
        }

        return identifiers;
      }
      catch (Exception e)
      {
        _logger.Error(string.Format("Error getting identifiers of object type [{0}]", objectType));
        throw e;
      }
    }

    public override IList<IDataObject> Get(string objectType, IList<string> identifiers)
    {
      IList<IDataObject> dataObjects = new List<IDataObject>();

      try
      {
        Connect();

        DataDictionary dictionary = GetDictionary();
        DataObject objDef = dictionary.dataObjects.Find(x => x.objectName.ToLower() == objectType.ToLower());

        if (objDef != null)
        {
          string classObject = objDef.objectNamespace;
          string key = objDef.keyProperties.FirstOrDefault().keyPropertyName;
          string keyValues = "('" + string.Join("','", identifiers) + "')";

          if (classObject.ToLower() == "document" || classObject.ToLower() == "tag")
          {
            string eql = "START WITH {0} SELECT {1} WHERE {2} IN {3}";
            StringBuilder builder = new StringBuilder();

            foreach (DataProperty dataProp in objDef.dataProperties)
            {
              string item = Utilities.ToQueryItem(dataProp);

              if (!string.IsNullOrEmpty(item))
              {
                if (builder.Length > 0)
                  builder.Append(",");

                builder.Append(item);
              }
            }

            eql = string.Format(eql, classObject, builder.ToString(), key, keyValues);

            EqlClient eqlClient = new EqlClient(_session);
            DataTable result = eqlClient.Search(_session, eql, new object[0], 0, -1);

            dataObjects = ToDataObjects(result, objDef);
          }
          else
          {
            throw new Exception("Class object [" + classObject + "] not currently supported.");
          }
        }
        else
        {
          throw new Exception("Object type " + objectType + " not found.");
        }

      }
      finally
      {
        Disconnect();
      }

      return dataObjects;
    }

    public override Response Post(IList<IDataObject> dataObjects)
    {
      Response response = new Response();

      try
      {
        if (dataObjects.Count <= 0)
        {
          response.Level = StatusLevel.Error;
          response.Messages.Add("No data objects to update.");
          return response;
        }

        DataDictionary dictionary = GetDictionary();
        string objType = ((GenericDataObject)dataObjects[0]).ObjectType;
        DataObject objDef = dictionary.dataObjects.Find(x => x.objectName.ToLower() == objType.ToLower());

        Connect();

        foreach (IDataObject dataObject in dataObjects)
        {
          KeyProperty keyProp = objDef.keyProperties.FirstOrDefault();
          string keyValue = Convert.ToString(dataObject.GetPropertyValue(keyProp.keyPropertyName));

          string revision = string.Empty;
          Map revisionMap = _config.Mappings.ToList<Map>().Find(x => x.Destination == (int)Destination.Revision);
          if (revisionMap != null)
          {
            string propertyName = Utilities.ToPropertyName(revisionMap.Column);
            revision = Convert.ToString(dataObject.GetPropertyValue(propertyName));
          }

          EqlClient eql = new EqlClient(_session);
          int objectId = eql.GetObjectId(keyValue, revision, _config.Template.ObjectType);
          org.iringtools.adaper.datalayer.eb.config.Template template = _config.Template;

          if (objectId == 0)  // does not exist, create
          {
            string templateName = GetTemplateName(template, objDef, dataObject);
            int templateId = eql.GetTemplateId(templateName);

            if (templateId == 0)
            {
              Status status = new Status()
              {
                Identifier = keyValue,
                Level = StatusLevel.Error,
                Messages = new Messages() { string.Format("Template [{0}] for the object does not exist.", templateName) }
              };

              response.StatusList.Add(status);
              break;
            }

            objectId = _session.Writer.CreateFromTemplate(templateId, "", "");
          }

          string objectType = Enum.GetName(typeof(ObjectType), template.ObjectType);
          ebProcessor processor = new ebProcessor(_session, _config.Mappings.ToList<Map>(), _rules);

          if (objectType == ObjectType.Tag.ToString())
          {
            response.Append(processor.ProcessTag(objDef, dataObject, objectId, keyValue));
          }
          else if (objectType == ObjectType.Document.ToString())
          {
            response.Append(processor.ProcessDocument(objDef, dataObject, objectId, keyValue));
          }
          else
          {
            Status status = new Status()
            {
              Identifier = keyValue,
              Level = StatusLevel.Error,
              Messages = new Messages() { string.Format("Object type [{0}] is not supoorted in this version.", template.ObjectType) }
            };

            response.StatusList.Add(status);
          }
        }
      }
      catch (Exception e)
      {
        _logger.Error("Error posting data objects: " + e);

        response.Level = StatusLevel.Error;
        response.Messages.Add("Error posting data objects: " + e);
      }
      finally
      {
        Disconnect();
      }

      return response;
    }

    public override Response Delete(string objectType, IList<string> identifiers)
    {
      Response response = new Response() { Level = StatusLevel.Success };

      try
      {
        DataDictionary dictionary = GetDictionary();
        DataObject objDef = dictionary.dataObjects.Find(x => x.objectName.ToLower() == objectType.ToLower());

        if (objDef != null)
        {
          try
          {
            Connect();

            EqlClient eqlClient = new EqlClient(_session);
            int objType = (int)_config.Template.ObjectType;

            foreach (string identifier in identifiers)
            {
              Status status = new Status()
              {
                Identifier = identifier,
                Level = StatusLevel.Success
              };

              int objId = eqlClient.GetObjectId(identifier, string.Empty, objType);

              if (objId != 0)
              {
                if (objType == (int)ObjectType.Tag)
                {
                  Tag tag = new Tag(_session, objId);
                  tag.Delete();
                  status.Messages.Add(string.Format("Tag [{0}] deleted succesfully.", identifier));
                }
                else if (objType == (int)ObjectType.Document)
                {
                  Document doc = new Document(_session, objId);
                  doc.Delete();
                  status.Messages.Add(string.Format("Document [{0}] deleted succesfully.", identifier));
                }
                else
                {
                  status.Level = StatusLevel.Error;
                  status.Messages.Add(string.Format("Object type [{0}] not supported.", objType));
                }
              }
              else
              {
                status.Level = StatusLevel.Error;
                status.Messages.Add(string.Format("Object [{0}] not found.", identifier));
              }

              response.Append(status);
            }
          }
          finally
          {
            Disconnect();
          }
        }
        else
        {
          response.Level = StatusLevel.Error;
          response.Messages.Add(string.Format("Object type [{0}] does not exist.", objectType));
        }
      }
      catch (Exception e)
      {
        _logger.Error("Error deleting data object: " + e);

        response.Level = StatusLevel.Error;
        response.Messages.Add(e.Message);
      }

      return response;
    }

    public override Response Delete(string objectType, DataFilter filter)
    {
      try
      {
        IList<string> identifiers = GetIdentifiers(objectType, filter);
        return Delete(objectType, identifiers);
      }
      catch (Exception e)
      {
        string filterXML = Utility.SerializeDataContract<DataFilter>(filter);
        _logger.Error(string.Format("Error deleting object type [{0}] with filter [{1}].", objectType, filterXML));
        throw e;
      }
    }

    public override Response RefreshAll()
    {
      Response response = new Response();

      try
      {
        _dictionary = null;
        System.IO.File.Delete(_dictionaryPath);
        GetDictionary();
        response.Level = StatusLevel.Success;
      }
      catch (Exception e)
      {
        response.Level = StatusLevel.Error;
        response.Messages = new Messages() { e.Message };
      }

      return response;
    }

    protected void Connect()
    {
      _proxy = new Proxy();

      int ret = _proxy.connect(0, _server);

      if (ret < 0)
      {
        throw new Exception(_proxy.get_error(ret));
      }

      ret = _proxy.logon(0, _dataSource, _userName, EncryptionUtility.Decrypt(_password));
      if (ret < 0)
      {
        throw new Exception(_proxy.get_error(ret));
      }

      _proxy.silent_mode = true;
      _session = new eB.Data.Session();
      _session.AttachProtoProxy(_proxy.proto_proxy, _proxy.connect_info);
    }

    protected void Disconnect()
    { 
      if (_proxy != null)
      {
        _proxy.Dispose();
        _proxy = null;
        _session = null;
      }
    }

    protected string GetTemplateName(org.iringtools.adaper.datalayer.eb.config.Template template, DataObject objectDefinition, IDataObject dataObject)
    {
      if ((template.Placeholders == null) || (template.Placeholders.Count() == 0))
      {
        return template.Name;
      }

      template.Placeholders.ToList<Placeholder>().Sort(new PlaceHolderComparer());

      string[] parameters = new string[template.Placeholders.Length];
      int i = 0;

      foreach (Placeholder placeholder in template.Placeholders)
      {
        string propertyName = Utilities.ToPropertyName(placeholder.Value);
        parameters[i++] = Convert.ToString(dataObject.GetPropertyValue(propertyName));
      }

      return string.Format(template.Name, parameters);
    }

    protected IDataObject ToDataObject(DataRow dataRow, DataObject objectDefinition)
    {
      IDataObject dataObject = null;

      if (dataRow != null)
      {
        try
        {
          dataObject = new GenericDataObject() { ObjectType = objectDefinition.objectName };
        }
        catch (Exception e)
        {
          throw e;
        }

        if (dataObject != null && objectDefinition.dataProperties != null)
        {
          foreach (DataProperty prop in objectDefinition.dataProperties)
          {
            try
            {
              string value = string.Empty;

              if (dataRow.Table.Columns.Contains(prop.propertyName))
              {
                value = Convert.ToString(dataRow[prop.propertyName]);
              }

              dataObject.SetPropertyValue(prop.propertyName, value);
            }
            catch (Exception e)
            {
              throw e;
            }
          }
        }
      }

      return dataObject;
    }

    protected IList<IDataObject> ToDataObjects(DataTable dataTable, DataObject objectDefinition)
    {
      IList<IDataObject> dataObjects = new List<IDataObject>();

      if (objectDefinition != null && dataTable.Rows != null)
      {
        foreach (DataRow dataRow in dataTable.Rows)
        {
          IDataObject dataObject = null;

          try
          {
            dataObject = ToDataObject(dataRow, objectDefinition);
          }
          catch (Exception e)
          {
            throw e;
          }

          if (dataObjects != null)
          {
            dataObjects.Add(dataObject);
          }
        }
      }

      return dataObjects;
    }
  }
}
