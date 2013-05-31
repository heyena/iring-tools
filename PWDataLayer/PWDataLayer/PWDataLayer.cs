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




namespace org.iringtools.adapter.datalayer
{
  public class PWDataLayer : BaseSQLDataLayer
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(PWDataLayer));

    private string _sUserName, _sPassword, _sDatasource, _sFileFormat, _sDocumentGUID; 
    private string _sProject, _sApp, _sDataPath;
    private int _iFoldrCount;
    private int iParentFolderID;
   
      /// <summary>
      ///constructor for initialization
      /// </summary>
      /// <param name="settings"></param>
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
      _sFileFormat = settings["Format"];
      _sDocumentGUID = settings["DocumentGUID"];
    }

/// <summary>
/// This method used to create dictionary
/// </summary>
/// <returns></returns>
    public override DatabaseDictionary GetDatabaseDictionary()
    {

        //_logger.Debug("In Dictionary Creation");
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

          if (dataObjects == null || dataObjects.Count == 0 || dataObjects.Contains(itemName.Replace(" ", "")))
          {
            DataObject dataObject = new DataObject()
            {
              objectName = itemName.Replace(" ", string.Empty),
              tableName = itemName,
              dataProperties = GetGenericProperties()
            };

            foreach (var pair in item.Value)
            {
              
              string type = (string)pair.Value.DataType;   

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

          
        DataObject subFolderDataObject = new DataObject();
        subFolderDataObject.objectName = "SubFolders";
        subFolderDataObject.tableName = "SubFolders";
        
          
        subFolderDataObject.isRelatedOnly = true;



        DataProperty dp11 = new DataProperty();
        dp11.columnName = "Name";
        dp11.propertyName = "Name";
        dp11.dataType = DataType.String;
        dp11.dataLength = 100;

        subFolderDataObject.dataProperties.Add(dp11);

        DataProperty dp12 = new DataProperty();
        dp12.columnName = "ID";
        dp12.propertyName = "ID";
        dp12.dataType = DataType.String;
        dp12.dataLength = 100;

        subFolderDataObject.dataProperties.Add(dp12);

        //DataProperty dp13 = new DataProperty();
        //dp13.columnName = "Path";
        //dp13.propertyName = "Path";
        //dp13.dataType = DataType.String;
        //dp13.dataLength = 1000;
        //subFolderDataObject.dataProperties.Add(dp13);

            subFolderDataObject.keyProperties = new List<KeyProperty>()
            {
              new KeyProperty()
              {
                keyPropertyName = "ID"
              }
            };

        dictionary.dataObjects.Add(subFolderDataObject);

          
        DataObject doc= new DataObject();
        
        doc.objectName = "Documents";
        doc.tableName = "Documents";

        doc.isRelatedOnly = true;



        DataProperty docp1 = new DataProperty();
        docp1.columnName = "Name";
        docp1.propertyName = "Name";
        docp1.dataType = DataType.String;
        docp1.dataLength = 100;

        doc.dataProperties.Add(docp1);

        DataProperty docp2 = new DataProperty();
        docp2.columnName = "ID";
        docp2.propertyName = "ID";
        docp2.dataType = DataType.String;
        docp2.dataLength = 100;

        doc.dataProperties.Add(docp2);

        DataProperty docp3 = new DataProperty();
        docp3.columnName = "Path";
        docp3.propertyName = "Path";
        docp3.dataType = DataType.String;
        docp3.dataLength = 1000;
        doc.dataProperties.Add(docp3);

        doc.keyProperties = new List<KeyProperty>()
            {
              new KeyProperty()
              {
                keyPropertyName = "ID"
              }
            };

        dictionary.dataObjects.Add(doc);
          //gagan

        DataObject folderDataObject = new DataObject();
        folderDataObject.objectName = "Folders";
        folderDataObject.tableName= "Folders";
        //folderDataObject.hasContent = false;
          


        DataProperty dp = new DataProperty();
        dp.columnName = "Name";
        dp.propertyName = "Name";
        dp.dataType = DataType.String;
        dp.dataLength = 100;

        folderDataObject.dataProperties.Add(dp);
        
        DataProperty dp1 = new DataProperty();
        dp1.columnName = "ID";
        dp1.propertyName = "ID";
        dp1.dataType = DataType.String;
        dp1.dataLength = 100;
         
        folderDataObject.dataProperties.Add(dp1);

        DataProperty dp2 = new DataProperty();
        dp2.columnName = "Path";
        dp2.propertyName = "Path";
        dp2.dataType = DataType.String;
        dp2.dataLength = 1000;
        folderDataObject.dataProperties.Add(dp2);

        folderDataObject.keyProperties = new List<KeyProperty>()
            {
              new KeyProperty()
              {
                keyPropertyName = "ID"
              }
            };

        DataRelationship dataRelation = new DataRelationship();
        dataRelation.relatedObjectName = "SubFolders";
        dataRelation.relationshipName = "SubFolders";
                  
        dataRelation.relationshipType = RelationshipType.OneToMany;

        PropertyMap propertyMap = new PropertyMap();
        propertyMap.dataPropertyName = "ID";
        propertyMap.relatedPropertyName = "ID";

        dataRelation.propertyMaps.Add(propertyMap);


        DataRelationship dataRelationDoc = new DataRelationship();
        dataRelationDoc.relatedObjectName = "Documents";
        dataRelationDoc.relationshipName = "Documents";
        

        dataRelationDoc.relationshipType = RelationshipType.OneToMany;

        PropertyMap propertyMapDoc = new PropertyMap();
        propertyMapDoc.dataPropertyName = "ID";
        propertyMapDoc.relatedPropertyName = "ID";

        dataRelationDoc.propertyMaps.Add(propertyMapDoc);


        
        dictionary.dataObjects.Add(folderDataObject);
        dictionary.dataObjects[3].dataRelationships.Add(dataRelation);
        dictionary.dataObjects[3].dataRelationships.Add(dataRelationDoc);   


       

        utility.Utility.Write<DataDictionary>(dictionary, path);

          
            dbDictionary = new DatabaseDictionary();
        
        
            dbDictionary.dataObjects = dictionary.dataObjects;
            dbDictionary.dataVersion = dictionary.dataVersion;
            dbDictionary.enableSearch = dictionary.enableSearch;
            dbDictionary.enableSummary = dictionary.enableSearch;
            dbDictionary.picklists = dictionary.picklists;
        
          

        

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
      /// <summary>
      /// The method mainly responsible to create dictionary and dictionary file name.
      /// </summary>
      /// <param name="tableName"></param>
      /// <returns></returns>
    public override Response RefreshDataTable(string tableName)
    {
        //_logger.Debug("In  RefreshDataTable()");
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
      /// <summary>
      /// Called from Iring and used to count the records from datatable.
      /// </summary>
      /// <param name="tableName"></param>
      /// <param name="whereClause"></param>
      /// <returns></returns>
    
    public override long GetCount(string tableName, string whereClause)
    {
      try
      {
          //_logger.Debug(" In GetCount()");
          if (tableName == "Folders" || tableName == "SubFolders")
          {

              return _iFoldrCount;
          }

          
        Login();

        DataTable dt = GetDocumentsForProject(
        _settings["PW.ProjectType"],
        _settings["PW.ProjectProperty"],
        _settings["PW.ProjectName"]);

        if (dt == null)
            return 0;


        if (whereClause != string.Empty && whereClause != null)
        {
            dt.DefaultView.RowFilter = whereClause != null ? whereClause.Replace("WHERE", "").Replace("UPPER", "") : "";
            
            
            return dt.DefaultView.ToTable().Rows.Count ;
        }
          
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
    private DataTable GetSubFolders(string whereClause)
    {
        DataTable dtsubFolderDetails;
        dtsubFolderDetails = GetDataTableSchema("subfolders");
        
         SortedList<int, string> lstSubFolder;

        string[] strFrag = whereClause.Split('=');
        string ID = strFrag[1];
        ID = ID.Replace("'", "");
        if (ID == string.Empty)
        {
            return dtsubFolderDetails;
        }
        if (whereClause == "from identifier")
             lstSubFolder = this.GetChildFolders(iParentFolder);
        else
            lstSubFolder = this.GetChildFolders(Convert.ToInt32(ID));

        foreach (var pair in lstSubFolder)
        {
            dtsubFolderDetails.Rows.Add(pair.Key, pair.Value);
        }
        _iFoldrCount = dtsubFolderDetails.Rows.Count;
        _settings["HasContentExpression"] = null;
        return dtsubFolderDetails;



        
    
    
    
    
    }



    private DataTable GetFolders()
    {
        DataTable dtFolderDetails;
        string setting = _settings["HasContentExpression"];
        List<string> strFolderPath = new List<string>();
        
        dtFolderDetails = GetDataTableSchema("Folders");


        SortedList<int, string> lstFolder = this.GetTopLevelFolders();
        foreach (var pair in lstFolder)
        {
            dtFolderDetails.Rows.Add(pair.Value, pair.Key, this.GetFolderPath(pair.Key));
        }

        _iFoldrCount = dtFolderDetails.Rows.Count;
        _settings["HasContentExpression"] = null;
        return dtFolderDetails;

    }
    private DataTable GetObjectDTP()
    {
        string orderBy = string.Empty;
        string strcolumnname = string.Empty;
        Login();

        DataTable dt = GetDocumentsForProject(
        _settings["PW.ProjectType"],
        _settings["PW.ProjectProperty"],
        _settings["PW.ProjectName"]);

        return dt;
        
    }
    /// <summary>
    /// This method used to create datatable with records on the bases of this GetCount method works.
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="whereClause"></param>
    /// <param name="start"></param>
    /// <param name="limit"></param>
    /// <returns></returns>
    public override DataTable GetDataTable(string tableName, string whereClause, long start, long limit)
    {
      
      try
      {
          DataTable dtglobal = new DataTable();
          
          if (tableName.ToUpper() == "FOLDERS")
          {
              DataTable dtFolders = GetFolders();

              if (whereClause != string.Empty)
              {
                  dtFolders.DefaultView.RowFilter = whereClause != null ? whereClause.Replace("WHERE", "").Replace("UPPER", "") : "";
                  _iFoldrCount = dtFolders.DefaultView.ToTable().Rows.Count;
                  return dtFolders.DefaultView.ToTable();

              }
              IEnumerable<System.Data.DataRow> query = from d in dtFolders.AsEnumerable().Skip((int)(start)).Take((int)limit) select d; //For Paging            
              if (query != null && query.Count() > 0)
              {
                  return query.CopyToDataTable();
              }
              return dtFolders;            
              

          }
          else if (tableName.ToUpper() == "SUBFOLDERS")
          {
              DataTable dtSubFolders = GetSubFolders(whereClause);
              return dtSubFolders;
          }
          else if (tableName.ToUpper() == "DOCUMENTS")
          {
              DataTable dtDocument = new DataTable();
              return dtDocument;
          }
          else if (tableName.ToUpper() == "DTP_ENG2")
          {
              DataTable dtObject = GetObjectDTP();
              if (whereClause != string.Empty)
              {
                  dtObject.DefaultView.RowFilter = whereClause != null ? whereClause.Replace("WHERE", "").Replace("UPPER", "") : "";
                  return dtObject.DefaultView.ToTable();
              }
              IEnumerable<System.Data.DataRow> query1 = from d in dtObject.AsEnumerable().Skip((int)(start)).Take((int)limit) select d; //For Paging
              if (query1 != null && query1.Count() > 0)
              {
                  return query1.CopyToDataTable();
              }
              return dtObject;
          }
          return dtglobal;
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
    /// <summary>
    /// Property only works for folder and subfolder object
    /// This is used to set and get the parent folder name.
    /// </summary>

    private  int iParentFolder
    {
        get
        {
            return this.iParentFolderID;
        }
        set
        {
            this.iParentFolderID = value;
        }
    }
    public override IList<IDataObject> Get(string objectType, IList<string> identifiers)
    {
        try
        {
            //_logger.Debug("In Get()"); 
            

            if (objectType.ToUpper() == "FOLDERS")
            {
                
                IList<IDataObject> dataObjects1 = new List<IDataObject>(); 
                DatabaseDictionary dictionary1 = GetDatabaseDictionary();
                iParentFolder = Convert.ToInt32(identifiers[0]); 
                DataObject objDef1 = dictionary1.dataObjects.Find(x => x.objectName.ToLower() == objectType.ToLower());
                List<string> strFolderPath = new List<string>();
                DataTable dtFolderDetails = GetFolders();
                string[] strArray = identifiers.ToArray<string>();
                dtFolderDetails.DefaultView.RowFilter = "ID = " + "'" + strArray[0] + "'";
                _iFoldrCount = dtFolderDetails.DefaultView.ToTable().Rows.Count;
                if (_iFoldrCount == 0)
                {
                    DataRow dr1 = dtFolderDetails.NewRow();
                    dr1["Name"]=string.Empty;
                    dr1["ID"] = string.Empty;
                    dr1["Path"] = string.Empty;
            

             
                    _settings["HasContentExpression"] = null;
                    IDataObject dataObject11 = ToDataObject(dr1, objDef1);
                    dataObjects1.Add(dataObject11);
                    return dataObjects1;
                }
                    
                DataRow dr = dtFolderDetails.DefaultView.ToTable().Rows[0];
                _settings["HasContentExpression"] = null;
                IDataObject dataObject = ToDataObject(dr, objDef1);

               
                dataObjects1.Add(dataObject);
                
                return dataObjects1;
            
            }
            
            else if (objectType.ToUpper() == "SUBFOLDERS")
            {
                //from identifier

                
                IList<IDataObject> dataObjects1 = new List<IDataObject>();
                DatabaseDictionary dictionary1 = GetDatabaseDictionary();
                DataObject objDef1 = dictionary1.dataObjects.Find(x => x.objectName.ToLower() == objectType.ToLower());

                List<string> strFolderPath = new List<string>();
                DataTable dtSubFolders = GetSubFolders("from identifier");
                dtSubFolders.DefaultView.RowFilter = "ID = " + "'" + identifiers[0] + "'";
                _iFoldrCount = dtSubFolders.DefaultView.ToTable().Rows.Count;

                if (_iFoldrCount == 0)
                {
                    DataRow dr1 = dtSubFolders.NewRow();
                   dr1["ID"] = string.Empty;
                   dr1["Name"] = string.Empty;
                   
                    _settings["HasContentExpression"] = null;
                    IDataObject dataObject11 = ToDataObject(dr1, objDef1);
                    dataObjects1.Add(dataObject11);
                    return dataObjects1;
                }

                DataRow dr = dtSubFolders.DefaultView.ToTable().Rows[0];
                _settings["HasContentExpression"] = null;
                IDataObject dataObject = ToDataObject(dr, objDef1);

                
                dataObjects1.Add(dataObject);

                return dataObjects1;
            }


           else if (objectType.ToUpper() == "DOCUMENTS")
            {
                IList<IDataObject> dataObjects1 = new List<IDataObject>();
                 List<string> _identifiers = new List<string>();
                DatabaseDictionary dictionary1 = GetDatabaseDictionary();
                DataObject objDef1 = dictionary1.dataObjects.Find(x => x.objectName.ToLower() == objectType.ToLower());

                List<string> strFolderPath = new List<string>();
                DataTable dtDocumentDetails;

                
                _identifiers .Add(identifiers[0]);
                dtDocumentDetails = GetDocumentsForFolder(iParentFolder, _identifiers);
                _iFoldrCount = dtDocumentDetails.DefaultView.ToTable().Rows.Count;

                if (_iFoldrCount == 0)
                {
                    //DataRow dr1 = dtDocumentDetails.NewRow();
                   // DataRow dr1 = dtDocumentDetails.Rows[0];
                    DataRow dr1 = null ;
                    //dr1[0] = string.Empty;
                    //dr1[1] = string.Empty;

                    _settings["HasContentExpression"] = null;
                    IDataObject dataObject11 = ToDataObject(dr1, objDef1);
                    dataObjects1.Add(dataObject11);
                    return dataObjects1;
                }

                DataRow dr = dtDocumentDetails.DefaultView.ToTable().Rows[0];
                _settings["HasContentExpression"] = null;
                IDataObject dataObject = ToDataObject(dr, objDef1);


                dataObjects1.Add(dataObject);

                return dataObjects1;




                
            }
            else if (objectType.ToUpper() == "DTP_ENG2")
            {

                List<string> docGuids = identifiers.ToList();
                List<string> listAttributes = new List<string>();


                DatabaseDictionary dictionary = GetDatabaseDictionary();
                DataObject objDef = dictionary.dataObjects.Find(x => x.objectName.ToLower() == objectType.ToLower());
                foreach (DataProperty prop in objDef.dataProperties)
                {
                    listAttributes.Add(prop.columnName);
                }

                Login();

                DataTable dtDoc = GetDocumentMetadata(docGuids, listAttributes);

                if (dtDoc == null)
                {
                    throw new Exception("Objects not found.");
                }

                IList<IDataObject> dataObjects = new List<IDataObject>();
                bool includeContent = _settings["IncludeContent"] != null && bool.Parse(_settings["IncludeContent"].ToString());

                foreach (DataRow row in dtDoc.Rows)
                {
                    IDataObject dataObject = ToDataObject(row, objDef);

                    if (includeContent)
                    {
                        IContentObject contentObject = new GenericContentObject(dataObject);
                        contentObject.ObjectType = objectType;


                        string id = _sDocumentGUID;
                        string tempFoder = "c:\\temp\\projectwise\\";

                        FileStream stream = GetProjectWiseFile(id, tempFoder);

                        if (stream != null)
                        {
                            try
                            {

                                string format = _sFileFormat;

                                string docName = stream.Name.ToLower();
                                int extIndex = docName.LastIndexOf('.');

                                if (extIndex >= 0)
                                {
                                    format = docName.Substring(extIndex);
                                }

                                string contentType = MimeTypes.Dictionary[format];

                                MemoryStream outStream = new MemoryStream();
                                stream.CopyTo(outStream);
                                outStream.Position = 0;
                                stream.Close();

                                contentObject.Content = outStream;
                            }
                            catch (Exception ex)
                            {
                                _logger.Error("Error getting content type: " + ex.ToString());
                                throw ex;
                            }
                            finally
                            {
                                stream.Close();
                            }
                        }

                        dataObjects.Add(contentObject);
                    }
                    else
                    {
                        dataObjects.Add(dataObject);
                    }
                }

                return dataObjects;
            }
            IList<IDataObject> dataObjectsGlbl = new List<IDataObject>();
            return dataObjectsGlbl;
            
            

            
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

    


    public override DataTable GetDataTable(string tableName, IList<string> identifiers)
    {
      try
      {
          List<string> docGuids = identifiers.ToList();
          List<string> listAttributes = new List<string>();
          DatabaseDictionary dictionary = GetDatabaseDictionary();
          DataObject objDef = dictionary.dataObjects.Find(x => x.tableName.ToLower() == tableName.ToLower());
          foreach (DataProperty prop in objDef.dataProperties)
          {
              listAttributes.Add(prop.columnName);
              
          }
          Login();
          //if (tableName == "Folders")
          //{
          //    string setting = _settings["HasContentExpression"];
          //    List<string> strFolderPath = new List<string>();
          //    DataTable dtFolderDetails;

          //    dtFolderDetails = new DataTable();
          //    dtFolderDetails.Columns.Add("Name");
          //    dtFolderDetails.Columns.Add("ID");
          //    dtFolderDetails.Columns.Add("Path");

          //    SortedList<int, string> lstFolder = this.GetTopLevelFolders();
          //    foreach (var pair in lstFolder)
          //    {
          //        dtFolderDetails.Rows.Add(pair.Value, pair.Key, this.GetFolderPath(pair.Key));

          //    }
          //    string[] strArray = identifiers.ToArray<string>();
          //    dtFolderDetails.DefaultView.RowFilter = "ID = " + "'" + strArray[0] + "'";
          //    _iFoldrCount = dtFolderDetails.DefaultView.ToTable().Rows.Count;
             
          //    return  dtFolderDetails.DefaultView.ToTable();
                
          //}

       

       

       

       

        DataTable dt = GetDocumentMetadata(docGuids, listAttributes);
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

    public override IList<IContentObject> GetContents(string objectType, IDictionary<string, string> idFormats)
    {
      try
      {
        IList<IContentObject> contentObjects = new List<IContentObject>();

        string contentType = "application/msword";
        string tempFoder = "c:\\temp\\projectwise\\";

        List<string> listAttributes = new List<string>();
        DatabaseDictionary dictionary = GetDatabaseDictionary();
        DataObject objDef = dictionary.dataObjects.Find(x => x.objectName.ToLower() == objectType.ToLower());

        foreach (DataProperty prop in objDef.dataProperties)
        {
          listAttributes.Add(prop.columnName);
        }

        Login();

        foreach (var pair in idFormats)
        {
          string id = pair.Key;
          string format = pair.Value;

          IContentObject contentObject = new GenericContentObject()
          {
            ObjectType = objectType,
            Identifier = id,
          };

          contentObjects.Add(contentObject);

          #region get meta data
          DataTable dt = GetDocumentMetadata(id, listAttributes);
          if (dt == null)
          {
            throw new Exception("Object [" + id + "] does not exist.");
          }

          if (dt.Rows.Count > 0)
          {
            IDataObject dataObject = ToDataObject(dt.Rows[0], objDef);
          }
          #endregion

          #region get content
          FileStream stream = GetProjectWiseFile(id, tempFoder);
          if (stream != null)
          {
            try
            {
              if (string.IsNullOrEmpty(format))
              {
                string docName = stream.Name.ToLower();
                int extIndex = docName.LastIndexOf('.');

                if (extIndex < 0)
                {
                 
                    format = _sFileFormat; 
                }
                else
                {
                  format = docName.Substring(extIndex);
                }
              }
              else if (!format.StartsWith("."))
              {
                format = "." + format;
              }

              contentType = MimeTypes.Dictionary[format];

              MemoryStream outStream = new MemoryStream();
              stream.CopyTo(outStream);
              outStream.Position = 0;
              stream.Close();

              contentObject.Content = outStream;
              contentObject.ContentType = contentType;
            }
            catch (Exception ex)
            {
              _logger.Error("Error getting content type: " + ex.ToString());
              throw ex;
            }
            finally
            {
              stream.Close();
            }
          }
          #endregion
        }

        return contentObjects;
      }
      catch (Exception ex)
      {
        //_logger.Debug("Error getting contents: " + ex.ToString());
        throw ex;
      }
      finally
      {
        Logout();
      }
    }
    
    //TODO: look up dictionary for object type and handle list
    //public override IList<IDataObject> Create(string objectType, IList<string> identifiers)
    //{
    //  IList<IDataObject> dataObjects = new List<IDataObject>();

    //  if (identifiers.Count == 1)
    //  {
    //    dataObjects.Add(new GenericDataObject() { ObjectType = objectType });
    //  }

    //  return dataObjects;
    //}
    public override IList<IDataObject> Create(string objectType, IList<string> identifiers)
    {
        IList<IDataObject> dataObjects = new List<IDataObject>();
        DatabaseDictionary dictionary = GetDatabaseDictionary();
        int icounter = 0;
        int iInnerCounter = 0;
        string[] idParts= new string[100] ;

        if (identifiers.Count == 1)
        {
            dataObjects.Add(new GenericDataObject() { ObjectType = objectType });
        }

        foreach (string identifier in identifiers)
        {
            IDataObject obj = new GenericDataObject() { ObjectType = objectType };


            var dataObject = (from o in dictionary.dataObjects
                              where o.objectName == objectType
                              select o).ToList().First();


            if (dataObject.keyDelimeter != string.Empty && dataObject.keyDelimeter != null)
            {


                string[] delim = new string[] { dataObject.keyDelimeter };
                idParts = identifier.Split(delim, StringSplitOptions.None);
                icounter = idParts.Length;
            }
            foreach (DataProperty dp in dataObject.dataProperties)
            {

                if (dataObject.isKeyProperty(dp.propertyName))
                {
                    if (dataObject.keyDelimeter != string.Empty &&  dataObject.keyDelimeter != null && iInnerCounter < icounter)
                    {
                        obj.SetPropertyValue(dp.propertyName, idParts[iInnerCounter]);
                        iInnerCounter = iInnerCounter + 1;
                    }
                    else
                        obj.SetPropertyValue(dp.propertyName, identifier);
                }
                else
                {
                    obj.SetPropertyValue(dp.propertyName, null);
                }
            }

            // obj.SetPropertyValue("keyfield", identifier);
        }

        return dataObjects;
    }
    private DataTable GetDataTableSchema(string objectType)
    {
        DatabaseDictionary dictionary = GetDatabaseDictionary();
        DataObject objDef = dictionary.dataObjects.Find(p => p.objectName.ToUpper() == objectType.ToUpper());
        DataTable dataTable = new DataTable();
        dataTable.TableName = objectType;
        foreach (DataProperty property in objDef.dataProperties)
        {
            DataColumn dataColumn = new DataColumn();
            dataColumn.ColumnName = property.columnName;
            dataColumn.DataType = Type.GetType("System." + property.dataType.ToString());
            dataTable.Columns.Add(dataColumn);
        }


        return dataTable;
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

          Stream content = null;

          if (typeof(IContentObject).IsAssignableFrom(dataObject.GetType()))
          {
            content = ((IContentObject)dataObject).Content;
          }

          string docGUID = CreateNewPWDocument(content, slProps);

          Status status = new Status()
          {
            Identifier = docGUID,
            Messages = new Messages { "saved successfully." }
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
      
    //public override Response PostContents(IList<IContentObject> contentObjects)
    //{
    //    Response response = new Response();

    //    try
    //    {
    //        DatabaseDictionary dictionary = GetDatabaseDictionary();

    //        Login();

    //        foreach (IContentObject contentObject in contentObjects)
    //        {
    //            DataObject objDef = dictionary.dataObjects.Find(x => x.objectName.ToLower() == contentObject.ObjectType.ToLower());
    //            SortedList<string, string> slProps = new SortedList<string, string>();

    //            foreach (DataProperty prop in objDef.dataProperties)
    //            {
    //                object propValue = null;

    //                try
    //                {
    //                    propValue = contentObject.DataObject.GetPropertyValue(prop.propertyName);
    //                }
    //                catch (Exception) { }

    //                if (propValue != null)
    //                {
    //                    slProps.Add(prop.columnName, propValue.ToString());
    //                }
    //            }

    //            slProps.Add("ProjectWiseFolderPath", "Bechtel Sample Project\\" + Guid.NewGuid().ToString());

    //            if (contentObject.Content != null)
    //            {
    //                //TODO: validate DocumentName in the property list
    //                string docGUID = CreateNewPWDocument(contentObject.Content, slProps);

    //                Status status = new Status()
    //                {
    //                    Identifier = docGUID,
    //                    Messages = new Messages { docGUID + " saved successfully." }
    //                };

    //                response.StatusList.Add(status);
    //            }
    //        }
    //    }
    //    catch (Exception e)
    //    {
    //        _logger.Error(e.Message);
    //        response.Level = StatusLevel.Error;
    //        response.Messages.Add(e.Message);
    //    }
    //    finally
    //    {
    //        Logout();
    //    }

    //    return response;
    //}


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
        //_logger.Debug("In login()");
        string currDir = Directory.GetCurrentDirectory();

        try
        {
            /// If user name and password are null, process creds will be used
            return PWWrapper.aaApi_Login(PWWrapper.DataSourceType.Unknown, _sDatasource, _sUserName, _sPassword, null, true);
        }
        finally
        {
            Directory.SetCurrentDirectory(currDir);
        }
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
        //_logger.Debug("In  GetEnvironments()");

      DataTable dt = GetGenericDocumentProperties();
      List<DataProperty> props = new List<DataProperty>();

      foreach (DataColumn dc in dt.Columns)
      {
        
        Type dataType =(Type)dc.DataType; 

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
          //_logger.Debug("In GetEnvironments()");
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

    //private SortedList<string, SortedList<string, TypeAndLength>> GetProjectTypes()
    //{
    //  //SortedList<int, string> slProjectTypes = ECComponentAssembly.ODSDataWrapper.GetListOfProjectTypesByIds();

    //  SortedList<int, string> slProjectTypes = GetListOfProjectTypesByIds();
    //  SortedList<string, SortedList<string, TypeAndLength>> slProjectsAndProperties = new SortedList<string, SortedList<string, TypeAndLength>>(StringComparer.CurrentCultureIgnoreCase);

    //  if (slProjectTypes.Count == 0)
    //  {
    //    BPSUtilities.WriteLog("No rich project types found");
    //    return slProjectsAndProperties;
    //  }

    //  foreach (int iClassId in slProjectTypes.Keys)
    //  {
    //    if (!slProjectsAndProperties.ContainsKey(slProjectTypes[iClassId]))
    //    {
    //      SortedList<string, int> slProperties = PWWrapper.GetClassPropertyIdsInList(iClassId);

    //      SortedList<string, TypeAndLength> slPropNamesTypesAndLengths = new SortedList<string, TypeAndLength>(StringComparer.CurrentCultureIgnoreCase);

    //      foreach (KeyValuePair<string, int> kvp in slProperties)
    //      {
    //        TypeAndLength typeAndLength = GetAttributeDataTypeAndLength(kvp.Value);

    //        if (!slPropNamesTypesAndLengths.ContainsKey(kvp.Key))
    //          slPropNamesTypesAndLengths.Add(kvp.Key, typeAndLength);
    //      }

    //      slProjectsAndProperties.Add(slProjectTypes[iClassId], slPropNamesTypesAndLengths);
    //    }
    //  }

    //  return slProjectsAndProperties;
    //}

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

   

    private System.IO.FileStream GetProjectWiseFile(string sDocumentGuid, string sWorkingDirectory)
    {
      try
      {
          //_logger.Debug("In   GetProjectWiseFile()");
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
        //_logger.Debug(" In GetDocumentsForProject()");
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

    private DataTable GetDocumentMetadata(string docGuid, List<string> listAttributes)
    {
      SavedSearchAssembly.SavedSearchWrapper.InitializeQuery(0, false);
      SavedSearchAssembly.SavedSearchWrapper.AddDocumentGuidCriterion(docGuid, 0);
      DataTable dt = SavedSearchAssembly.SavedSearchWrapper.Search(listAttributes);
      return dt;
    }

    private DataTable GetDocumentMetadata(List<string> docGuids, List<string> listAttributes)
    {
        //_logger.Debug("In GetDocumentMetadata()");
      DataTable dtReturn = new DataTable();

      foreach (string docGuid in docGuids)
      {
        SavedSearchAssembly.SavedSearchWrapper.InitializeQuery(0, false);
        SavedSearchAssembly.SavedSearchWrapper.AddDocumentGuidCriterion(docGuid, 0);

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

   
      
    public SortedList<int, string> GetTopLevelFolders()
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

    public SortedList<int, string> GetChildFolders(int iParentId)
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
      
    private  DataTable GetDocumentsForFolder(int iFolderId, List<string> listAttributes)
    {
      DataTable dtReturn = new DataTable();

      SavedSearchAssembly.SavedSearchWrapper.InitializeQuery(iFolderId, true);
      // SavedSearchAssembly.SavedSearchWrapper.AddOriginalsOnlyCriterion(0);
      DataTable dt = SavedSearchAssembly.SavedSearchWrapper.Search(listAttributes);


        //DataTable  dt1 = SavedSearchAssembly.SavedSearchWrapper.Search();
        //int iTargetDocumentId = (int)dt.Rows[0]["DocumentId"];
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
