﻿using System;
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
    private string _classCodes = string.Empty;

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
      _classCodes = _settings["ebClassCodes"];

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

        StringBuilder cosQuery = new StringBuilder("select * from class_objects");

        if (!string.IsNullOrEmpty(_classCodes))
        {
          string[] classCodes = _classCodes.Split(',');
          cosQuery.Append(" where ");

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

            cosQuery.Append(string.Format("(group_id = {0} and code = '{1}')", groupType.Value, code));
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
          GroupType group = _groupTypes.Single<org.iringtools.adaper.datalayer.eb.GroupType>(x => x.Value == groupId);
          string tableName = group.Name + "." + classCode;

          if (dictionary.dataObjects.Find(x => x.tableName.ToLower() == tableName.ToLower()) != null)
            continue;

          DataObject dataObjectDef = new DataObject();
          dataObjectDef.objectName = className;
          dataObjectDef.tableName = tableName;

          string ebMetadataQuery = _settings["ebMetadataQuery." + group.Name];

          if (string.IsNullOrEmpty(ebMetadataQuery))
            throw new Exception("No metadata query configured for group [" + group.Name + "]");

          //ebMetadataQuery = "select d.char_name, d.char_data_type, d.char_length, 0 as readonly from class_objects a inner join class_attributes c on c.class_id = a.class_id inner join characteristics d on c.char_id = d.char_id where d.object_type<20";
          //ebMetadataQuery = "select * from class_objects  order by object_type";

          string attrsQuery = string.Format(ebMetadataQuery, classCode);

          XmlDocument attrsDoc = new XmlDocument();
          string attrsResults = _proxy.query(attrsQuery, ref status);
          attrsDoc.LoadXml(attrsResults);

          // --------populate user attributes--------

          string ebUserAttributesQuery = _settings["ebUserAttributesQuery." + group.Name];
          //int objectType = 212;
          int objectType = (int)(Enum.Parse(typeof(ObjectType), group.Name));
          string userAttrsQuery = string.Format(ebUserAttributesQuery, objectType);
          string userAttrsResults = _proxy.query(userAttrsQuery, ref status);
          XDocument userAttributesDoc = new XDocument();
          userAttributesDoc = XDocument.Parse(userAttrsResults);

          //-------------------------------------------

          foreach (XmlNode attrNode in attrsDoc.DocumentElement.ChildNodes)
          {
            DataProperty dataProp = new DataProperty();
            dataProp.columnName = attrNode.SelectSingleNode("char_name").InnerText;
            dataProp.propertyName = Regex.Replace(dataProp.columnName, @" |\.", "");
            dataProp.dataType = ToCSharpType(attrNode.SelectSingleNode("char_data_type").InnerText);
            dataProp.dataLength = Int32.Parse(attrNode.SelectSingleNode("char_length").InnerText);

            //dataProp.isReadOnly = attrNode.SelectSingleNode("readonly").InnerText == "0" ? false : true;
            var userAttributes = (from item in userAttributesDoc.Descendants("record")
                                  where item.Element("char_name").Value == dataProp.columnName
                                  select item.Element("char_name").Value).ToList();

            // system properties are readonly
            if (userAttributes.Count == 0)
            {
                dataProp.isReadOnly = true;
            }
            else
            {
                // user defined  properties are writable
                dataProp.isReadOnly = false;
            }

            dataObjectDef.dataProperties.Add(dataProp);
          }

          dataObjectDef.keyDelimeter = ";";

          if (group.Name.ToLower() == "tag")
          {
            dataObjectDef.keyProperties = new List<KeyProperty>()
            {
              new KeyProperty() {
                keyPropertyName = "Code",
              },
              new KeyProperty() {
                keyPropertyName = "Middle",
              },
              new KeyProperty() {
                keyPropertyName = "Revision",
              }
            };
          }
          else if (group.Name.ToLower() == "document")
          {
            dataObjectDef.keyProperties = new List<KeyProperty>()
            {
              new KeyProperty() {
                keyPropertyName = "PrimaryPhysicalItem.Id",
              },
              new KeyProperty() {
                keyPropertyName = "Code",
              }
            };
          }

          dictionary.dataObjects.Add(dataObjectDef);
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
        DataObject dataObjectDef = dictionary.dataObjects.Find(x => x.objectName.ToLower() == objectType.ToLower());

        if (dataObjectDef != null)
        {
          string classGroup = (dataObjectDef.tableName.ToLower().StartsWith("document")) ? "Document" : "Tag";
          string query = "START WITH {0} SELECT {1} WHERE Class.Code = '{2}'";
          StringBuilder queryBuilder = new StringBuilder();

          foreach (DataProperty dataProp in dataObjectDef.dataProperties)
          {
            if (queryBuilder.Length > 0)
              queryBuilder.Append(",");

            if (dataProp.isReadOnly)
            {
                queryBuilder.Append(dataProp.columnName);
              
            }
            else
            {
                queryBuilder.Append(string.Format("Attributes[\"Global\", \"{0}\"].Value \"{1}\"", dataProp.columnName, dataProp.propertyName));
            }
          }

          query = string.Format(query, classGroup, queryBuilder.ToString(), "SECN");
          DataTable result = ExecuteSearch(_session, query, new object[0], -1);

          dataObjects = ToDataObjects(result, dataObjectDef);
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
      return 10000;
    }

    public override Response Delete(string objectType, DataFilter filter)
    {
      throw new NotImplementedException();
    }

    public override Response Delete(string objectType, IList<string> identifiers)
    {
      throw new NotImplementedException();
    }

    public override IList<IDataObject> Get(string objectType, IList<string> identifiers)
    {
      throw new NotImplementedException();
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
          response.Level = StatusLevel.Warning;
          response.Messages.Add("Data objects are empty.");
          return response;
        }
        
        DataDictionary dictionary = GetDictionary();
        string objType = ((GenericDataObject)dataObjects[0]).ObjectType;
        DataObject objDef = dictionary.dataObjects.Find(x => x.objectName.ToLower() == objType.ToLower());

        Connect();

        foreach (IDataObject dataObject in dataObjects)
        {
          //KeyProperty keyProp = objDef.keyProperties.FirstOrDefault();
          //string keyValue = (string) dataObject.GetPropertyValue(keyProp.keyPropertyName);

          string keyValue = string.Empty;
          try
          {
            Map codeMap = _config.Mappings.Find(x => x.Type == PropertyType.Code);
            keyValue = (string)dataObject.GetPropertyValue(codeMap.Property);
          }
          catch {}

          string revision = string.Empty;
          try
          {
            Map revisionMap = _config.Mappings.Find(x => x.Type == PropertyType.Revision);
            revision = (string)dataObject.GetPropertyValue(revisionMap.Property);
          }
          catch {}

          EqlClient eql = new EqlClient(_session);          
          int objectId = eql.GetObjectId(keyValue, revision, _config.Template.ObjectType);
          org.iringtools.adaper.datalayer.eb.config.Template template = _config.Template;

          if (objectId == 0)  // does not exist, create
          { 
            string templateName = GetTemplateName(template, dataObject);
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
            response.Append(processor.ProcessTag(dataObject, objectId, keyValue));
          }
          else if (objectType == ObjectType.Document.ToString())
          {
            response.Append(processor.ProcessDocument(dataObject, objectId, keyValue));
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

    public void Connect()
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

    public void Disconnect()
    {
      if (_proxy != null) _proxy.Dispose();
    }

    protected string GetTemplateName(org.iringtools.adaper.datalayer.eb.config.Template template, IDataObject dataObject)
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
        parameters[i++] = (string)dataObject.GetPropertyValue(placeholder.Value);
      }

      return string.Format(template.Name, parameters);
    }

    //protected int GetTemplateId(string templateName)
    //{
    //  string selectCommandText;

    //  if (Proxy.DatabaseType == eB.Common.ConnectionInfo.DatabaseTypes.MicrosoftSQL)
    //    selectCommandText = string.Format("SELECT template_id FROM templates (NOLOCK) WHERE name = '{0}'", templateName);
    //  else
    //    selectCommandText = string.Format("SELECT template_id FROM templates WHERE name = '{0}'", templateName);

    //  int rc = 0;
    //  string result = Proxy.query(selectCommandText, ref rc);
    //  XmlDocument doc = new XmlDocument();
    //  doc.LoadXml(result);

    //  if (doc.DocumentElement.ChildNodes.Count <= 0)
    //    throw new Exception("ConnectionGetTemplateIdNoEntryFoundForTemplateName" + templateName + ";");

    //  int templateId = Convert.ToInt32(doc.SelectSingleNode("records/record/template_id").InnerText);
    //  return (templateId);
    //}

    protected int GetItemId(string itemNumber, string version)
    {
      try
      {
        string criteria;
        if (version != null)
          criteria = string.Format("Code = '{0}' AND Version = '{1}'", itemNumber, version);
        else
          criteria = string.Format("Code = '{0}'", itemNumber);

        string column = "Id";
        eB.Data.Search s = new Search(_session, eB.Common.Enum.ObjectType.Item, column, criteria);
        return (s.RetrieveScalar<int>(column));
      }
      catch (Exception)
      {
        return (0);
      }
    }

    protected int GetDocId(string docNumber, string revision)
    {
      try
      {
        string criteria;
        if (revision != null)
          criteria = string.Format("Code = '{0}' AND Revision = '{1}'", docNumber, revision);
        else  //Code LIKE '%' AND IsLatestRevision = 'Y'
          criteria = string.Format("Code = '{0}' AND IsLatestRevision = 'Y'", docNumber);

        string column = "Id";
        eB.Data.Search s = new Search(_session, eB.Common.Enum.ObjectType.Document, column, criteria);
        return (s.RetrieveScalar<int>(column));
      }
      catch (Exception)
      {
        return (0);
      }
    }

    protected int GetCharId(string charName)
    {
      try
      {
        string criteria = string.Format("Name = '{0}'", charName);
        string column = "Id";
        eB.Data.Search s = new Search(_session, eB.Common.Enum.ObjectType.AttributeDef, column, criteria);
        return (s.RetrieveScalar<int>(column));
      }
      catch (Exception)
      {
        return (0);
      }
    }

    protected int GetTagId(string itemCode, string itemVersion, string code, string revnName)
    {
      string selectCommandText;
      int tagId = 0;

      int itemId = GetItemId(itemCode, itemVersion);

      if (_proxy.DatabaseType == eB.Common.ConnectionInfo.DatabaseTypes.MicrosoftSQL)
        selectCommandText = string.Format("SELECT TOP 1 tag_id FROM tags WHERE item_id = {0} AND code = '{1}' AND revn_name = '{2}'", itemId, code, revnName);
      else
        selectCommandText = string.Format("SELECT tag_id FROM tags WHERE rownum <= 1 AND item_id = {0} AND code = '{1}' AND revn_name = '{2}'", itemId, code, revnName);

      int rc = 0;
      string result = _proxy.query(selectCommandText, ref rc);
      XmlDocument doc = new XmlDocument();
      doc.LoadXml(result);

      if (doc.DocumentElement.ChildNodes.Count <= 0)
        throw new Exception("There are no tags that match this code: " + code);

      tagId = Convert.ToInt32(doc.SelectSingleNode("records/record/tag_id").InnerText);

      return (tagId);
    }

    protected DataTable ExecuteSearch(Session session, string eql, object[] parameters, int pageSize = -1)
    {
      parameters = parameters.Select(p =>
      {
        if (p.GetType() == typeof(string))
        {
          return (p as string).Replace("'", "''");
        }
        else
          if (p.GetType().IsEnum)
          {
            return (int)p;
          }
          else
          {
            return p;
          }
      }).ToArray();

      eql = string.Format(eql, parameters);
      return new Search(session, new eB.ContentData.Eql.Search(eql)).Retrieve<DataTable>(1, pageSize);
    }

    protected DataType ToCSharpType(string type)
    {
      // convert ebType to known type
      switch (type)
      {
        case "CH":
        case "PD":
          type = "String";
          break;
        case "NU":
          type = "Decimal";
          break;
        case "DA":
          type = "DateTime";
          break;
      }

      return (DataType)Enum.Parse(typeof(DataType), type, true);
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
