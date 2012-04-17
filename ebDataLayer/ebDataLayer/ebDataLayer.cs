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

namespace org.iringtools.adapter.datalayer.eb
{
  public class ebDataLayer : BaseDataLayer
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(ebDataLayer));

    private string _appDataPath = string.Empty;
    private string _dataLayerPath = string.Empty;
    private string _scope = string.Empty;
    private string _dictionaryXML = string.Empty;
    
    private GroupTypes _groupTypes = null;
    private string _server = string.Empty;
    private string _dataSource = string.Empty;
    private string _userName = string.Empty;
    private string _password = string.Empty;
    private string _classCodes = string.Empty;

    private int _docId;

    [Inject]
    public ebDataLayer(AdapterSettings settings)
      : base(settings)
    {
      _appDataPath = settings["AppDataPath"];
      _dataLayerPath = settings["DataLayerPath"];
      _scope = _settings["ProjectName"] + "." + _settings["ApplicationName"];

      _dictionaryXML = string.Format("{0}DataDictionary.{1}.xml", _appDataPath, _scope);
      _groupTypes = Utility.Read<GroupTypes>(_dataLayerPath + "GroupTypes.xml", false);

      _server = _settings["ebServer"];
      _dataSource = _settings["ebDataSource"];
      _userName = _settings["ebUserName"];
      _password = _settings["ebPassword"];
      _classCodes = _settings["ebClassCodes"];
      _docId = int.Parse(_settings["ebDocId"]);
    }

    public Proxy Proxy { get; set; }

    public Session Session { get; set; }

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
        string cosResults = Proxy.query(cosQuery.ToString(), ref status);
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

          string attrsQuery = string.Format(ebMetadataQuery, classCode);

          XmlDocument attrsDoc = new XmlDocument();
          string attrsResults = Proxy.query(attrsQuery, ref status);
          attrsDoc.LoadXml(attrsResults);

          foreach (XmlNode attrNode in attrsDoc.DocumentElement.ChildNodes)
          {
            DataProperty dataProp = new DataProperty();
            dataProp.columnName = attrNode.SelectSingleNode("char_name").InnerText;
            dataProp.propertyName = Regex.Replace(dataProp.columnName, @" |\.", "");
            dataProp.dataType = ToCSharpType(attrNode.SelectSingleNode("char_data_type").InnerText);
            dataProp.dataLength = Int32.Parse(attrNode.SelectSingleNode("char_length").InnerText);
            dataProp.isReadOnly = attrNode.SelectSingleNode("readonly").InnerText == "0" ? false : true;
            dataObjectDef.dataProperties.Add(dataProp);
          }

          dataObjectDef.keyDelimeter = ";";

          //if (group.Name.ToLower() == "tag")
          //{
          //  dataObjectDef.keyProperties = new List<KeyProperty>()
          //  {
          //    new KeyProperty() {
          //      keyPropertyName = "Code",
          //    },
          //    new KeyProperty() {
          //      keyPropertyName = "Middle",
          //    },
          //    new KeyProperty() {
          //      keyPropertyName = "Revision",
          //    }
          //  };
          //}
          //else if (group.Name.ToLower() == "document")
          //{
          //  dataObjectDef.keyProperties = new List<KeyProperty>()
          //  {
          //    new KeyProperty() {
          //      keyPropertyName = "PrimaryPhysicalItem.Id",
          //    },
          //    new KeyProperty() {
          //      keyPropertyName = "Code",
          //    }
          //  };
          //}

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
          string classGroup = (dataObjectDef.tableName.ToLower().StartsWith("documents")) ? "Document" : "Tag";
          string query = "START WITH {0} SELECT {1} WHERE Class.Code = '{2}'";
          StringBuilder queryBuilder = new StringBuilder();

          foreach (DataProperty dataProp in dataObjectDef.dataProperties)
          {
            if (queryBuilder.Length > 0)
              queryBuilder.Append(",");

            if (dataProp.isReadOnly)
            {
              queryBuilder.Append(string.Format("Attributes[\"Global\", \"{0}\"].Value \"{1}\"", dataProp.columnName, dataProp.propertyName));
            }
            else
            {
              queryBuilder.Append(dataProp.columnName);
            }
          }

          query = string.Format(query, classGroup, queryBuilder.ToString(), dataObjectDef.objectName);
          DataTable result = ExecuteSearch(Session, query, new object[0], -1);

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
        //string docTpl = GetDocumentTemplate(_docId);
                
        //load mapping
        /* process row data
         * RowProcessor r = new RowProcessor(this.Session, dataSet, importMappingFile);
                    try {
                        string message = r.ProcessObject();
         */
      }
      catch (Exception e)
      {
        _logger.Error("Error posting data objects: " + e);
        
        response.Level = StatusLevel.Error;
        response.Messages.Add("Error posting data objects: " + e);
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
      Proxy = new Proxy();

      int ret = Proxy.connect(0, _server);

      if (ret < 0)
      {
        throw new Exception(Proxy.get_error(ret));
      }

      ret = Proxy.logon(0, _dataSource, _userName, EncryptionUtility.Decrypt(_password));
      if (ret < 0)
      {
        throw new Exception(Proxy.get_error(ret));
      }

      Proxy.silent_mode = true;
      Session = new eB.Data.Session();
      Session.AttachProtoProxy(Proxy.proto_proxy, Proxy.connect_info);
    }

    public void Disconnect()
    {
      if (Proxy != null) Proxy.Dispose();
    }

    protected string GetDocumentTemplate(int id)
    {
      string templateName = string.Empty;

      try
      {
        Connect();

        string eql = String.Format("START WITH Template SELECT Name  WHERE Instances.Object.Id = {0} AND Instances.Object.Type = 3", id);
        Search search = new Search(Session, eql);
        templateName = search.RetrieveScalar<string>("Name");
      }
      catch (Exception e)
      {
        throw e;
      }
      finally
      {
        Disconnect();
      }

      return templateName;
    }
    
    //TODO: retrieve document template and id dictionary
    //protected Dictionary<string, string> DocumentTemplateDictionary()
    //{
    //  string eql = String.Format("START WITH Template SELECT Instances.Object.Id, Name WHERE Instances.Object.Type = 3");
    //  Search search = new Search(Session, eql);
    //  return null;
    //}

    protected int GetTemplateId(string templateName)
    {
      string selectCommandText;
      int templateId = 0;

      if (Proxy.DatabaseType == eB.Common.ConnectionInfo.DatabaseTypes.MicrosoftSQL)
        selectCommandText = string.Format("SELECT template_id FROM templates (NOLOCK) WHERE name = '{0}'", templateName);
      else
        selectCommandText = string.Format("SELECT template_id FROM templates WHERE name = '{0}'", templateName);
      int rc = 0;
      string result = Proxy.query(selectCommandText, ref rc);
      XmlDocument doc = new XmlDocument();
      doc.LoadXml(result);

      if (doc.DocumentElement.ChildNodes.Count <= 0)
        throw new Exception("ConnectionGetTemplateIdNoEntryFoundForTemplateName" + templateName + ";");
      templateId = Convert.ToInt32(doc.SelectSingleNode("records/record/template_id").InnerText);

      return (templateId);
    }

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
        eB.Data.Search s = new Search(Session, eB.Common.Enum.ObjectType.Item, column, criteria);
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
        eB.Data.Search s = new Search(Session, eB.Common.Enum.ObjectType.Document, column, criteria);
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
        eB.Data.Search s = new Search(Session, eB.Common.Enum.ObjectType.AttributeDef, column, criteria);
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

      if (Proxy.DatabaseType == eB.Common.ConnectionInfo.DatabaseTypes.MicrosoftSQL)
        selectCommandText = string.Format("SELECT TOP 1 tag_id FROM tags WHERE item_id = {0} AND code = '{1}' AND revn_name = '{2}'", itemId, code, revnName);
      else
        selectCommandText = string.Format("SELECT tag_id FROM tags WHERE rownum <= 1 AND item_id = {0} AND code = '{1}' AND revn_name = '{2}'", itemId, code, revnName);

      int rc = 0;
      string result = Proxy.query(selectCommandText, ref rc);
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

      eql = String.Format(eql, parameters);
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
        catch (Exception ex)
        {
          throw ex;
        }

        if (dataObject != null && objectDefinition.dataProperties != null)
        {
          foreach (DataProperty objectProperty in objectDefinition.dataProperties)
          {
            try
            {
              if (dataRow.Table.Columns.Contains(objectProperty.propertyName))
              {
                String value = Convert.ToString(dataRow[objectProperty.propertyName]);

                if (value != null)
                {
                  dataObject.SetPropertyValue(objectProperty.propertyName, value);
                }
              }
              else
              {
                //_logger.Warn(String.Format("Value for column [{0}] not found in data row of table [{1}]",
                //  objectProperty.columnName, objectDefinition.tableName));
              }
            }
            catch (Exception ex)
            {
              throw ex;
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
          catch (Exception ex)
          {
            throw ex;
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
