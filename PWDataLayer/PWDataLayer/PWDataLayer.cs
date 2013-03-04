using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.library;
using Ninject;
using log4net;
using System.Data;
using System.IO;
using System.Collections.Concurrent;
using System.Collections;
using Microsoft.Win32;


///TODO: 
///  project types
///  data type
///  count
///  get
///  content
///  post
///  delete
///  folders
///  authorization
///     SSO: identity
///  identifier: projectid + document id
///  

namespace org.iringtools.adapter.datalayer
{
  public class PWDataLayer : BaseSQLDataLayer
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(PWDataLayer));
    
    private string _sUserName, _sPassword, _sDatasource;
    private string _sProject, _sApp, _sDataPath;

    [Inject]
    public PWDataLayer(AdapterSettings settings)
      : base(settings)
    {
      _sUserName = settings["PW.UserName"];
      _sPassword = settings["PW.Password"];
      _sDatasource = settings["PW.Datasource"];

      _sProject = settings["ProjectName"];
      _sApp = settings["ApplicationName"];
      _sDataPath = settings["AppDataPath"];
    }


    public override DatabaseDictionary GetDatabaseDictionary()
    {
      DatabaseDictionary dbDictionary = null;
      DataDictionary dictionary = new DataDictionary();

      try
      {
        string path = string.Format("{0}DataDictionary.{1}.{2}.xml", _sDataPath, _sProject, _sApp);

        if (File.Exists(path))
        {
          dictionary = utility.Utility.Read<DataDictionary>(path, true);

          dbDictionary = new DatabaseDictionary()
          {
            dataObjects = dictionary.dataObjects,
            dataVersion = dictionary.dataVersion,
            enableSearch = dictionary.enableSearch,
            enableSummary = dictionary.enableSearch,
            picklists = dictionary.picklists
          };

          return dbDictionary;
        }

        Login();

        List<string> dataObjects = new List<string>();
        string dataObjectsStr = _settings["PW.DataObjects"];

        if (dataObjectsStr != null)
        {
          dataObjects = dataObjectsStr.ToString().Replace(" ", "").Split(',').ToList<string>();
        }

        SortedList<string, SortedList<string, TypeAndLength>> env = GetEnvironments();

        foreach (var item in env)
        {
          string itemName = item.Key;

          if (dataObjects == null || dataObjects.Count == 0 || dataObjects.Contains(itemName))
          {
            DataObject dataObject = new DataObject()
            {
              objectName = itemName.Replace(" ", string.Empty),
              tableName = itemName,
              dataProperties = GetGenericProperties()
            };

            foreach (var pair in item.Value)
            {
              ///TODO: type conversion
              string type = pair.Value.DataType;

              DataProperty prop = new DataProperty()
              {
                propertyName = pair.Key,
                columnName = pair.Key,
                dataType = DataType.String,
                dataLength = pair.Value.DataLength
              };

              dataObject.dataProperties.Add(prop);
            }

            dataObject.keyProperties = new List<KeyProperty>()
            {
              new KeyProperty()
              {
                keyPropertyName = "DocumentGUID"
              }
            };

            dictionary.dataObjects.Add(dataObject);
          }
        }

        //env = GetProjectTypes();
        //foreach (var item in env)
        //{
        //  DataObject dataObject = new DataObject()
        //  {
        //    objectName = item.Key.Replace(" ", string.Empty),
        //    tableName = item.Key,
        //    dataProperties = new List<DataProperty>()
        //  };

        //  //TODO: env name is key

        //  foreach (var pair in item.Value)
        //  {
        //    string type = pair.Value.DataType;

        //    DataProperty prop = new DataProperty()
        //    {
        //      propertyName = pair.Key,
        //      dataLength = pair.Value.DataLength
        //    };

        //    dataObject.dataProperties.Add(prop);
        //  }

        //  dictionary.dataObjects.Add(dataObject);
        //}

        utility.Utility.Write<DataDictionary>(dictionary, path, true);
      }
      catch (Exception e)
      {
        _logger.Error(e.Message);
        throw e;
      }
      finally
      {
        Logout();
      }

      return dbDictionary;
    }

    public override Response RefreshDataTable(string tableName)
    {
      Response response = new Response();

      try
      {
        string path = string.Format("{0}DataDictionary.{1}.{2}.xml", _sDataPath, _sProject, _sApp);

        if (File.Exists(path))
        {
          File.Delete(path);
        }

        GetDatabaseDictionary();

        response.Level = StatusLevel.Success;
      }
      catch (Exception e)
      {
        _logger.Error(e.Message);
        response.Level = StatusLevel.Error;
      }
      finally
      {
        Logout();
      }

      return response;
    }

    public override long GetCount(string tableName, string whereClause)
    {
      try
      {
        Login();

        DataTable dt = GetDocumentsForProject(
        _settings["PW.ProjectType"],
        _settings["PW.ProjectProperty"],
        _settings["PW.ProjectName"]);

        if (dt == null)
          return 0;

        return dt.Rows.Count;
      }
      catch (Exception e)
      {
        _logger.Error(e.Message);
        throw e;
      }
      finally
      {
        Logout();
      }
    }

    //TODO: implement paging
    public override DataTable GetDataTable(string tableName, string whereClause, long start, long limit)
    {
      try
      {
        Login();

        DataTable dt = GetDocumentsForProject(
        _settings["PW.ProjectType"],
        _settings["PW.ProjectProperty"],
        _settings["PW.ProjectName"]);

        return dt;
      }
      catch (Exception e)
      {
        _logger.Error(e.Message);
        throw e;
      }
      finally
      {
        Logout();
      }
    }

    public override DataTable GetDataTable(string tableName, IList<string> identifiers) { return null; }

    // Return content if available or metadata otherwise
    public override IList<IDataObject> Get(string objectType, IList<string> identifiers)
    {
      IList<IDataObject> dataObjects = new List<IDataObject>();

      try
      {
        ///TODO: get from config
        string tempFoder = "c:\\temp\\projectwise\\";

        DatabaseDictionary dictionary = GetDatabaseDictionary();
        DataObject objDef = dictionary.dataObjects.Find(x => x.objectName.ToLower() == objectType.ToLower());
        List<string> listAttributes = new List<string>();

        foreach (DataProperty prop in objDef.dataProperties)
        {
          listAttributes.Add(prop.columnName);
        }

        foreach (string identifier in identifiers)
        {
          FileStream stream = GetProjectWiseFile(identifier, tempFoder);

          if (stream != null)
          {
            string docName = stream.Name.ToLower();
            int extIndex = docName.LastIndexOf('.');
            string contentType = "application/msword";

            try
            {
              //TODO: get content type from configuration
              contentType = Registry.ClassesRoot.OpenSubKey(docName.Substring(extIndex)).GetValue("Content Type").ToString();
            }
            catch (Exception ex)
            {
              _logger.Error("Error getting content type: " + ex.ToString());
              throw ex;
            }

            MemoryStream outStream = new MemoryStream();
            stream.CopyTo(outStream);
            outStream.Position = 0;
            stream.Close();

            IContentObject contentObject = new GenericContentObject()
            {
              ObjectType = objectType,
              Identifier = identifier,
              Content = outStream,
              ContentType = contentType
            };

            dataObjects.Add(contentObject);
          }
          else
          {
            try
            {
              Login();

              DataTable dt = GetDocumentMetadata(identifier, listAttributes);

              if (dt != null && dt.Rows.Count > 0)
              {
                IDataObject dataObject = new GenericDataObject()
                {
                  ObjectType = objectType
                };

                foreach (string attr in listAttributes)
                {
                  //TODO: attrs need to be property names instead of column names
                  dataObject.SetPropertyValue(attr, dt.Rows[0][attr]);
                }

                dataObjects.Add(dataObject);
              }
            }
            finally
            {
              Logout();
            }
          }
        }
      }
      catch (Exception e)
      {
        _logger.Error(e.Message);
        throw e;
      }

      return dataObjects;
    }

    public override IList<IContentObject> GetContents(string objectType, IDictionary<string, string> idFormatPairs)
    {
      IList<IContentObject> contentObjects = new List<IContentObject>();

      if (idFormatPairs != null && idFormatPairs.Count > 0)
      {
        ///TODO: get from config
        string tempFoder = "c:\\temp\\projectwise\\";

        Login();

        try
        {
          foreach (var pair in idFormatPairs)
          {
            string identifier = pair.Key;
            string format = pair.Value;
            string contentType = "application/msword";

            FileStream stream = GetProjectWiseFile(identifier, tempFoder);

            if (stream != null)
            {
              if (string.IsNullOrEmpty(format))
              {
                string docName = stream.Name.ToLower();
                int extIndex = docName.LastIndexOf('.');
                format = docName.Substring(extIndex);
              }
              else if (!format.StartsWith("."))
              {
                format = "." + format;
              }

              try
              {
                //TODO: get content type from configuration
                contentType = Registry.ClassesRoot.OpenSubKey(format).GetValue("Content Type").ToString();
              }
              catch (Exception ex)
              {
                _logger.Error("Error getting content type: " + ex.ToString());
                throw ex;
              }

              MemoryStream outStream = new MemoryStream();
              stream.CopyTo(outStream);
              outStream.Position = 0;
              stream.Close();

              IContentObject contentObject = new GenericContentObject()
              {
                ObjectType = objectType,
                Identifier = identifier,
                Content = outStream,
                ContentType = contentType
              };

              contentObjects.Add(contentObject);
            }
          }
        }
        catch (Exception ex)
        {
          _logger.Error("Error getting content objects: " + ex.ToString());
          throw ex;
        }
        finally
        {
          Logout();
        }
      }

      return contentObjects;
    }

    public override DataTable CreateDataTable(string tableName, IList<string> identifiers)
    {
      throw new NotImplementedException();
    }

    public override IList<string> GetIdentifiers(string tableName, string whereClause)
    {
      throw new NotImplementedException();
    }

    public override long GetRelatedCount(DataRow dataRow, string relatedTableName)
    {
      throw new NotImplementedException();
    }

    public override DataTable GetRelatedDataTable(DataRow dataRow, string relatedTableName, long start, long limit)
    {
      throw new NotImplementedException();
    }

    public override DataTable GetRelatedDataTable(DataRow dataRow, string relatedTableName)
    {
      throw new NotImplementedException();
    }

    public override Response Post(IList<IDataObject> dataObjects)
    {
      Response response = new Response();

      try
      {
        DatabaseDictionary dictionary = GetDatabaseDictionary();

        Login();

        foreach (IDataObject dataObject in dataObjects)
        {
          SortedList<string, string> slProps = new SortedList<string, string>();
          string objectType = ((GenericDataObject)dataObject).ObjectType;

          Stream fsi = null;
          if (dataObject is IContentObject)
          {
            fsi = ((IContentObject)dataObject).Content;

            ///TODO: check inbound DataObject for DocumentName          
            slProps.Add("DocumentName", "DWG-" + DateTime.Now.Ticks + "." + ((IContentObject)dataObject).ContentType);
          }

          DataObject objDef = dictionary.dataObjects.Find(x => x.objectName.ToLower() == objectType.ToLower());
          foreach (DataProperty prop in objDef.dataProperties)
          {
            object propValue = null;

            try
            {
              propValue = dataObject.GetPropertyValue(prop.propertyName);
            }
            catch (Exception) { }

            if (propValue != null)
            {
              slProps.Add(prop.columnName, propValue.ToString());
            }
          }

          slProps.Add("ProjectWiseFolderPath", "Bechtel Sample Project\\" + Guid.NewGuid().ToString());

          string docGUID = CreateNewPWDocument(fsi, slProps);
          Status status = new Status()
          {
            Identifier = docGUID
          };

          response.StatusList.Add(status);
        }
      }
      catch (Exception e)
      {
        _logger.Error(e.Message);
        response.Level = StatusLevel.Error;
        response.Messages.Add(e.Message);
      }
      finally
      {
        Logout();
      }

      return response;
    }

    public override Response PostDataTables(IList<DataTable> dataTables)
    {
      try
      {
        return null;
      }
      catch (Exception e)
      {
        _logger.Error(e.Message);
        throw e;
      }
      finally
      {
        Logout();
      }
    }

    public override Response DeleteDataTable(string tableName, IList<string> identifiers)
    {
      throw new NotImplementedException();
    }

    public override Response DeleteDataTable(string tableName, string whereClause)
    {
      throw new NotImplementedException();
    }

    public override Response Delete(string objectType, IList<string> identifiers)
    {
      Response response = new Response();

      try
      {
        Login();

        bool success = DeletePWDocuments(identifiers.ToList());
        response.Level = StatusLevel.Success;
      }
      catch (Exception e)
      {
        response.Level = StatusLevel.Error;
        response.Messages.Add(e.Message);
      }
      finally
      {
        Logout();
      }

      return response;
    }
    
    private bool Login()
    {
      /// If user name and password are null, process creds will be used
      return PWWrapper.aaApi_Login(PWWrapper.DataSourceType.Unknown, _sDatasource, _sUserName, _sPassword, null, true);
    }

    private bool Logout()
    {
      return PWWrapper.aaApi_Logout(_sDatasource);
    }

    private DataTable GetGenericDocumentProperties()
    {
      DataTable dt = new DataTable();
      dt.Columns.Add(new DataColumn("ProjectId", Type.GetType("System.Int32")));
      dt.Columns.Add(new DataColumn("DocumentId", Type.GetType("System.Int32")));
      dt.Columns.Add(new DataColumn("DocumentGUID", Type.GetType("System.String")));
      dt.Columns.Add(new DataColumn("DocumentName", Type.GetType("System.String")));
      dt.Columns.Add(new DataColumn("FileName", Type.GetType("System.String")));
      dt.Columns.Add(new DataColumn("DocumentDesc", Type.GetType("System.String")));
      dt.Columns.Add(new DataColumn("MimeType", Type.GetType("System.String")));
      dt.Columns.Add(new DataColumn("FileUpdateTime", Type.GetType("System.DateTime")));
      dt.Columns.Add(new DataColumn("FileSize", Type.GetType("System.Int32")));
      dt.Columns.Add(new DataColumn("DocumentVersion", Type.GetType("System.String")));
      dt.Columns.Add(new DataColumn("ApplicationId", Type.GetType("System.Int32")));
      dt.Columns.Add(new DataColumn("OriginalNo", Type.GetType("System.Int32")));
      dt.Columns.Add(new DataColumn("StateId", Type.GetType("System.Int32")));
      dt.Columns.Add(new DataColumn("VersionNum", Type.GetType("System.Int32")));
      dt.Columns.Add(new DataColumn("ItemType", Type.GetType("System.Int32")));
      dt.Columns.Add(new DataColumn("CreatorId", Type.GetType("System.Int32")));
      dt.Columns.Add(new DataColumn("UpdaterId", Type.GetType("System.Int32")));
      dt.Columns.Add(new DataColumn("FileUpdaterId", Type.GetType("System.Int32")));
      dt.Columns.Add(new DataColumn("CreateTime", Type.GetType("System.DateTime")));
      dt.Columns.Add(new DataColumn("UpdateTime", Type.GetType("System.DateTime")));
      dt.Columns.Add(new DataColumn("DMSStatus", Type.GetType("System.String")));
      dt.Columns.Add(new DataColumn("SetId", Type.GetType("System.Int32")));
      dt.Columns.Add(new DataColumn("SetType", Type.GetType("System.Int32")));
      dt.Columns.Add(new DataColumn("ProjectGUID", Type.GetType("System.String")));

      return dt;
    }

    private List<DataProperty> GetGenericProperties()
    {
      DataTable dt = GetGenericDocumentProperties();
      List<DataProperty> props = new List<DataProperty>();

      foreach (DataColumn dc in dt.Columns)
      {
        ///TODO: type conversion
        Type dataType = dc.DataType;

        DataProperty prop = new DataProperty()
        {
          propertyName = dc.ColumnName,
          columnName = dc.ColumnName,
          dataType = DataType.String
        };

        props.Add(prop);
      }

      return props;
    }

    private SortedList<string, SortedList<string, TypeAndLength>> GetEnvironments()
    {
      try
      {
        SortedList<string, SortedList<string, TypeAndLength>> slEnvironments = new SortedList<string, SortedList<string, TypeAndLength>>();

        if (!PWWrapper.aaApi_IsConnectionLost())
        {
          int iNumEnvrs = PWWrapper.aaApi_SelectAllEnvs(false);

          if (iNumEnvrs > 0)
          {
            for (int i = 0; i < iNumEnvrs; i++)
            {
              string sEnvName = PWWrapper.aaApi_GetEnvStringProperty(PWWrapper.EnvironmentProperty.Name, i);

              if (!slEnvironments.ContainsKey(sEnvName))
                slEnvironments.Add(sEnvName,
                new SortedList<string, TypeAndLength>());
            }


            if (slEnvironments.Count > 0)
            {
              string[] listEnvNames = new string[slEnvironments.Count];
              slEnvironments.Keys.CopyTo(listEnvNames, 0);

              foreach (string sEnvName in listEnvNames)
              {
                slEnvironments[sEnvName] = GetEnvironmentAttributes(sEnvName);
              }
            }
          }
        }

        return slEnvironments;
      }
      catch (Exception e)
      {
        _logger.Error(e.Message);
        throw e;
      }
      finally
      {
        Logout();
      }
    }

    private enum SQLDataType : int
    {
      FIRST = UNKNOWN,
      UNKNOWN = 0,
      NUMERIC = 1,
      DECIMAL = 2,
      INTEGER = 3,
      SMALLINT = 4,
      FLOAT = 5,
      REAL = 6,
      DOUBLE = 7,
      DATETIME = 8,
      CHAR = 9,
      VARCHAR = 10,
      LONGVARCHAR = 11,
      WCHAR = 12,
      VARWCHAR = 13,
      LONGVARWCHAR = 14,
      DATE = 15,
      TIME = 16,
      TIMESTAMP = 17,
      BINARY = 18,
      VARBINARY = 19,
      LONGVARBINARY = 20,
      GUID = 21,
      BIT = 22,
      BIGINT = 23,
      TINYINT = 26,
      LAST = TINYINT
    }

    private class TypeAndLength
    {
      public TypeAndLength(string sDataType, int iLength)
      {
        DataType = sDataType;
        DataLength = iLength;
      }

      public TypeAndLength()
      {
      }

      public string DataType { get; set; }
      public int DataLength { get; set; }

    }

    private SortedList<string, TypeAndLength> GetEnvironmentAttributes(string sEnvironmentName)
    {
      SortedList<string, TypeAndLength> slColumnDataTypesAndLengths = new SortedList<string, TypeAndLength>(StringComparer.CurrentCultureIgnoreCase);

      int iEnvrID = PWWrapper.GetEnvironmentId(sEnvironmentName);

      if (PWWrapper.aaApi_SelectEnv(iEnvrID) > 0)
      {
        int iColCount = 0;
        int iTableID = PWWrapper.aaApi_GetEnvNumericProperty(PWWrapper.EnvironmentProperty.TableID, 0);

        iColCount = PWWrapper.aaApi_SelectColumnsByTable(iTableID);

        if (iColCount > 0)
        {
          string sType = "";

          for (int i = 0; i < iColCount; i++)
          {
            if (PWWrapper.aaApi_GetColumnStringProperty(PWWrapper.ColumnProperty.Name, i).ToLower() != "a_attrno")
            {
              int iType = PWWrapper.aaApi_GetColumnNumericProperty(PWWrapper.ColumnProperty.SQLType, i);

              switch (iType)
              {
                case (int)SQLDataType.UNKNOWN:
                  sType = "Unknown";
                  break;
                case (int)SQLDataType.BINARY:
                  sType = "Binary";
                  break;
                case (int)SQLDataType.DATE:
                  sType = "Date";
                  break;
                case (int)SQLDataType.DATETIME:
                  sType = "DateTime";
                  break;
                case (int)SQLDataType.DECIMAL:
                  sType = "Decimal";
                  break;
                case (int)SQLDataType.DOUBLE:
                  sType = "Double";
                  break;
                case (int)SQLDataType.VARCHAR:
                  sType = "VarChar";
                  break;
                case (int)SQLDataType.CHAR:
                  sType = "Char";
                  break;
                case (int)SQLDataType.WCHAR:
                  sType = "WChar"; // returns this for dates, too
                  break;
                case (int)SQLDataType.BIGINT:
                  sType = "Big Int";
                  break;
                case (int)SQLDataType.INTEGER:
                  sType = "Int";
                  break;
                case (int)SQLDataType.FLOAT:
                  sType = "Float";
                  break;
                case (int)SQLDataType.TIMESTAMP:
                  sType = "TimeStamp";
                  break;
                case (int)SQLDataType.BIT:
                  sType = "Boolean";
                  break;
                case (int)SQLDataType.VARWCHAR:
                  sType = "VarWChar";
                  break;
                default:
                  sType = string.Format("{0}", iType);
                  break;
              }

              TypeAndLength colTypeAndLength = new TypeAndLength(sType, PWWrapper.aaApi_GetColumnNumericProperty(PWWrapper.ColumnProperty.Length, i));

              slColumnDataTypesAndLengths.Add(PWWrapper.aaApi_GetColumnStringProperty(PWWrapper.ColumnProperty.Name, i),
              colTypeAndLength);

              BPSUtilities.WriteLog(string.Format("Environment:{0} ColumnName:{1} DataType:{2}",
              sEnvironmentName, PWWrapper.aaApi_GetColumnStringProperty(PWWrapper.ColumnProperty.Name, i),
              sType));
            }
          }
        }
      }
      return slColumnDataTypesAndLengths;
    }

    unsafe private static SortedList<string, int> GetListOfProjectTypes()
    {
      SortedList<string, int> slListOfProjectTypes = new SortedList<string, int>();

      PWWrapper.aaApi_Initialize(0);
      PWWrapper.aaOApi_Initialize();
      PWWrapper.aaOApi_LoadAllClasses(0);

      IntPtr pQualPtr = PWWrapper.aaOApi_FindQualifierPtrByName("AAQUAL_PROJECTTYPE");

      if (pQualPtr != null)
      {
        int iQualId = PWWrapper.aaOApi_GetQualifierId(pQualPtr);

        IntPtr pClassIdsArray = IntPtr.Zero;

        int iClassCount = PWWrapper.aaOApi_GetClassesByQualId(iQualId, ref pClassIdsArray);

        if (pClassIdsArray.ToInt32() != 0)
        {
          int[] iClassArrayP = new int[iClassCount];
          iClassArrayP[0] = 0;

          // this is the unsafe part, but it works
          fixed (int* pDest = iClassArrayP)
          {
            int* pSrc = (int*)pClassIdsArray.ToPointer();

            for (int index = 0; index < iClassCount; index++)
            {
              pDest[index] = pSrc[index];
            }
          }

          for (int i = 0; i < iClassCount; i++)
          {
            string sClassName = PWWrapper.GetClassNameFromClassId(iClassArrayP[i]);

            slListOfProjectTypes.Add(sClassName, iClassArrayP[i]);
          }
        }
        else
        {
          BPSUtilities.WriteLog("No classes found");
        }
      }
      else
      {
        BPSUtilities.WriteLog("Qualifier not found");
      }

      return slListOfProjectTypes;
    }

    unsafe private static SortedList<int, string> GetListOfProjectTypesByIds()
    {
      SortedList<int, string> slListOfProjectTypes = new SortedList<int, string>();

      PWWrapper.aaApi_Initialize(0);
      PWWrapper.aaOApi_Initialize();
      PWWrapper.aaOApi_LoadAllClasses(0);

      IntPtr pQualPtr = PWWrapper.aaOApi_FindQualifierPtrByName("AAQUAL_PROJECTTYPE");

      if (pQualPtr != null)
      {
        int iQualId = PWWrapper.aaOApi_GetQualifierId(pQualPtr);

        //if (iQualId == 0) iQualId = 47;

        IntPtr pClassIdsArray = IntPtr.Zero;

        int iClassCount = PWWrapper.aaOApi_GetClassesByQualId(iQualId, ref pClassIdsArray);

        if (pClassIdsArray.ToInt32() != 0)
        {
          int[] iClassArrayP = new int[iClassCount];
          iClassArrayP[0] = 0;

          // this is the unsafe part, but it works
          fixed (int* pDest = iClassArrayP)
          {
            int* pSrc = (int*)pClassIdsArray.ToPointer();

            for (int index = 0; index < iClassCount; index++)
            {
              pDest[index] = pSrc[index];
            }
          }

          for (int i = 0; i < iClassCount; i++)
          {
            string sClassName = PWWrapper.GetClassNameFromClassId(iClassArrayP[i]);

            slListOfProjectTypes.Add(iClassArrayP[i], sClassName);
          }
        }
        else
        {
          BPSUtilities.WriteLog("No classes found");
        }
      }
      else
      {
        BPSUtilities.WriteLog("Qualifier not found");
        BPSUtilities.WriteLog("Error: {0}", PWWrapper.aaApi_GetLastErrorId());
      }

      return slListOfProjectTypes;
    }

    private SortedList<string, SortedList<string, TypeAndLength>> GetProjectTypes()
    {
      //SortedList<int, string> slProjectTypes = ECComponentAssembly.ODSDataWrapper.GetListOfProjectTypesByIds();

      SortedList<int, string> slProjectTypes = GetListOfProjectTypesByIds();
      SortedList<string, SortedList<string, TypeAndLength>> slProjectsAndProperties = new SortedList<string, SortedList<string, TypeAndLength>>(StringComparer.CurrentCultureIgnoreCase);

      if (slProjectTypes.Count == 0)
      {
        BPSUtilities.WriteLog("No rich project types found");
        return slProjectsAndProperties;
      }

      foreach (int iClassId in slProjectTypes.Keys)
      {
        if (!slProjectsAndProperties.ContainsKey(slProjectTypes[iClassId]))
        {
          SortedList<string, int> slProperties = PWWrapper.GetClassPropertyIdsInList(iClassId);

          SortedList<string, TypeAndLength> slPropNamesTypesAndLengths = new SortedList<string, TypeAndLength>(StringComparer.CurrentCultureIgnoreCase);

          foreach (KeyValuePair<string, int> kvp in slProperties)
          {
            TypeAndLength typeAndLength = GetAttributeDataTypeAndLength(kvp.Value);

            if (!slPropNamesTypesAndLengths.ContainsKey(kvp.Key))
              slPropNamesTypesAndLengths.Add(kvp.Key, typeAndLength);
          }

          slProjectsAndProperties.Add(slProjectTypes[iClassId], slPropNamesTypesAndLengths);
        }
      }

      return slProjectsAndProperties;
    }

    private TypeAndLength GetAttributeDataTypeAndLength(int iAttrId)
    {
      int iRetVal = -1;

      TypeAndLength typeAndLength = new TypeAndLength();

      IntPtr pAttr = IntPtr.Zero;

      // find the attribute using the id
      if (PWWrapper.aaOApi_FindAttribute(ref pAttr, iAttrId))
      {
        StringBuilder wAttrName = new StringBuilder(256);
        StringBuilder wAttrDesc = new StringBuilder(256);

        int lVisibility = 0, iType = 0, iDataLen = 0;

        if (PWWrapper.aaOApi_GetAttributeCmnProps(pAttr, ref iAttrId, wAttrName, wAttrDesc,
        ref lVisibility, ref iType, ref iDataLen))
        {
          iRetVal = iType;
        }

        BPSUtilities.WriteLog("Name: {0}", wAttrName.ToString());

        typeAndLength.DataType = GetAttributeDataTypeName(iType);
        typeAndLength.DataLength = iDataLen;

        BPSUtilities.WriteLog("Control: {0}",
        PWWrapper.aaOApi_GetAttributeNumericProperty(pAttr,
        PWWrapper.ODSAttributeProperty.Control));

        BPSUtilities.WriteLog("UIType: {0}",
        PWWrapper.aaOApi_GetAttributeNumericProperty(pAttr,
        PWWrapper.ODSAttributeProperty.UIType));

        // PWWrapper.aaOApi_SetAttributeDbProps();

        int iControl = 0;
        StringBuilder sbColName = new StringBuilder(256);

        PWWrapper.aaOApi_GetAttributeDbProps(pAttr, ref iControl, sbColName);

        // PWWrapper.aaOApi_SetAttributeDbProps(pAttr, 0, sbColName.ToString());

        BPSUtilities.WriteLog("DBControl: {0}", iControl);
        BPSUtilities.WriteLog("Column: {0}", sbColName.ToString());

        IntPtr pQual = IntPtr.Zero;

        PWWrapper.aaOApi_FindQualifierByName("ForceToList", ref pQual);

        int iQualId = PWWrapper.aaOApi_GetQualifierId(pQual);

        int iQualId2 = 0, iTypeId2 = 0, iQualVal = 0;

        PWWrapper.aaOApi_GetAttributeQualifierProperties(pAttr, iQualId, 0, ref iQualId2,
        ref iTypeId2, ref iQualVal);

        BPSUtilities.WriteLog("ForceToList, ID: {0}, Type: {1}, Val: {2}",
        iQualId, iTypeId2, iQualVal);

        // PWWrapper.aaOApi_RemoveAttributeQualifier(iQualId, pAttr);

        // PWWrapper.aaOApi_SaveAttribute(ref pAttr);

        // PWWrapper.aaOApi_FindQualifierByName();

        // PWWrapper.aaOApi_GetQualifierId()
      }

      return typeAndLength;
    }

    private string GetAttributeDataTypeName(int iType)
    {
      string sRetVal = "None";

      switch ((PWWrapper.ODSAttributeDataType)iType)
      {
        case PWWrapper.ODSAttributeDataType.Int16:
          sRetVal = "Int16";
          break;
        case PWWrapper.ODSAttributeDataType.Long32:
          sRetVal = "Long32";
          break;
        case PWWrapper.ODSAttributeDataType.Float32:
          sRetVal = "Float32";
          break;
        case PWWrapper.ODSAttributeDataType.Double64:
          sRetVal = "Double64";
          break;
        case PWWrapper.ODSAttributeDataType.DateTime:
          sRetVal = "DateTime";
          break;
        case PWWrapper.ODSAttributeDataType.Timestamp:
          sRetVal = "Timestamp";
          break;
        case PWWrapper.ODSAttributeDataType.String:
          sRetVal = "String";
          break;
        case PWWrapper.ODSAttributeDataType.Raw:
          sRetVal = "Raw";
          break;
        case PWWrapper.ODSAttributeDataType.LongRaw:
          sRetVal = "LongRaw";
          break;
        default:
          break;
      }


      return sRetVal;
    }

    //private SortedList<string, SortedList<string, string>> GetEnvironments()
    //{
    //  SortedList<string, SortedList<string, string>> slEnvironments = new SortedList<string, SortedList<string, string>>();

    //  if (!PWWrapper.aaApi_IsConnectionLost())
    //  {
    //    int iNumEnvrs = PWWrapper.aaApi_SelectAllEnvs(false);

    //    if (iNumEnvrs > 0)
    //    {
    //      for (int i = 0; i < iNumEnvrs; i++)
    //      {
    //        if (!slEnvironments.ContainsKey(PWWrapper.aaApi_GetEnvStringProperty(PWWrapper.EnvironmentProperty.Name, i)))
    //          slEnvironments.Add(PWWrapper.aaApi_GetEnvStringProperty(PWWrapper.EnvironmentProperty.Name, i),
    //          new SortedList<string, string>());
    //      }
    //    }
    //  }

    //  return slEnvironments;
    //}

    private System.IO.FileStream GetProjectWiseFile(string sDocumentGuid, string sWorkingDirectory)
    {
      try
      {
        Login();

        Guid docGuid = new Guid(sDocumentGuid);

        StringBuilder sbOutFile = new StringBuilder(1024);

        Directory.CreateDirectory(sWorkingDirectory);

        if (PWWrapper.aaApi_GUIDCopyOutDocument(ref docGuid, sWorkingDirectory, sbOutFile, sbOutFile.Capacity))
        {
          if (File.Exists(sbOutFile.ToString()))
          {
            return new FileStream(sbOutFile.ToString(), FileMode.Open);
          }
        }
        else
        {
          BPSUtilities.WriteLog("Error on copy out: {0}", PWWrapper.aaApi_GetLastErrorId());
        }
      }
      catch (Exception ex)
      {
        BPSUtilities.WriteLog("Error: {0}\n{1}", ex.Message, ex.StackTrace);
      }
      finally
      {
        Logout();
      }

      return null;
    }

    //private DataTable GetDocumentsForProject(string sProjectType, string sProjectPropertyName, string sPropertyValue)
    //{
    //  SavedSearchAssembly.SavedSearchWrapper.InitializeQuery(0, false);

    //  SortedList<string, string> slQueryPropVals = new SortedList<string, string>();

    //  slQueryPropVals.Add(sProjectPropertyName, sPropertyValue);

    //  SortedList<int, string> slProjects = SavedSearchAssembly.SavedSearchWrapper.GetListOfRichProjectsFromProperties(sProjectType,
    //  slQueryPropVals, false);

    //  DataTable dtReturn = new DataTable("Documents");

    //  foreach (int iProjectId in slProjects.Keys)
    //  {
    //    SavedSearchAssembly.SavedSearchWrapper.InitializeQuery(iProjectId, true);
    //    SavedSearchAssembly.SavedSearchWrapper.AddOriginalsOnlyCriterion(0);
    //    DataTable dt = SavedSearchAssembly.SavedSearchWrapper.Search();

    //    if (dtReturn.Columns.Count == 0)
    //      dtReturn = dt;
    //    else
    //    {
    //      foreach (DataRow dr in dt.Rows)
    //      {
    //        dtReturn.Rows.Add(dr.ItemArray);
    //      }
    //    }
    //  }

    //  return dtReturn;
    //}

    private DataTable GetDocumentsForProject(string sProjectType, string sProjectPropertyName, string sPropertyValue)
    {
      SavedSearchAssembly.SavedSearchWrapper.InitializeQuery(0, false);

      SortedList<string, string> slQueryPropVals = new SortedList<string, string>();

      slQueryPropVals.Add(sProjectPropertyName, sPropertyValue);

      SortedList<int, string> slProjects = SavedSearchAssembly.SavedSearchWrapper.GetListOfRichProjectsFromProperties(sProjectType,
      slQueryPropVals, false);

      DataTable dtReturn = new DataTable("Documents");

      foreach (int iProjectId in slProjects.Keys)
      {
        List<string> listAttributes = new List<string>();

        if (1 == PWWrapper.aaApi_SelectProject(iProjectId))
        {
          int iEnvId = PWWrapper.aaApi_GetProjectNumericProperty(PWWrapper.ProjectProperty.EnvironmentID, 0);
          SortedList<string, TypeAndLength> slAttributes = GetEnvironmentAttributes(null, iEnvId);
          foreach (string sColumnName in slAttributes.Keys)
            listAttributes.Add(sColumnName);
        }

        SavedSearchAssembly.SavedSearchWrapper.InitializeQuery(iProjectId, true);
        SavedSearchAssembly.SavedSearchWrapper.AddOriginalsOnlyCriterion(0);
        DataTable dt = SavedSearchAssembly.SavedSearchWrapper.Search(listAttributes);
        
        if (dtReturn.Columns.Count == 0)
          dtReturn = dt;
        else
        {
          foreach (DataRow dr in dt.Rows)
          {
            dtReturn.Rows.Add(dr.ItemArray);
          }
        }
      }

      return dtReturn;
    }

    private SortedList<string, TypeAndLength> GetEnvironmentAttributes(string sEnvironmentName, int iEnvId)
    {
      SortedList<string, TypeAndLength> slColumnDataTypesAndLengths = new SortedList<string, TypeAndLength>(StringComparer.CurrentCultureIgnoreCase);

      int iEnvrID = iEnvId;

      if (!string.IsNullOrEmpty(sEnvironmentName))
        iEnvrID = PWWrapper.GetEnvironmentId(sEnvironmentName);

      if (PWWrapper.aaApi_SelectEnv(iEnvrID) > 0)
      {
        int iColCount = 0;
        int iTableID = PWWrapper.aaApi_GetEnvNumericProperty(PWWrapper.EnvironmentProperty.TableID, 0);

        iColCount = PWWrapper.aaApi_SelectColumnsByTable(iTableID);

        if (iColCount > 0)
        {
          string sType = "";

          for (int i = 0; i < iColCount; i++)
          {
            if (PWWrapper.aaApi_GetColumnStringProperty(PWWrapper.ColumnProperty.Name, i).ToLower() != "a_attrno")
            {
              int iType = PWWrapper.aaApi_GetColumnNumericProperty(PWWrapper.ColumnProperty.SQLType, i);

              switch (iType)
              {
                case (int)SQLDataType.UNKNOWN:
                  sType = "Unknown";
                  break;
                case (int)SQLDataType.BINARY:
                  sType = "Binary";
                  break;
                case (int)SQLDataType.DATE:
                  sType = "Date";
                  break;
                case (int)SQLDataType.DATETIME:
                  sType = "DateTime";
                  break;
                case (int)SQLDataType.DECIMAL:
                  sType = "Decimal";
                  break;
                case (int)SQLDataType.DOUBLE:
                  sType = "Double";
                  break;
                case (int)SQLDataType.VARCHAR:
                  sType = "VarChar";
                  break;
                case (int)SQLDataType.CHAR:
                  sType = "Char";
                  break;
                case (int)SQLDataType.WCHAR:
                  sType = "WChar"; // returns this for dates, too
                  break;
                case (int)SQLDataType.BIGINT:
                  sType = "Big Int";
                  break;
                case (int)SQLDataType.INTEGER:
                  sType = "Int";
                  break;
                case (int)SQLDataType.FLOAT:
                  sType = "Float";
                  break;
                case (int)SQLDataType.TIMESTAMP:
                  sType = "TimeStamp";
                  break;
                case (int)SQLDataType.BIT:
                  sType = "Boolean";
                  break;
                case (int)SQLDataType.VARWCHAR:
                  sType = "VarWChar";
                  break;
                default:
                  sType = string.Format("{0}", iType);
                  break;
              }

              TypeAndLength colTypeAndLength = new TypeAndLength(sType,
              PWWrapper.aaApi_GetColumnNumericProperty(PWWrapper.ColumnProperty.Length, i));

              slColumnDataTypesAndLengths.Add(PWWrapper.aaApi_GetColumnStringProperty(PWWrapper.ColumnProperty.Name, i),
              colTypeAndLength);

              BPSUtilities.WriteLog(string.Format("Environment:{0} ColumnName:{1} DataType:{2}",
              sEnvironmentName, PWWrapper.aaApi_GetColumnStringProperty(PWWrapper.ColumnProperty.Name, i),
              sType));
            }
          }
        }
      }
      return slColumnDataTypesAndLengths;
    }

    private int CreateNewFileOrVersion1(int iProjectId, string sFilePath, SortedList<string, string> slProps)
    {
      SavedSearchAssembly.SavedSearchWrapper.InitializeQuery(iProjectId, false);
      SavedSearchAssembly.SavedSearchWrapper.AddDocumentCriteria(slProps["ProjectWiseName"], null, null, 0);
      SavedSearchAssembly.SavedSearchWrapper.AddOriginalsOnlyCriterion(0);

      DataTable dt = SavedSearchAssembly.SavedSearchWrapper.Search();

      if (dt.Rows.Count == 0)
      {
        // search for the filename
        SavedSearchAssembly.SavedSearchWrapper.InitializeQuery(iProjectId, false);
        SavedSearchAssembly.SavedSearchWrapper.AddDocumentCriteria(null, slProps["ProjectWiseFileName"], null, 0);
        SavedSearchAssembly.SavedSearchWrapper.AddOriginalsOnlyCriterion(0);

        dt = SavedSearchAssembly.SavedSearchWrapper.Search();
      }

      Hashtable ht = new Hashtable();

      foreach (KeyValuePair<string, string> kvp in slProps)
      {
        if (!ht.ContainsKey(kvp.Key.ToLower()))
          ht.Add(kvp.Key.ToLower(), kvp.Value);
      }

      if (dt.Rows.Count > 0)
      {
        int iTargetDocumentId = (int)dt.Rows[0]["DocumentId"];

        int iNewVersionId = 0;

        if (PWWrapper.aaApi_NewDocumentVersion(PWWrapper.NewVersionCreationFlags.CopyAttrs | PWWrapper.NewVersionCreationFlags.KeepRelations,
        iProjectId, iTargetDocumentId, null, null, ref iNewVersionId))
        {
          BPSUtilities.WriteLog("New version created");

          if (PWWrapper.aaApi_ChangeDocumentFile(iProjectId, iTargetDocumentId, sFilePath))
          {
            File.Delete(sFilePath);
          }
          // convert sorted list to hash table

          if (!PWWrapper.SetAttributesValues(iProjectId, iTargetDocumentId, ht))
            BPSUtilities.WriteLog("Attribute update failed");

          return iTargetDocumentId;
        }
        else
        {
          int iDocumentId = 0, iAttrId = 0;
          StringBuilder sbWorkingFile = new StringBuilder(1024);

          if (PWWrapper.aaApi_CreateDocument(ref iDocumentId, iProjectId, 0,
          0, PWWrapper.DocumentType.Normal, PWWrapper.aaApi_GetFExtensionApplication(Path.GetExtension(slProps["ProjectWiseFileName"])),
          0, 0, sFilePath, slProps["ProjectWiseFileName"], slProps["ProjectWiseName"], slProps["ProjectWiseName"],
          null, false, PWWrapper.DocumentCreationFlag.NoAttributeRecord, sbWorkingFile, sbWorkingFile.Capacity,
          ref iAttrId))
          {
            BPSUtilities.WriteLog("Created document '{0}' OK", PWWrapper.GetDocumentNamePath(iProjectId, iDocumentId));

            if (!PWWrapper.SetAttributesValues(iProjectId, iDocumentId, ht))
              BPSUtilities.WriteLog("Attribute update failed");

            return iDocumentId;
          }
          else
          {
            BPSUtilities.WriteLog("Error creating document '{0}': {1}",
            slProps["ProjectWiseName"], PWWrapper.aaApi_GetLastErrorId());
          }
        }
      }

      return 0;
    }

    private string CreateNewPWDocument(Stream fsi, SortedList<string, string> slProps)
    {
      string sFileName = string.Empty;

      if (!slProps.ContainsKey("ProjectWiseFolderPath"))
      {
        BPSUtilities.WriteLog("No ProjectWiseFolderPath provided, using default");

        //slProps.Add("ProjectWiseFolderPath", BPSUtilities.GetSetting("DefaultFolderPath"));
        
        string projectWiseFolderPath = _settings["PW.DefaultFolderPath"];
        slProps.Add("ProjectWiseFolderPath", projectWiseFolderPath);
      }
      else if (string.IsNullOrEmpty(slProps["ProjectWiseFolderPath"]))
      {
        BPSUtilities.WriteLog("No ProjectWiseFolderPath provided, using default");

        //slProps["ProjectWiseFolderPath"] = BPSUtilities.GetSetting("DefaultFolderPath");
        string projectWiseFolderPath = _settings["PW.DefaultFolderPath"];
        slProps["ProjectWiseFolderPath"] = projectWiseFolderPath;
      }

      if (!slProps.ContainsKey("ProjectWiseName"))
      {
        BPSUtilities.WriteLog("No ProjectWiseName provided, using default");
        //slProps.Add("ProjectWiseName", string.Format("{0}_{1}", BPSUtilities.GetSetting("DefaultName"), Guid.NewGuid().ToString()));

        string projectWiseName = _settings["PW.DefaultName"];
        slProps.Add("ProjectWiseName", projectWiseName + "_" + Guid.NewGuid().ToString());
      }
      else if (string.IsNullOrEmpty(slProps["ProjectWiseName"]))
      {
        BPSUtilities.WriteLog("No ProjectWiseName provided, using default");
        //slProps["ProjectWiseName"] = string.Format("{0}_{1}", BPSUtilities.GetSetting("DefaultName"), Guid.NewGuid().ToString());

        string projectWiseName = _settings["PW.DefaultName"];
        slProps["ProjectWiseName"] = projectWiseName + "_" + Guid.NewGuid().ToString();
      }

      //int iStorageId = PWWrapper.GetStorageAreaId(BPSUtilities.GetSetting("StorageArea"));
      int iStorageId = PWWrapper.GetStorageAreaId(_settings["PW.StorageArea"]);

      //int iEnvId = PWWrapper.GetEnvironmentId(BPSUtilities.GetSetting("Environment"));
      int iEnvId = PWWrapper.GetEnvironmentId(_settings["PW.Environment"]);

      //int iWorkflowId = PWWrapper.GetWorkflowId(BPSUtilities.GetSetting("Workflow"));
      int iWorkflowId = PWWrapper.GetWorkflowId(_settings["PW.Workflow"]);

      if (iStorageId == 0)
      {
        BPSUtilities.WriteLog("StorageArea '{0}' not found", _settings["PW.StorageArea"]);
        return string.Empty;
      }

      int iProjectId = PWWrapper.BuildPathWithBackslashesOnly(slProps["ProjectWiseFolderPath"], -1, iEnvId, iStorageId, iWorkflowId);
      //int iProjectId = PWWrapper.GetProjectNoFromPath(slProps["ProjectWiseFolderPath"]);
      
      if (iProjectId > 0)
      {
        if (fsi != null)
        {
          //string sReceiveDir = BPSUtilities.GetSetting("ReceiveDir");
          string sReceiveDir = _settings["PW.ReceiveDir"];

          Directory.CreateDirectory(sReceiveDir);

          if (fsi.CanRead)
          {
            long lLength = fsi.Length;

            byte[] bytes = new byte[lLength];

            fsi.Read(bytes, 0, (int)lLength);

            string sFileOut = Path.Combine(sReceiveDir, Guid.NewGuid().ToString());

            FileStream fso = new FileStream(sFileOut, FileMode.Create);

            fso.Write(bytes, 0, bytes.Length);

            fso.Close();

            if (File.Exists(sFileOut))
            {
              if (!slProps.ContainsKey("ProjectWiseFileName"))
              {
                BPSUtilities.WriteLog("No ProjectWiseFileName provided, using default");
                slProps.Add("ProjectWiseFileName", slProps["ProjectWiseName"]);
              }
              else if (string.IsNullOrEmpty(slProps["ProjectWiseFileName"]))
              {
                BPSUtilities.WriteLog("No ProjectWiseFileName provided, using default");
                slProps["ProjectWiseFileName"] = slProps["ProjectWiseName"];
              }

              int iDocumentId = CreateNewFileOrVersion(iProjectId, sFileOut, slProps);

              if (iDocumentId > 0)
              {
                return PWWrapper.GetGuidStringFromIds(iProjectId, iDocumentId);
              }
            }
          }
        }
        else
        {
          int iDocumentId =  CreateNewFileOrVersion(iProjectId, null, slProps);

          if (iDocumentId > 0)
          {
            return PWWrapper.GetGuidStringFromIds(iProjectId, iDocumentId);
          }
        }
      }
      else
      {
        BPSUtilities.WriteLog("Error creating/finding folder: '{0}'", slProps["ProjectWiseFolderPath"]);
      }

      return string.Empty;
    }

    private bool UpdatePWAttributes(SortedList<string, string> slProps)
    {
      if (slProps.ContainsKey("DocumentGUID"))
      {
        string sDocumentGuid = slProps["DocumentGUID"];

        try
        {
          Guid docGuid = new Guid(sDocumentGuid);

          if (1 == PWWrapper.aaApi_GUIDSelectDocument(ref docGuid))
          {
            Hashtable ht = new Hashtable();

            foreach (KeyValuePair<string, string> kvp in slProps)
            {
              if (!ht.ContainsKey(kvp.Key.ToLower()))
                ht.Add(kvp.Key.ToLower(), kvp.Value);
            }

            int iProjectId = PWWrapper.aaApi_GetDocumentNumericProperty(PWWrapper.DocumentProperty.ProjectID, 0);
            int iDocumentid = PWWrapper.aaApi_GetDocumentId(0);

            return PWWrapper.SetAttributesValues(iProjectId, iDocumentid, ht);
          }
          else
          {
            BPSUtilities.WriteLog("DocumentGUID '{0}' not found", sDocumentGuid);
          }
        }
        catch (Exception ex)
        {
          BPSUtilities.WriteLog("Error: {0}\n{1}", ex.Message, ex.StackTrace);
        }
      }
      else
      {
        BPSUtilities.WriteLog("Property 'DocumentGUID' not passed");
      }

      return false;
    }

    private int CreateNewFileOrVersion(int iProjectId, string sFilePath, SortedList<string, string> slProps)
    {
      SavedSearchAssembly.SavedSearchWrapper.InitializeQuery(iProjectId, false);
      SavedSearchAssembly.SavedSearchWrapper.AddDocumentCriteria(slProps["ProjectWiseName"], null, null, 0);
      SavedSearchAssembly.SavedSearchWrapper.AddOriginalsOnlyCriterion(0);

      DataTable dt = SavedSearchAssembly.SavedSearchWrapper.Search();
      Hashtable ht = new Hashtable();
      
      foreach (KeyValuePair<string, string> kvp in slProps)
      {
        if (!ht.ContainsKey(kvp.Key.ToLower()))
          ht.Add(kvp.Key.ToLower(), kvp.Value);
      }

      if (!string.IsNullOrEmpty(sFilePath))
      {
        if (dt.Rows.Count == 0)
        {
          // search for the filename
          SavedSearchAssembly.SavedSearchWrapper.InitializeQuery(iProjectId, false);
          SavedSearchAssembly.SavedSearchWrapper.AddDocumentCriteria(null, slProps["ProjectWiseFileName"], null, 0);
          SavedSearchAssembly.SavedSearchWrapper.AddOriginalsOnlyCriterion(0);

          dt = SavedSearchAssembly.SavedSearchWrapper.Search();
        }

        if (dt.Rows.Count > 0)
        {
          int iTargetDocumentId = (int)dt.Rows[0]["DocumentId"];

          int iNewVersionId = 0;

          if (PWWrapper.aaApi_NewDocumentVersion(PWWrapper.NewVersionCreationFlags.CopyAttrs | PWWrapper.NewVersionCreationFlags.KeepRelations,
          iProjectId, iTargetDocumentId, null, null, ref iNewVersionId))
          {
            BPSUtilities.WriteLog("New version created");

            if (PWWrapper.aaApi_ChangeDocumentFile(iProjectId, iTargetDocumentId, sFilePath))
            {
              File.Delete(sFilePath);
            }

            if (!PWWrapper.SetAttributesValues(iProjectId, iTargetDocumentId, ht))
              BPSUtilities.WriteLog("Attribute update failed");

            return iTargetDocumentId;
          }
        }
        else
        {
          int iDocumentId = 0, iAttrId = 0;
          StringBuilder sbWorkingFile = new StringBuilder(1024);

          if (PWWrapper.aaApi_CreateDocument(ref iDocumentId, iProjectId, 0,
          0, PWWrapper.DocumentType.Normal, PWWrapper.aaApi_GetFExtensionApplication(Path.GetExtension(slProps["ProjectWiseFileName"])),
          0, 0, sFilePath, slProps["ProjectWiseFileName"], slProps["ProjectWiseName"], slProps["ProjectWiseName"],
          null, false, PWWrapper.DocumentCreationFlag.NoAttributeRecord, sbWorkingFile, sbWorkingFile.Capacity,
          ref iAttrId))
          {
            BPSUtilities.WriteLog("Created document '{0}' OK", PWWrapper.GetDocumentNamePath(iProjectId, iDocumentId));

            if (!PWWrapper.SetAttributesValues(iProjectId, iDocumentId, ht))
              BPSUtilities.WriteLog("Attribute update failed");

            return iDocumentId;
          }
          else
          {
            BPSUtilities.WriteLog("Error creating document '{0}': {1}",
            slProps["ProjectWiseName"], PWWrapper.aaApi_GetLastErrorId());
          }
        }
      }
      else
      {          
        int iDocumentId = 0, iAttrId = 0;
        StringBuilder sbWorkingFile = new StringBuilder(1024);

        if (PWWrapper.aaApi_CreateDocument(ref iDocumentId, iProjectId, 0,
        0, PWWrapper.DocumentType.Abstract, 0,
        0, 0, null, null, slProps["ProjectWiseName"], slProps["ProjectWiseName"],
        null, false, PWWrapper.DocumentCreationFlag.NoAttributeRecord, sbWorkingFile, sbWorkingFile.Capacity,
        ref iAttrId))
        {
          BPSUtilities.WriteLog("Created document '{0}' OK", PWWrapper.GetDocumentNamePath(iProjectId, iDocumentId));

          if (!PWWrapper.SetAttributesValues(iProjectId, iDocumentId, ht))
            BPSUtilities.WriteLog("Attribute update failed");

          return iDocumentId;
        }
        else
        {
          BPSUtilities.WriteLog("Error creating document '{0}': {1}",
          slProps["ProjectWiseName"], PWWrapper.aaApi_GetLastErrorId());
        }
      }

      return 0;
    }

    //public override DataTable GetDataTable(string tableName, IList<string> identifiers)
    //{
    //  try
    //  {
    //    List<string> docGuids = identifiers.ToList();
    //    List<string> listAttributes = new List<string>();

    //    DatabaseDictionary dictionary = GetDatabaseDictionary();
    //    DataObject objDef = dictionary.dataObjects.Find(x => x.tableName.ToLower() == tableName.ToLower());
        
    //    foreach (DataProperty prop in objDef.dataProperties)
    //    {
    //      listAttributes.Add(prop.columnName);
    //    }

    //    Login();

    //    DataTable dt = GetDocumentMetadata(docGuids, listAttributes);
    //    return dt;
    //  }
    //  catch (Exception e)
    //  {
    //    _logger.Error(e.Message);
    //    throw e;
    //  }
    //  finally
    //  {
    //    Logout();
    //  }
    //}

    private DataTable GetDocumentMetadata(string docGuid, List<string> listAttributes)
    {
      SavedSearchAssembly.SavedSearchWrapper.InitializeQuery(0, false);
      SavedSearchAssembly.SavedSearchWrapper.AddDocumentGuidCriterion(docGuid, 0);
      DataTable dt = SavedSearchAssembly.SavedSearchWrapper.Search(listAttributes);
      return dt;
    }

    private DataTable GetDocumentMetadata(List<string> docGuids, List<string> listAttributes)
    {
      DataTable dtReturn = new DataTable();

      foreach (string sDocGuid in docGuids)
      {
        SavedSearchAssembly.SavedSearchWrapper.InitializeQuery(0, false);
        SavedSearchAssembly.SavedSearchWrapper.AddDocumentGuidCriterion(sDocGuid, 0);

        DataTable dt = SavedSearchAssembly.SavedSearchWrapper.Search(listAttributes);

        if (dtReturn.Columns.Count == 0)
          dtReturn = dt;
        else
        {
          foreach (DataRow dr in dt.Rows)
            dtReturn.Rows.Add(dr.ItemArray);
        }
      }

      return dtReturn;
    }

    private bool DeletePWDocuments(List<string> docGuids)
    {
      int iSuccessfulDeletes = 0;

      foreach (string sDocGuid in docGuids)
      {
        Guid docGuid = new Guid(sDocGuid);

        if (PWWrapper.aaApi_GUIDDeleteDocument(PWWrapper.DocumentDeleteMasks.IncludeVersions, ref docGuid))
        {
          iSuccessfulDeletes++;
        }
        else
        {
          BPSUtilities.WriteLog("Error deleting document '{0}' {1}", PWWrapper.GetDocumentNamePath(docGuid),
          PWWrapper.aaApi_GetLastErrorId());
        }
      }

      return (iSuccessfulDeletes == docGuids.Count);
    }

    private SortedList<int, string> GetTopLevelFolders()
    {
      try
      {
        Login();

        SortedList<int, string> slProjects = new SortedList<int, string>();

        int iNumFolders = PWWrapper.aaApi_SelectTopLevelProjects();

        for (int i = 0; i < iNumFolders; i++)
        {
          slProjects.Add(PWWrapper.aaApi_GetProjectId(i), PWWrapper.aaApi_GetProjectStringProperty(PWWrapper.ProjectProperty.Name, i));
        }

        return slProjects;
      }
      finally
      {
        Logout();
      }
    }

    private SortedList<int, string> GetChildFolders(int iParentId)
    {
      try
      {
        Login();

        SortedList<int, string> slProjects = new SortedList<int, string>();

        int iNumFolders = PWWrapper.aaApi_SelectChildProjects(iParentId);

        for (int i = 0; i < iNumFolders; i++)
        {
          slProjects.Add(PWWrapper.aaApi_GetProjectId(i), PWWrapper.aaApi_GetProjectStringProperty(PWWrapper.ProjectProperty.Name, i));
        }

        return slProjects;
      }
      finally
      {
        Logout();
      }
    }

    private string GetFolderPath(int iFolderId)
    {
      return PWWrapper.GetProjectNamePath(iFolderId);
    }

    private DataTable GetDocumentsForFolder(int iFolderId, List<string> listAttributes)
    {
      DataTable dtReturn = new DataTable();

      SavedSearchAssembly.SavedSearchWrapper.InitializeQuery(iFolderId, true);
      // SavedSearchAssembly.SavedSearchWrapper.AddOriginalsOnlyCriterion(0);
      DataTable dt = SavedSearchAssembly.SavedSearchWrapper.Search(listAttributes);

      if (dtReturn.Columns.Count == 0)
        dtReturn = dt;
      else
      {
        foreach (DataRow dr in dt.Rows)
        {
          dtReturn.Rows.Add(dr.ItemArray);
        }
      }

      return dtReturn;
    }
  }
}
