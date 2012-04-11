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

namespace org.iringtools.adapter.datalayer
{
  public class ebDataLayer : BaseDataLayer
  {
    private string _dictionaryPath = String.Empty;

    [Inject]
    public ebDataLayer(AdapterSettings settings)
      : base(settings)
    {
      _dictionaryPath = string.Format("{0}DataDictionary.{1}.{2}.xml",
          settings["AppDataPath"],
          settings["ProjectName"],
          settings["ApplicationName"]);
    }

    public Proxy proxy { get; set; }

    public Session session { get; set; }

    public void Connect()
    {
      proxy = new Proxy();
      int ret = proxy.connect(0, _settings["eb_server"]);

      if (ret < 0)
      {
        throw new Exception(proxy.get_error(ret));
      }

      ret = proxy.logon(0, _settings["eb_datasource"], _settings["eb_username"], 
        EncryptionUtility.Decrypt(_settings["eb_password"]));
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

    public override DataDictionary GetDictionary()
    {
      DataDictionary dictionary = new DataDictionary();
            
      if (System.IO.File.Exists(_dictionaryPath))
        return Utility.Read<DataDictionary>(_dictionaryPath);

      try
      {
        Connect();

        string cosQuery = "select * from class_objects where group_id in (1, 17)";

        XmlDocument cosDoc = new XmlDocument();
        int status = 0;
        string cosResults = proxy.query(cosQuery, ref status);
        cosDoc.LoadXml(cosResults);

        foreach (XmlNode coNode in cosDoc.DocumentElement.ChildNodes)
        {
          string classCode = coNode.SelectSingleNode("code").InnerText.Trim();
          int groupId = int.Parse(coNode.SelectSingleNode("group_id").InnerText);

          DataObject dataObjectDef = new DataObject();
          dataObjectDef.objectName = classCode;

          string attrsQuery = @"
              select d.char_name, d.char_data_type, d.char_length, 0 as readonly from class_objects a 
              inner join class_attributes c on c.class_id = a.class_id
              inner join characteristics d on c.char_id = d.char_id
              where a.code = '{0}'";

          attrsQuery = string.Format(attrsQuery, classCode);

          //TODO: use sysobjects, ebType & .net map
          //      use namespace.id and class.id to query for existing
          switch (groupId)
          {
            case 1:
              dataObjectDef.tableName = "Documents." + classCode;
              attrsQuery += @"
                 union select 'Class.Code', 'String', 255, 1
                 union select 'Id', 'Int32', 255 , 1
                 union select 'Code', 'String', 100, 1
                 union select 'Middle', 'String', 100, 1
                 union select 'Revision', 'String', 100, 1
                 union select 'DateEffective', 'DateTime', 100, 1
                 union select 'Name', 'String', 255, 1
                 union select 'ChangeControlled', 'String', 1, 1
                 union select 'ApprovalStatus', 'String', 1, 1
                 union select 'Remark', 'String', 255, 1
                 union select 'Synopsis', 'String', 255, 1
                 union select 'DateObsolete', 'DateTime', 100, 1
                 union select 'Class.Id', 'Int32', 8, 1";
              break;
            case 17:
              dataObjectDef.tableName = "Tags." + classCode;
              attrsQuery += @"
                 union select 'Class.Code', 'String', 255, 1
                 union select 'Id', 'Int32', 255, 1
                 union select 'Class.Id', 'Int32', 255, 1
                 union select 'PrimaryPhysicalItem.Id', 'Int32', 100, 1
                 union select 'Code', 'String', 100, 1
                 union select 'Revision', 'String', 100, 1
                 union select 'Name', 'String', 255, 1
                 union select 'Description', 'String', 1000, 1
                 union select 'ApprovalStatus', 'String', 1, 1
                 union select 'ChangeControlled', 'String', 1, 1
                 union select 'OperationalStatus', 'String', 1, 1
                 union select 'Quantity', 'Int32', 8, 1";
              break;
            //case 22:
            //  attrsQuery = string.Format(attrsQuery, "Relationships");
            //  break;
            default:
              attrsQuery = string.Format(attrsQuery, classCode);
              break;
          }

          XmlDocument attrsDoc = new XmlDocument();
          string attrsResults = proxy.query(attrsQuery, ref status);
          attrsDoc.LoadXml(attrsResults);

          foreach (XmlNode attrNode in attrsDoc.DocumentElement.ChildNodes)
          {
            DataProperty dataProp = new DataProperty();
            dataProp.columnName = attrNode.SelectSingleNode("char_name").InnerText.Trim();
            dataProp.propertyName = Regex.Replace(dataProp.columnName, @" |\.", "");
            dataProp.dataType = ToSystemType(attrNode.SelectSingleNode("char_data_type").InnerText);
            dataProp.isReadOnly = (attrNode.SelectSingleNode("readonly").InnerText == "0") ? true : false;
            dataObjectDef.dataProperties.Add(dataProp);
          }
                    
          dataObjectDef.keyDelimeter = ";";
          switch (groupId)
          {
            case 1:
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
            case 17: 
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

        Utility.Write<DataDictionary>(dictionary, _dictionaryPath);
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

    protected DataType ToSystemType(string ebType)
    {
      return DataType.String;
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
        System.IO.File.Delete(_dictionaryPath);
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
