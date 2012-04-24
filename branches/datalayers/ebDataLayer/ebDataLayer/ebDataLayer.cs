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

namespace org.iringtools.adapter.datalayer.eb
{
  public class ebDataLayer : BaseDataLayer
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(ebDataLayer));

    private string _dataPath = string.Empty;
    private string _scope = string.Empty;
    private string _dictionaryXML = string.Empty;

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
      _dataPath = (settings["DataLayerPath"] == null) ? settings["AppDataPath"] : settings["DataLayerPath"] + "App_Data\\";
      _scope = _settings["ProjectName"] + "." + _settings["ApplicationName"];
      _dictionaryXML = string.Format("{0}DataDictionary.{1}.xml", _dataPath, _scope);

      _server = _settings["ebServer"];
      _dataSource = _settings["ebDataSource"];
      _userName = _settings["ebUserName"];
      _password = _settings["ebPassword"];
      _filteredClasses = _settings["ebFilteredClasses"];
      _keyDelimiter = _settings["ebKeyDelimiter"];

      try
      {
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
        _logger.Error("Error initializing eb data layer: " + e.ToString());
      }
      finally
      {
        Disconnect();
      }
    }

    public override DataDictionary GetDictionary()
    {
      DataDictionary dictionary = new DataDictionary();

      if (System.IO.File.Exists(_dictionaryXML))
        return Utility.Read<DataDictionary>(_dictionaryXML);

      try
      {
        Connect();

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
            throw new Exception("Group type not supported: " + e.Message);
          }

          EqlClient eqlClient = new EqlClient(_session);
          List<string> subClassCodes = eqlClient.GetSubClassCodes(className);

          DataObject objDef = new DataObject();
          objDef.objectNamespace = group.Name;
          objDef.objectName = className + "(" + group.Name + ")";
          objDef.tableName = (subClassCodes.Count == 0)
            ? className : string.Join(",", subClassCodes.ToArray());
          objDef.keyDelimeter = _keyDelimiter;

          Map codeMap = _config.Mappings.Find(x => x.Destination == (int)Destination.Code);
          if (codeMap == null)
          {
            throw new Exception("No mapping configured for key property.");
          }

          objDef.keyProperties = new List<KeyProperty>()
          {
            new KeyProperty() { keyPropertyName = codeMap.Column }
          };

          string ebMetadataQuery = _settings["ebMetadataQuery." + group.Name];

          if (string.IsNullOrEmpty(ebMetadataQuery))
            throw new Exception("No metadata query configured for group [" + group.Name + "]");

          int objectType = (int)Enum.Parse(typeof(ObjectType), group.Name);
          string attrsQuery = string.Format(ebMetadataQuery, objectType);

          XmlDocument attrsDoc = new XmlDocument();
          string attrsResults = _proxy.query(attrsQuery, ref status);
          attrsDoc.LoadXml(attrsResults);

          foreach (XmlNode attrNode in attrsDoc.DocumentElement.ChildNodes)
          {
            DataProperty dataProp = new DataProperty();
            dataProp.columnName = attrNode.SelectSingleNode("char_name").InnerText;
            dataProp.propertyName = Utilities.ToPropertyName(dataProp.columnName);
            dataProp.dataType = Utilities.ToCSharpType(attrNode.SelectSingleNode("char_data_type").InnerText);
            dataProp.dataLength = Int32.Parse(attrNode.SelectSingleNode("char_length").InnerText);
            dataProp.isReadOnly = attrNode.SelectSingleNode("readonly").InnerText == "0" ? false : true;

            objDef.dataProperties.Add(dataProp);
          }

          // add related properties
          foreach (Map m in _config.Mappings.Where(x => x.Destination == (int)Destination.Relationship).Select(m => m))
          {
            DataProperty dataProp = new DataProperty();
            dataProp.columnName = m.Column + Utilities.RELATED_COLUMN_TOKEN;
            dataProp.propertyName = Utilities.ToPropertyName(m.Column);
            dataProp.dataType = DataType.String;
            dataProp.isReadOnly = false;
            dataProp.isHidden = true;

            objDef.dataProperties.Add(dataProp);
          }

          dictionary.dataObjects.Add(objDef);
        }

        Utility.Write<DataDictionary>(dictionary, _dictionaryXML);
      }
      catch (Exception e)
      {
        throw e;
      }
      finally
      {
        Disconnect();
      }

      return dictionary;
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
            string eql = "START WITH {0} SELECT {1} WHERE Class.Code IN ({2})";
            StringBuilder attrsBuilder = new StringBuilder();

            foreach (DataProperty dataProp in objDef.dataProperties)
            {
              if (dataProp.isReadOnly)
              {
                if (attrsBuilder.Length > 0)
                  attrsBuilder.Append(",");

                attrsBuilder.Append(dataProp.columnName);
              }
              else if (!dataProp.columnName.EndsWith(Utilities.RELATED_COLUMN_TOKEN))
              {
                if (attrsBuilder.Length > 0)
                  attrsBuilder.Append(",");

                attrsBuilder.Append(string.Format("Attributes[\"Global\", \"{0}\"].Value \"{1}\"", dataProp.columnName, dataProp.propertyName));
              }
            }

            eql = string.Format(eql, classObject, attrsBuilder.ToString(), classCodes);

            string whereClause = Utilities.ToSqlWhereClause(filter, objDef);
            if (!string.IsNullOrEmpty(whereClause))
            {
              eql += whereClause.Replace(" WHERE ", " AND ");
            }

            EqlClient eqlClient = new EqlClient(_session);
            DataTable result = eqlClient.SearchPage(_session, eql, new object[0], startIndex, pageSize);

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
          string classCodes = "'" + string.Join("','", objDef.tableName.Split(',')) + "'";
          string eql = string.Empty;

          if (objType == (int)ObjectType.Tag)
          {
            eql = string.Format("START WITH Tag WHERE Class.Code IN ({0}) AND Code NOT IN ({0})", classCodes);
          }
          else if (objType == (int)ObjectType.Document)
          {
            eql = string.Format("START WITH Document WHERE Class.Code IN ({0}) AND Code NOT IN ({0})", classCodes);
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
    public override Response Delete(string objectType, DataFilter filter)
    {
      throw new NotImplementedException();
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
                  response.Messages.Add(string.Format("Tag [{0}] deleted succesfully.", identifier));
                }
                else if (objType == (int)ObjectType.Document)
                {
                  Document doc = new Document(_session, objId);
                  doc.Delete();
                  response.Messages.Add(string.Format("Document [{0}] deleted succesfully.", identifier));
                }
                else
                {
                  response.Level = StatusLevel.Error;
                  response.Messages.Add(string.Format("Object type [{0}] not supported.", objType));
                }
              }
              else
              {
                response.Level = StatusLevel.Error;
                response.Messages.Add(string.Format("Object [{0}] not found.", identifier));
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
            string query = "START WITH {0} SELECT {1} WHERE {2} IN {3}";
            StringBuilder attrsBuilder = new StringBuilder();

            foreach (DataProperty dataProp in objDef.dataProperties)
            {
              if (dataProp.isReadOnly)
              {
                if (attrsBuilder.Length > 0)
                  attrsBuilder.Append(",");

                attrsBuilder.Append(dataProp.columnName);
              }
              else if (!dataProp.columnName.EndsWith(Utilities.RELATED_COLUMN_TOKEN))
              {
                if (attrsBuilder.Length > 0)
                  attrsBuilder.Append(",");

                attrsBuilder.Append(string.Format("Attributes[\"Global\", \"{0}\"].Value \"{1}\"", dataProp.columnName, dataProp.propertyName));
              }
            }

            query = string.Format(query, classObject, attrsBuilder.ToString(), key, keyValues);

            EqlClient eqlClient = new EqlClient(_session);
            DataTable result = eqlClient.SearchPage(_session, query, new object[0], 0, -1);

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

    public override IList<string> GetIdentifiers(string objectType, DataFilter filter)
    {
      throw new NotImplementedException();
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
          string keyValue = (string)dataObject.GetPropertyValue(keyProp.keyPropertyName);

          string revision = string.Empty;
          Map revisionMap = _config.Mappings.Find(x => x.Destination == (int)Destination.Revision);
          if (revisionMap != null)
          {
            string propertyName = Utilities.ToPropertyName(revisionMap.Column);
            revision = (string)dataObject.GetPropertyValue(propertyName);
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
          ebProcessor processor = new ebProcessor(_session, _config.Mappings, _rules);

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

    public override Response RefreshAll()
    {
      Response response = new Response();

      try
      {
        System.IO.File.Delete(_dictionaryXML);
        GetDictionary();
        response.Level = StatusLevel.Success;
      }
      catch (Exception e)
      {
        response.Level = StatusLevel.Error;
        response.Messages = new Messages() { e.ToString() };
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

      template.Placeholders.Sort(new PlaceHolderComparer());

      string[] parameters = new string[template.Placeholders.Count];
      int i = 0;

      foreach (Placeholder placeholder in template.Placeholders)
      {
        string propertyName = Utilities.ToPropertyName(placeholder.Value);
        parameters[i++] = (string)dataObject.GetPropertyValue(propertyName);
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
          foreach (DataProperty objectProperty in objectDefinition.dataProperties)
          {
            try
            {
              if (dataRow.Table.Columns.Contains(objectProperty.propertyName))
              {
                string value = Convert.ToString(dataRow[objectProperty.propertyName]);

                if (value != null)
                {
                  dataObject.SetPropertyValue(objectProperty.propertyName, value);
                }
              }
              else
              {
                _logger.Warn(string.Format("Value for column [{0}] not found in data row of table [{1}]",
                  objectProperty.columnName, objectDefinition.tableName));
              }
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
