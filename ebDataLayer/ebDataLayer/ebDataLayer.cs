using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using eB.Common.Enum;
using eB.Data;
using eB.Service.Client;
using log4net;
using Ninject;
using org.iringtools.adaper.datalayer.eb;
using org.iringtools.library;
using org.iringtools.utility;
using StaticDust.Configuration;

namespace org.iringtools.adapter.datalayer.eb
{
  public class ebDataLayer : BaseDataLayer
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(ebDataLayer));

    private readonly string TAG_METADATA_SQL = @"
      SELECT d.char_name, d.char_data_type, d.char_length, 0 AS is_system_char FROM class_objects a 
      INNER JOIN class_attributes c ON c.class_id = a.class_id 
      INNER JOIN characteristics d ON c.char_id = d.char_id 
      WHERE d.object_type = {0} 
      UNION SELECT 'Id', 'Int32', 255 , 1 
      UNION SELECT 'Class.Code', 'String', 255, 1 
      UNION SELECT 'Class.Id', 'Int32', 255, 1 
      UNION SELECT 'Code', 'String', 100, 1 
      UNION SELECT 'Revision', 'String', 100, 1 
      UNION SELECT 'Name', 'String', 255, 1 
      UNION SELECT 'ChangeControlled', 'String', 1, 1 
      UNION SELECT 'DateEffective', 'DateTime', 100, 1 
      UNION SELECT 'DateObsolete', 'DateTime', 100, 1 
      UNION SELECT 'ApprovalStatus', 'String', 1, 1 
      UNION SELECT 'OperationalStatus', 'String', 1, 1 
      UNION SELECT 'Quantity', 'Int32', 8, 1 
      UNION SELECT 'Description', 'String', 1000, 1";

    private readonly string DOCUMENT_METADATA_SQL = @"
      SELECT d.char_name, d.char_data_type, d.char_length, 0 AS is_system_char FROM class_objects a 
      INNER JOIN class_attributes c ON c.class_id = a.class_id 
      INNER JOIN characteristics d ON c.char_id = d.char_id 
      WHERE d.object_type = {0} 
      UNION SELECT 'Id', 'Int32', 255, 1 
      UNION SELECT 'Class.Code', 'String', 255, 1 
      UNION SELECT 'Class.Id', 'Int32', 255, 1 
      UNION SELECT 'Code', 'String', 100, 1 
      UNION SELECT 'Revision', 'String', 100, 1 
      UNION SELECT 'Name', 'String', 255, 1 
      UNION SELECT 'ChangeControlled', 'String', 1, 1 
      UNION SELECT 'DateEffective', 'DateTime', 100, 1 
      UNION SELECT 'DateObsolete', 'DateTime', 100, 1 
      UNION SELECT 'ApprovalStatus', 'String', 1, 1 
      UNION SELECT 'Remark', 'String', 255, 1 
      UNION SELECT 'Synopsis', 'String', 255, 1";

    private readonly string CLASS_OBJECTS_EQL = @"
      START WITH Class SELECT ClassGroup.Id GroupId, ClassGroup.ObjectType ObjectType, Path
      WHERE ClassGroup.Id IN (1,17) AND Path NOT LIKE '%\%' ORDER BY ClassGroup.Id, Path";

    private readonly string M3_OBJECTS_SQL = @"
      SELECT document_id, hash_key FROM (
        SELECT MAX(a.pdm_file_id) OVER (PARTITION BY a.document_id) as max_file_id, 
        a.pdm_file_id, c.document_id, b.hash_key FROM doc_source a 
        INNER JOIN m3_objects b ON a.m3_object_id = b.object_id
        INNER JOIN documents c ON a.document_id = c.document_id
        WHERE a.pdm_file_id IS NOT NULL AND a.document_id IN ({0}) 
      ) t WHERE pdm_file_id = max_file_id";

    private readonly string CONTENT_EQL = @"
      START WITH Document
      SELECT Id, Code, Files.Id, Files.Name
      WHERE Id IN ({0}) AND Files.Id IS NOT NULL AND Files.Name NOT LIKE '%(eB Historic%'";

    private string _dataPath = string.Empty;
    private string _scope = string.Empty;
    private string _dictionaryPath = string.Empty;
    private DataDictionary _dictionary = null;

    private string _server = string.Empty;
    private string _dataSource = string.Empty;
    private string _userName = string.Empty;
    private string _password = string.Empty;
    private string _communityName = string.Empty;
    private string _classObjects = string.Empty;
    private string _keyDelimiter = string.Empty;

    private Proxy _proxy = null;
    private Session _session = null;
    private Dictionary<string, Configuration> _configs = null;
    private Rules _rules = null;

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
        _classObjects = _settings["ebClassObjects"];

        _keyDelimiter = _settings["ebKeyDelimiter"];
        if (_keyDelimiter == null)
        {
          _keyDelimiter = ";";
        }

        _communityName = _settings["ebCommunityName"];
        string[] configFiles = Directory.GetFiles(_dataPath, "*" + _communityName + ".xml");
        string ruleFile = _dataPath + "Rules_" + _communityName + ".xml";

        //
        // Load configuration files
        //
        _configs = new Dictionary<string, Configuration>(StringComparer.OrdinalIgnoreCase);      
  
        foreach (string configFile in configFiles)
        {
          if (configFile.ToLower() != ruleFile.ToLower())
          {
            string fileName = Path.GetFileName(configFile);
            Configuration config = Utility.Read<Configuration>(configFile, false);
            _configs[fileName] = config;
          }
        }

        // Load rule file
        _rules = Utility.Read<Rules>(ruleFile, false);
      }
      catch (Exception e)
      {
        _logger.Error("Error initializing ebDataLayer: " + e.Message);
      }
    }

    #region interface implementation methods
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

        EqlClient eqlClient = new EqlClient(_session);
        List<ClassObject> classObjects = GetClassObjects(eqlClient);

        _dictionary = new DataDictionary();
        foreach (ClassObject classObject in classObjects)
        {
          DataObject objDef = CreateObjectDefinition(classObject);

          if (objDef != null)
          {
            _dictionary.dataObjects.Add(objDef);
          }
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
        DataObject objDef = GetObjectDefinition(objectType);

        if (objDef != null)
        {
          Connect();

          Configuration config = GetConfiguration(objDef);
          int objType = (int)config.Template.ObjectType;
          string classIds = objDef.tableName.Replace("_", ",");
          string eql = string.Empty;

          if (objType == (int)ObjectType.Tag)
          {
            eql = string.Format("START WITH Tag WHERE Class.Id IN ({0})", classIds);
          }
          else if (objType == (int)ObjectType.Document)
          {
            eql = string.Format("START WITH Document WHERE Class.Id IN ({0})", classIds);
          }
          else
          {
            throw new Exception(string.Format("Object type [{0}] not supported.", objectType));
          }

          if (filter != null)
          {
            string whereClause = Utilities.ToSqlWhereClause(filter, objDef);
            if (!string.IsNullOrEmpty(whereClause))
            {
              eql += whereClause.Replace(" WHERE ", " AND ");
            }
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

        DataObject objDef = GetObjectDefinition(objectType);

        if (objDef != null)
        {
          string classObject = objDef.objectNamespace;
          string classIds = objDef.tableName.Replace("_", ",");

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

            eql = string.Format(eql, classObject, builder.ToString(), classIds);

            if (filter != null)
            {
              string whereClause = Utilities.ToSqlWhereClause(filter, objDef);
              if (!string.IsNullOrEmpty(whereClause))
              {
                eql += whereClause.Replace(" WHERE ", " AND ");
              }
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
        DataObject objDef = GetObjectDefinition(objectType);
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
      return Get(objectType, identifiers, false);
    }

    public IList<IDataObject> Get(string objectType, IList<string> identifiers, bool retainSession)
    {
      IList<IDataObject> dataObjects = new List<IDataObject>();

      try
      {
        Connect();

        DataObject objDef = GetObjectDefinition(objectType);

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
        if (!retainSession) Disconnect();
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

        string objType = ((GenericDataObject)dataObjects[0]).ObjectType;
        DataObject objDef = GetObjectDefinition(objType);
        Configuration config = GetConfiguration(objDef);

        Connect();

        foreach (IDataObject dataObject in dataObjects)
        {
          KeyProperty keyProp = objDef.keyProperties.FirstOrDefault();
          string keyValue = Convert.ToString(dataObject.GetPropertyValue(keyProp.keyPropertyName));

          string revision = string.Empty;
          Map revisionMap = config.Mappings.ToList<Map>().Find(x => x.Destination == (int)Destination.Revision);
          if (revisionMap != null)
          {
            string propertyName = Utilities.ToPropertyName(revisionMap.Column);
            revision = Convert.ToString(dataObject.GetPropertyValue(propertyName));
          }

          EqlClient eql = new EqlClient(_session);
          int objectId = eql.GetObjectId(keyValue, revision, config.Template.ObjectType);
          org.iringtools.adaper.datalayer.eb.Template template = config.Template;

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
                Messages = new Messages() { string.Format("Template [{0}] does not exist.", templateName) }
              };

              response.StatusList.Add(status);
              continue;
            }

            objectId = _session.Writer.CreateFromTemplate(templateId, "", "");
          }

          string objectType = Enum.GetName(typeof(ObjectType), template.ObjectType);
          ebProcessor processor = new ebProcessor(_session, config.Mappings.ToList<Map>(), _rules);

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
              Messages = new Messages() { string.Format("Object type [{0}] not supported.", template.ObjectType) }
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
        DataObject objDef = GetObjectDefinition(objectType);
        
        if (objDef != null)
        {
          try
          {
            Connect();

            EqlClient eqlClient = new EqlClient(_session);
            Configuration config = GetConfiguration(objDef);
            int objType = (int)config.Template.ObjectType;

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

    public override Response Refresh(string objectType)
    {
      return RefreshAll();
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

    //TODO: add to interface and override
    public IDictionary<string, string> GetHashValues(string objectType, IList<string> identifiers)
    {
      try
      {
        IList<IDataObject> dataObjects = Get(objectType, identifiers);
        IList<int> docIds = GetDocumentIds(dataObjects);
        return GetHashValues(docIds);
      }
      catch (Exception e)
      {
        _logger.Error("Error getting hash values: " + e.Message);
        throw e;
      }
    }

    //TODO: add to interface and override
    public IDictionary<string, string> GetHashValues(string objectType, DataFilter filter, int pageSize, int startIndex)
    {
      try
      {
        IList<IDataObject> dataObjects = Get(objectType, filter, pageSize, startIndex);
        IList<int> docIds = GetDocumentIds(dataObjects);
        return GetHashValues(docIds);
      }
      catch (Exception e)
      {
        _logger.Error("Error getting hash values: " + e.Message);
        throw e;
      }
    }

    //TODO: add to interface and override
    public IList<IContentObject> GetContents(string objectType, IList<string> identifiers)
    {
      try
      {
        IList<IDataObject> dataObjects = Get(objectType, identifiers);
        IList<int> docIds = GetDocumentIds(dataObjects);
        return GetContents(objectType, docIds);
      }
      catch (Exception e)
      {
        _logger.Error("Error getting contents: " + e.Message);
        throw e;
      }
    }

    //TODO: add to interface and override
    public IList<IContentObject> GetContents(string objectType, DataFilter filter, int pageSize, int startIndex)
    {
      try
      {
        IList<IDataObject> dataObjects = Get(objectType, filter, pageSize, startIndex);
        IList<int> docIds = GetDocumentIds(dataObjects);
        return GetContents(objectType, docIds);
      }
      catch (Exception e)
      {
        _logger.Error("Error getting contents: " + e.Message);
        throw e;
      }
    }

    //TODO: add to interface and override
    public Response PostContents(IList<IContentObject> contentObjects)
    {
      Response response = new Response() { Level = StatusLevel.Success };

      if (contentObjects == null || contentObjects.Count == 0)
      {
        response.Level = StatusLevel.Error;
        response.Messages.Add("No content object to post.");
        return response;
      }

      foreach (GenericContentObject contentObject in contentObjects)
      {
        try
        {
          //
          // get doc Id
          //
          IList<IDataObject> dataObjects = Get(contentObject.ObjectType, new List<string> { contentObject.identifier }, true);
          int docId = GetDocumentId(dataObjects.FirstOrDefault());

          if (docId <= 0)
          {
            _session.ProtoProxy.AddObjectFile(_session.ReaderSessionString, docId, 
              (int)ObjectType.Document, contentObject.content, contentObject.identifier, 0);

            Status status = new Status()
            {
              Identifier = contentObject.identifier,
              Level = StatusLevel.Error,
              Messages = new Messages() { string.Format("Document [{0}] not found.", contentObject.identifier) }
            };

            response.StatusList.Add(status);
            continue;
          }
          
          // 
          // get content id
          //
          EqlClient client = new EqlClient(_session);
          DataTable dt = client.RunQuery(string.Format(CONTENT_EQL, docId));
          int fileId = (int)(dt.Rows[0]["FilesId"]);

          if (fileId <= 0)  // add
          {
            _session.ProtoProxy.AddObjectFile(_session.ReaderSessionString, docId,
              (int)ObjectType.Document, contentObject.content, contentObject.identifier, 0);

            Status status = new Status()
            {
              Identifier = contentObject.identifier,
              Level = StatusLevel.Success,
              Messages = new Messages() { string.Format("Document [{0}] added successfully.", contentObject.identifier) }
            };

            response.StatusList.Add(status);
          }
          else  // update
          {
            //
            // check out content
            //
            eB.Data.File file = new eB.Data.File(_session);
            file.Retrieve(fileId, "Header;Repositories");

            FileInfo localFile = new FileInfo(Path.GetTempPath() + file.Name);
            if (localFile.Exists)
              localFile.Delete();

            file.CheckOut(localFile.FullName);

            //
            // update content
            //
            Utility.WriteStream(contentObject.content, localFile.FullName);

            // 
            // check content back in
            //
            file = new eB.Data.File(_session);
            file.Retrieve(fileId, "Header;Repositories");

            if (file.IsCheckedOut)
            {
              try
              {
                file.CheckIn(localFile.FullName, eB.ContentData.File.CheckinOptions.DeleteLocalCopy, null);
                _session.Writer.CheckinDoc(file.Document.Id, 0);

                Status status = new Status()
                {
                  Identifier = contentObject.identifier,
                  Level = StatusLevel.Success,
                  Messages = new Messages() { string.Format("Document [{0}] updated successfully.", contentObject.identifier) }
                };

                response.StatusList.Add(status);
              }
              catch
              {
                file.UndoCheckout();
              }
            }
          }
        }
        catch (Exception e)
        {
          _logger.Error("Error posting content: " + e.Message);

          Status status = new Status()
          {
            Identifier = contentObject.identifier,
            Level = StatusLevel.Error,
            Messages = new Messages() { e.Message }
          };

          response.StatusList.Add(status);
        }
      }

      return response;
    }
    #endregion

    #region helper methods
    protected DataObject CreateObjectDefinition(ClassObject classObject)
    {
      if (classObject.Ids == null || classObject.Ids.Count == 0)
      {
        return null;
      }

      string metadataQuery = string.Empty;

      if (classObject.ObjectType == ObjectType.Tag)
      {
        metadataQuery = string.Format(TAG_METADATA_SQL, (int)ObjectType.Tag);
      }
      else if (classObject.ObjectType == ObjectType.Document)
      {
        metadataQuery = string.Format(DOCUMENT_METADATA_SQL, (int)ObjectType.Document);
      }
      else
      {
        throw new Exception(string.Format("Object type [{0}] not supported.", classObject.ObjectType));
      }

      int status = 0;
      string result = _proxy.query(metadataQuery, ref status);
      XmlDocument resultXml = new XmlDocument();
      resultXml.LoadXml(result);

      string type = Enum.GetName(typeof(ObjectType), classObject.ObjectType);
      DataObject objDef = new DataObject();
      objDef.objectNamespace = type;
      objDef.objectName = classObject.Name + "(" + type + ")";
      objDef.tableName = string.Join("_", classObject.Ids.ToArray());
      objDef.keyDelimeter = _keyDelimiter;

      Configuration config = GetConfiguration(objDef);
      if (config == null)
        return null;

      Map codeMap = config.Mappings.ToList<Map>().Find(x => x.Destination == (int)Destination.Code);
      if (codeMap == null)
      {
        throw new Exception("No mapping configured for key property.");
      }

      objDef.keyProperties = new List<KeyProperty>()
      {
        new KeyProperty() { keyPropertyName = codeMap.Column }
      };

      foreach (XmlNode attrNode in resultXml.DocumentElement.ChildNodes)
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
      foreach (Map m in config.Mappings.Where(x => x.Destination == (int)Destination.Relationship).Select(m => m))
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
      foreach (Map m in config.Mappings.Where(x => x.Destination != (int)Destination.Relationship &&
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

      return objDef;
    }

    protected DataObject GetObjectDefinition(string objectType)
    {
      DataDictionary dictionary = GetDictionary();
      DataObject objDef = dictionary.dataObjects.Find(x => x.objectName.ToLower() == objectType.ToLower());
      return objDef;
    }

    protected Configuration GetConfiguration(DataObject objDef)
    {
      string fileName = objDef.objectNamespace + "_" + 
        Regex.Replace(objDef.objectName, @"\(.*\)", string.Empty) + "_" + _communityName + ".xml";

      // use specific config for object type if available
      if (_configs.ContainsKey(fileName))
        return _configs[fileName];

      // specific config does not exist, look for higher scope configuration
      fileName = objDef.objectNamespace + "_" + _communityName + ".xml";
      if (_configs.ContainsKey(fileName))
        return _configs[fileName];

      _logger.Error(string.Format("No configuration available for object type [{0}].", objDef.objectName));        
      return null;
    }

    protected void Connect()
    {
      if (_proxy == null)
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

    protected List<ClassObject> GetClassObjects(EqlClient eqlClient)
    {
      List<ClassObject> classObjects = new List<ClassObject>();

      if (string.IsNullOrEmpty(_classObjects))
      {
        DataTable dt = eqlClient.RunQuery(CLASS_OBJECTS_EQL);

        foreach (DataRow row in dt.Rows)
        {
          int groupId = (int)row["GroupId"];
          string groupName = Enum.GetName(typeof(GroupType), groupId);
          string path = row["Path"].ToString();

          ClassObject classObject = new ClassObject()
          {
            Name = path,
            ObjectType = (ObjectType)(row["ObjectType"]),
            GroupId = groupId,
            Ids = eqlClient.GetClassIds(groupId, path)
          };

          classObjects.Add(classObject);
        }
      }
      else
      {
        string[] cosParts = _classObjects.Split(',');

        for (int i = 0; i < cosParts.Length; i++)
        {
          string[] coParts = cosParts[i].Trim().Split('.');
          string groupName = coParts[0];
          string className = coParts[1];

          ClassObject classObject = new ClassObject()
          {
            Name = className,
            ObjectType = (ObjectType)Enum.Parse(typeof(ObjectType), groupName),
            GroupId = (int)Enum.Parse(typeof(GroupType), groupName),
            Ids = eqlClient.GetClassIds((int)Enum.Parse(typeof(GroupType), groupName), className)
          };

          classObjects.Add(classObject);
        }
      }

      return classObjects;
    }

    protected string GetTemplateName(org.iringtools.adaper.datalayer.eb.Template template, DataObject objectDefinition, IDataObject dataObject)
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

    protected int GetDocumentId(IDataObject dataObject)
    {
      return Convert.ToInt32(dataObject.GetPropertyValue("Id"));
    }

    protected IList<int> GetDocumentIds(IList<IDataObject> dataObjects)
    {
      IList<int> docIds = new List<int>();

      foreach (IDataObject dataObject in dataObjects)
      {
        docIds.Add(GetDocumentId(dataObject));
      }

      return docIds;
    }

    protected IDictionary<string, string> GetHashValues(IList<int> docIds)
    {
      IDictionary<string, string> hashValues = new Dictionary<string, string>();

      try
      {
        Connect();

        int status = 0;
        string query = string.Format(M3_OBJECTS_SQL, string.Join(",", docIds.ToArray()));
        string result = _proxy.query(query, ref status);
        XDocument resultDoc = XDocument.Parse(result);

        foreach (XElement elt in resultDoc.Element("records").Elements("record"))
        {
          string docId = elt.Element("document_id").Value;
          string hashValue = elt.Element("hash_key").Value;
          hashValues[docId] = hashValue;
        }
      }
      catch (Exception e)
      {
        _logger.Error("Error getting hash values: " + e.Message);
        throw e;
      }
      finally
      {
        Disconnect();
      }

      return hashValues;
    }

    public IList<IContentObject> GetContents(String objectType, IList<int> docIds)
    {
      IList<IContentObject> contents = new List<IContentObject>();

      try
      {
        Connect();

        string query = string.Format(CONTENT_EQL, string.Join(",", docIds.ToArray()));
        EqlClient client = new EqlClient(_session);
        DataTable dt = client.RunQuery(query);

        foreach (DataRow row in dt.Rows)
        {
          int fileId = (int)row["FilesId"];
          string code = (string)row["Code"];

          string name = ((string)row["FilesName"]);
          string type = name.Substring(name.LastIndexOf(".") + 1);

          eB.Data.File f = new eB.Data.File(_session);
          f.Retrieve(fileId, "Header; Repositories");
          MemoryStream stream = new MemoryStream();
          f.ContentData = new eB.ContentData.File(f, stream);
          f.ContentData.ReadAllBytes();
          stream.Position = 0;

          GenericContentObject content = new GenericContentObject()
          {
            ObjectType = objectType,
            identifier = code,
            content = stream,
            contentType = type.ToUpper(),
            name = name
          };

          contents.Add(content);
        }
      }
      catch (Exception e)
      {
        _logger.Error("Error getting contents: " + e.Message);
        throw e;
      }
      finally
      {
        Disconnect();
      }

      return contents;
    }
    #endregion    
  }
}
