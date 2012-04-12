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

namespace org.iringtools.adapter.datalayer
{
  public class ebDataLayer : BaseDataLayer
  {
    private string _appDataPath = string.Empty;
    private string _scope = string.Empty;
    private string _dictionaryXML = string.Empty;  

    private string _server = string.Empty;
    private string _dataSource = string.Empty;
    private string _userName = string.Empty;
    private string _password = string.Empty;
    private string _groups = string.Empty;
    private string _classCodes = string.Empty; 

    [Inject]
    public ebDataLayer(AdapterSettings settings)
      : base(settings)
    {
      _appDataPath = settings["AppDataPath"];
      _scope = _settings["ProjectName"] + "." + _settings["ApplicationName"];
      _dictionaryXML = string.Format("{0}DataDictionary.{1}.xml", _appDataPath, _scope);

      _server = _settings["ebServer"];
      _dataSource = _settings["ebDataSource"];
      _userName = _settings["ebUserName"];
      _password = _settings["ebPassword"];
      _classCodes = _settings["ebClassCodes"];
    }

    public override DataDictionary GetDictionary()
    {
      DataDictionary dictionary = new DataDictionary();

      if (System.IO.File.Exists(_dictionaryXML))
        return Utility.Read<DataDictionary>(_dictionaryXML);

      try
      {
        Connect();

        string cosQuery = "select * from class_objects";
        if (!string.IsNullOrEmpty(_classCodes))
        {
          string[] codes = _classCodes.Split(',');

          for (int i = 0; i < codes.Length; i++)
          {
            codes[i] = "'" + codes[i].Trim() + "'";
          }

          cosQuery += " where code in (" + string.Join(",", codes) + ")";
        }

        XmlDocument cosDoc = new XmlDocument();
        int status = 0;
        string cosResults = proxy.query(cosQuery, ref status);
        cosDoc.LoadXml(cosResults);

        foreach (XmlNode coNode in cosDoc.DocumentElement.ChildNodes)
        {
          string classCode = coNode.SelectSingleNode("code").InnerText;
          string className = coNode.SelectSingleNode("name").InnerText;
          string groupId = coNode.SelectSingleNode("group_id").InnerText;

          DataObject dataObjectDef = new DataObject();
          dataObjectDef.objectName = className;
          dataObjectDef.tableName = groupId;

          string ebMetadataQuery = _settings["ebMetadataQuery." + groupId];

          if (string.IsNullOrEmpty(ebMetadataQuery))
            throw new Exception("No meta query configured for group_id [" + groupId + "]");

          string attrsMeatadataQuery = string.Format(ebMetadataQuery, classCode);

          XmlDocument attrsDoc = new XmlDocument();
          string attrsResults = proxy.query(attrsMeatadataQuery, ref status);
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
          switch (groupId)
          {
            case "1":
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
              break;

            case "17":
              dataObjectDef.keyProperties = new List<KeyProperty>()
              {
                new KeyProperty() {
                  keyPropertyName = "PrimaryPhysicalItem.Id",
                },
                new KeyProperty() {
                  keyPropertyName = "Code",
                }
              };
              break;
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

    public Proxy proxy { get; set; }

    public Session session { get; set; }

    public void Connect()
    {
      proxy = new Proxy();

      int ret = proxy.connect(0, _server);

      if (ret < 0)
      {
        throw new Exception(proxy.get_error(ret));
      }

      ret = proxy.logon(0, _dataSource, _userName, EncryptionUtility.Decrypt(_password));
      if (ret < 0)
      {
        throw new Exception(proxy.get_error(ret));
      }

      proxy.silent_mode = true;
      session = new eB.Data.Session();
      session.AttachProtoProxy(proxy.proto_proxy, proxy.connect_info);
    }

    public void Disconnect()
    {
      if (proxy != null) proxy.Dispose();
    }

    public int GetTemplateId(string templateName)
    {
      string selectCommandText;
      int templateId = 0;

      if (proxy.DatabaseType == eB.Common.ConnectionInfo.DatabaseTypes.MicrosoftSQL)
        selectCommandText = string.Format("SELECT template_id FROM templates (NOLOCK) WHERE name = '{0}'", templateName);
      else
        selectCommandText = string.Format("SELECT template_id FROM templates WHERE name = '{0}'", templateName);
      int rc = 0;
      string result = proxy.query(selectCommandText, ref rc);
      XmlDocument doc = new XmlDocument();
      doc.LoadXml(result);

      if (doc.DocumentElement.ChildNodes.Count <= 0)
        throw new Exception("ConnectionGetTemplateIdNoEntryFoundForTemplateName" + templateName + ";");
      templateId = Convert.ToInt32(doc.SelectSingleNode("records/record/template_id").InnerText);

      return (templateId);
    }

    public int GetItemId(string itemNumber, string version)
    {
      try
      {
        string criteria;
        if (version != null)
          criteria = string.Format("Code = '{0}' AND Version = '{1}'", itemNumber, version);
        else
          criteria = string.Format("Code = '{0}'", itemNumber);

        string column = "Id";
        eB.Data.Search s = new Search(session, eB.Common.Enum.ObjectType.Item, column, criteria);
        return (s.RetrieveScalar<int>(column));
      }
      catch (Exception)
      {
        return (0);
      }
    }

    public int GetDocId(string docNumber, string revision)
    {
      try
      {
        string criteria;
        if (revision != null)
          criteria = string.Format("Code = '{0}' AND Revision = '{1}'", docNumber, revision);
        else  //Code LIKE '%' AND IsLatestRevision = 'Y'
          criteria = string.Format("Code = '{0}' AND IsLatestRevision = 'Y'", docNumber);

        string column = "Id";
        eB.Data.Search s = new Search(session, eB.Common.Enum.ObjectType.Document, column, criteria);
        return (s.RetrieveScalar<int>(column));
      }
      catch (Exception)
      {
        return (0);
      }
    }

    public int GetCharId(string charName)
    {
      try
      {
        string criteria = string.Format("Name = '{0}'", charName);
        string column = "Id";
        eB.Data.Search s = new Search(session, eB.Common.Enum.ObjectType.AttributeDef, column, criteria);
        return (s.RetrieveScalar<int>(column));
      }
      catch (Exception)
      {
        return (0);
      }
    }

    public int GetTagId(string itemCode, string itemVersion, string code, string revnName)
    {
      string selectCommandText;
      int tagId = 0;

      int itemId = GetItemId(itemCode, itemVersion);

      if (proxy.DatabaseType == eB.Common.ConnectionInfo.DatabaseTypes.MicrosoftSQL)
        selectCommandText = string.Format("SELECT TOP 1 tag_id FROM tags WHERE item_id = {0} AND code = '{1}' AND revn_name = '{2}'", itemId, code, revnName);
      else
        selectCommandText = string.Format("SELECT tag_id FROM tags WHERE rownum <= 1 AND item_id = {0} AND code = '{1}' AND revn_name = '{2}'", itemId, code, revnName);

      int rc = 0;
      string result = proxy.query(selectCommandText, ref rc);
      XmlDocument doc = new XmlDocument();
      doc.LoadXml(result);

      if (doc.DocumentElement.ChildNodes.Count <= 0)
        throw new Exception("There are no tags that match this code: " + code);

      tagId = Convert.ToInt32(doc.SelectSingleNode("records/record/tag_id").InnerText);

      return (tagId);
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
          DataTable result = ExecuteSearch(session, query, new object[0], -1);

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
      throw new NotImplementedException();
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
  }
}
