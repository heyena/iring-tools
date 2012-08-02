using System;
using System.Data;
using System.Collections;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq.Expressions;
using System.Xml.Linq;
using Ciloci.Flee;
using log4net;
using Ninject;
using org.iringtools.adapter;
using org.iringtools.library;
using org.iringtools.utility;
using StaticDust.Configuration;
using Ingr.SP3D.Common.Middle.Services;
using Ingr.SP3D.Common.Middle;
using Ingr.SP3D.Structure.Middle;
using Ingr.SP3D.ReferenceData.Middle;
using Ingr.SP3D.Systems.Middle;
using Ingr.SP3D.ReferenceData.Middle.Services;

namespace iringtools.sdk.sp3ddatalayer
{
  public class SP3DDataLayer : BaseDataLayer
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(SP3DDataLayer));
    private string _dataPath = string.Empty;
    private string _scope = string.Empty;
    private string _dictionaryPath = string.Empty;
    private BusinessObjectConfiguration _configuration = null;
    private Dictionary<string, Configuration> _configs = null;
    private string _communityName = string.Empty;
    private DataDictionary _dictionary = null;

    [Inject]
    public SP3DDataLayer(AdapterSettings settings)
      : base(settings)
    {
       _dataPath = settings["DataLayerPath"];
        if (_dataPath == null)
        {
          _dataPath = settings["AppDataPath"];
        }

        _scope = _settings["ProjectName"] + "." + _settings["ApplicationName"];

        string appSettingsPath = string.Format("{0}{1}.config", _dataPath, _scope);
        if (!System.IO.File.Exists(appSettingsPath))
        {
          _dataPath += "App_Data\\";
          appSettingsPath = string.Format("{0}{1}.config", _dataPath, _scope);
        }
        _settings.AppendSettings(new AppSettingsReader(appSettingsPath));

        _dictionaryPath = string.Format("{0}DataDictionary.{1}.xml", _dataPath, _scope);
    }

    public override DataDictionary GetDictionary()
    {
      if (_dictionary != null)
        return _dictionary;

      try
      {
        getConfigure();
        _dictionary = new DataDictionary();
        _dictionary.dataObjects = new List<DataObject>();
        foreach (BusinessObject businessObject in _configuration.businessObjects)
        {
          DataObject dataObject = CreateDataObject(businessObject);
          _dictionary.dataObjects.Add(dataObject);
        }        

        return _dictionary;
      }
      catch (Exception ex)
      {
        _logger.Error("connect SP3D: " + ex.ToString());
        throw ex;
      }
    }

    private DataObject GetDataObject(string objectName)
    {
      DataDictionary dictionary = GetDictionary();
      DataObject dataObject = dictionary.dataObjects.Find(x => x.objectName.ToLower() == objectName.ToLower());
      return dataObject;
    }

    private DataObject CreateDataObject(BusinessObject businessObject)
    {
      string propertyName = string.Empty;
      string relatedPropertyName = string.Empty;
      DataObject dataObject = new DataObject();
      string objectName = businessObject.objectName;
      dataObject.objectName = objectName;
      dataObject.tableName = objectName;
      dataObject.keyProperties = new List<KeyProperty>();
      dataObject.dataProperties = new List<DataProperty>();
      dataObject.dataRelationships = new List<DataRelationship>();

      if (businessObject.dataFilter != null)
        dataObject.dataFilter = businessObject.dataFilter;

      foreach (BusinessProperty businessProperty in businessObject.businessProperties)
      {
        DataProperty dataProperty = new DataProperty();
        propertyName = businessProperty.propertyName;
        dataProperty.columnName = propertyName;
        dataProperty.propertyName = propertyName;

        if (businessProperty.isNullable != null)
          dataProperty.isNullable = businessProperty.isNullable;

        if (businessProperty.isReadOnly != null)
          dataProperty.isReadOnly = businessObject.isReadOnly;

        if (businessProperty.description != null)
          dataProperty.description = businessObject.description;

        dataObject.dataProperties.Add(dataProperty);
      }

      foreach (BusinessRelationship businessRelationship in businessObject.businessRelationships)
      {
        DataRelationship dataRelationship = new DataRelationship();
        dataRelationship.relatedObjectName = businessRelationship.relatedObjectName;
        dataRelationship.relationshipName = businessRelationship.relationshipName;
        dataRelationship.propertyMaps = new List<PropertyMap>();
        
        if (businessRelationship.BusinessRelatedProperties != null)
          foreach (BusinessRelatedProperty businessRelationProperty in businessRelationship.BusinessRelatedProperties)
          {
            DataProperty dataProperty = new DataProperty();
            PropertyMap propertyMap = new PropertyMap();
            relatedPropertyName = businessRelationProperty.relatedPropertyName;
            propertyMap.relatedPropertyName = relatedPropertyName;
            dataProperty.propertyName = relatedPropertyName;
            dataRelationship.propertyMaps.Add(propertyMap);
            dataObject.dataProperties.Add(dataProperty);
          }    
      }
      return dataObject;
    }

    private void getConfigure()
    {
      string configurationPath = string.Format("{0}{1}.configuration", _dataPath, _scope);
      _configuration = Utility.Read<BusinessObjectConfiguration>(configurationPath);
    }

    public override IList<IDataObject> Get(string objectType, DataFilter filter, int pageSize, int startIndex)
    {
      IList<IDataObject> dataObjects = new List<IDataObject>();

      try
      {
        Connect();

        DataObject dataObject = GetDataObject(objectType);

        ReadOnlyDictionary<ClassInformation> classInfoDictionary = new ReadOnlyDictionary<ClassInformation>(); 



        // Apply filter
        if (filter != null && filter.Expressions != null && filter.Expressions.Count > 0)
        {
          var predicate = filter.ToPredicate(_dataObjectDefinition);

          if (predicate != null)
          {
            _dataObjects = allDataObjects.AsQueryable().Where(predicate).ToList();
          }
        }

        if (filter != null && filter.OrderExpressions != null && filter.OrderExpressions.Count > 0)
        {
          throw new NotImplementedException("OrderExpressions are not supported by the SPPID DataLayer.");
        }

        //Page and Sort The Data
        if (pageSize > _dataObjects.Count())
          pageSize = _dataObjects.Count();
        _dataObjects = _dataObjects.GetRange(startIndex, pageSize);

        return _dataObjects;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetList: " + ex);

        throw new Exception(
          "Error while getting a list of data objects of type [" + objectType + "].",
          ex
        );
      }
    }

    private void Connect()
    {
      Site SP3DSite = null;
      SP3DSite = MiddleServiceProvider.SiteMgr.ConnectSite();
      
      if (SP3DSite != null)
      {
        if( SP3DSite.Plants.Count > 0 )
          MiddleServiceProvider.SiteMgr.ActiveSite.OpenPlant((Plant)SP3DSite.Plants[0]);
      }

      Model SP3DModel = null;
      SP3DModel = MiddleServiceProvider.SiteMgr.ActiveSite.ActivePlant.PlantModel;
      MetadataManager metadataManager = SP3DModel.MetadataMgr;

      string displayName, name, showupMsg = "", category, iid, interfaceInfoNamespace, propertyName, propertyDescriber;
      string propertyInterfaceInformationStr, unitTypeString;
      Type type;
      ReadOnlyDictionary<InterfaceInformation> interfactInfo, commonInterfaceInfo;
      ReadOnlyDictionary<PropertyInformation> properties;
      ReadOnlyDictionary<BOCInformation> oSystemsByName = metadataManager.BOCs;
      bool complex, comAccess, displayedOnPage, isvalueRequired, metaDataAccess, metadataReadOnly, SqlAccess;
      string propertyDisplayName, proPropertyName, uomType;
      CodelistInformation codeListInfo;
      InterfaceInformation propertyInterfaceInformation;
      SP3DPropType sp3dProType;
      UnitType unitType;
      string showupPropertyMessage = "";
      string showupProMsg = "";
      foreach (string key in oSystemsByName.Keys)
      {
        BOCInformation bocInfo = null;
        oSystemsByName.TryGetValue(key, out bocInfo);
        displayName = bocInfo.DisplayName;
        name = bocInfo.Name;
        type = bocInfo.GetType();
        interfactInfo = bocInfo.DefiningInterfaces;
        foreach (string infoKey in interfactInfo.Keys)
        {
          InterfaceInformation itemInterfaceInfo;
          interfactInfo.TryGetValue(infoKey, out itemInterfaceInfo);
          interfaceInfoNamespace = itemInterfaceInfo.Namespace;
          category = itemInterfaceInfo.Category;
          iid = itemInterfaceInfo.IID;
          properties = itemInterfaceInfo.Properties;

          foreach (string propertyKey in properties.Keys)
          {
            PropertyInformation propertyInfo;
            properties.TryGetValue(propertyKey, out propertyInfo);
            complex = propertyInfo.Complex;

            codeListInfo = propertyInfo.CodeListInfo;
            comAccess = propertyInfo.COMAccess;
            displayedOnPage = propertyInfo.DisplayedOnPropertyPage;
            propertyDisplayName = propertyInfo.DisplayName;
            propertyInterfaceInformation = propertyInfo.InterfaceInfo;
            propertyInterfaceInformationStr = propertyInterfaceInformation.ToString();
            isvalueRequired = propertyInfo.IsValueRequired;
            metaDataAccess = propertyInfo.MetadataAccess;
            metadataReadOnly = propertyInfo.MetadataReadOnly;
            proPropertyName = propertyInfo.Name;
            sp3dProType = propertyInfo.PropertyType;
            SqlAccess = propertyInfo.SQLAccess;
            unitType = propertyInfo.UOMType;
            unitTypeString = unitType.ToString();

            showupPropertyMessage = showupPropertyMessage + "\n propertyInfo.key: " + propertyKey + "\n"
                                  + "CodeListInfo.DisplayName: " + codeListInfo.DisplayName + "\n"
                                  + "comAccess: " + comAccess + "\n"
                                  + "propertyDisplayName: " + propertyDisplayName + "\n"
                                  + "propertyInterfaceInformation: " + propertyInterfaceInformation.Name + "\n"
                                  + "proPropertyName: " + proPropertyName;


          }
        }

        commonInterfaceInfo = bocInfo.CommonInterfaces;
        foreach (string comInfoKey in commonInterfaceInfo.Keys)
        {
          InterfaceInformation comItemInterfaceInfo;
          commonInterfaceInfo.TryGetValue(comInfoKey, out comItemInterfaceInfo);
          interfaceInfoNamespace = comItemInterfaceInfo.Namespace;
          category = comItemInterfaceInfo.Category;
          iid = comItemInterfaceInfo.IID;
          properties = comItemInterfaceInfo.Properties;
          
          foreach (string propertyKey in properties.Keys)
          {
            PropertyInformation propertyInfo;
            properties.TryGetValue(propertyKey, out propertyInfo);
            complex = propertyInfo.Complex;
           
            codeListInfo = propertyInfo.CodeListInfo;
            comAccess = propertyInfo.COMAccess;
            displayedOnPage = propertyInfo.DisplayedOnPropertyPage;
            propertyDisplayName = propertyInfo.DisplayName;
            propertyInterfaceInformation = propertyInfo.InterfaceInfo;
            propertyInterfaceInformationStr = propertyInterfaceInformation.ToString();
            isvalueRequired = propertyInfo.IsValueRequired;
            metaDataAccess = propertyInfo.MetadataAccess;
            metadataReadOnly = propertyInfo.MetadataReadOnly;
            proPropertyName = propertyInfo.Name;
            sp3dProType = propertyInfo.PropertyType;
            SqlAccess = propertyInfo.SQLAccess;
            unitType = propertyInfo.UOMType;
            unitTypeString = unitType.ToString();

            showupProMsg = showupProMsg + "\n propertyInfo.key: " + propertyKey + "\n"
                                  + "CodeListInfo.DisplayName: " + codeListInfo.DisplayName + "\n"
                                  + "comAccess: " + comAccess + "\n"
                                  + "propertyDisplayName: " + propertyDisplayName + "\n"
                                  + "propertyInterfaceInformation: " + propertyInterfaceInformation.Name + "\n"
                                  + "proPropertyName: " + proPropertyName;          


          }


        }
        
        showupMsg = showupMsg + "\n bocInfo.key: " + key + "\n"
                  + "bocInfo.DisplayName: " + displayName + "\n"
                  + "bocInfo.Name: " + name + "\n"
                  + "bocInfo.type: " + type.FullName + "\n"
                 // + "bocInfo.DefiningInterfaces: " + showupPropertyMessage + "\n"
                  + "bocInfo.commonInterfaceInfo: " + showupProMsg + "\n";                 

      }
      File.WriteAllText(@"C:\temp\sp3d.txt", showupMsg);

      //System.Windows.Forms.MessageBox.Show(showupMsg);
      //oSystemsByName
    }

    public override long GetCount(string objectType, DataFilter filter)
    {
            try
            {
              Connect();

              //NOTE: pageSize of 0 indicates that all rows should be returned.
              //IList<IDataObject> dataObjects = Get(objectType, filter, 0, 0);

                //return dataObjects.Count();
              return 5;
            }
            catch (Exception ex)
            {
                _logger.Error("Error in GetIdentifiers: " + ex);

                throw new Exception(
                  "Error while getting a count of type [" + objectType + "].",
                  ex
                );
            }
        }

        public override IList<string> GetIdentifiers(string objectType, DataFilter filter)
        {
          throw new Exception("Error while getting a count of type ");
            //try
            //{
            //    List<string> identifiers = new List<string>();

            //    //NOTE: pageSize of 0 indicates that all rows should be returned.
            //    IList<IDataObject> dataObjects = Get(objectType, filter, 0, 0);

            //    foreach (IDataObject dataObject in dataObjects)
            //    {
            //        identifiers.Add((string)dataObject.GetPropertyValue("Tag"));
            //    }

            //    return identifiers;
            //}
            //catch (Exception ex)
            //{
            //    _logger.Error("Error in GetIdentifiers: " + ex);

            //    throw new Exception(
            //      "Error while getting a list of identifiers of type [" + objectType + "].",
            //      ex
            //    );
            //}
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

              EqlClient eql = new EqlClient();
              int objectId = eql.GetObjectId(keyValue, revision, config.Template.ObjectType);
              iringtools.sdk.sp3ddatalayer.Template template = config.Template;

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

                objectId = 0;
              }

              

              
                Status st = new Status()
                {
                  Identifier = keyValue,
                  Level = StatusLevel.Error,
                  Messages = new Messages() { string.Format("Object type [{0}] not supported.", template.ObjectType) }
                };

                response.StatusList.Add(st);
             
            }
          }
          catch (Exception e)
          {
            _logger.Error("Error posting data objects: " + e);

            response.Level = StatusLevel.Error;
            response.Messages.Add("Error posting data objects: " + e);
          }
          //finally
          //{
          //  Disconnect();
          //}

          return response;
        }

        private string GetTemplateName(iringtools.sdk.sp3ddatalayer.Template template, DataObject objectDefinition, IDataObject dataObject)
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

        public override IList<IDataObject> GetRelatedObjects(IDataObject dataObject, string relatedObjectType)
        {
            throw new NotImplementedException();
        }        

        public override Response Delete(string objectType, IList<string> identifiers)
        {
          throw new Exception("Error while getting a count of type ");
          //// Not gonna do it. Wouldn't be prudent.
          //Response response = new Response();
          //Status status = new Status();
          //status.Level = StatusLevel.Error;
          //status.Messages.Add("Delete not supported by the SP3D DataLayer.");
          //response.Append(status);
          //return response;
        }

        public override Response Delete(string objectType, DataFilter filter)
        {
          throw new Exception("Error while getting a count of type ");
          //// Not gonna do it. Wouldn't be prudent with a filter either.
          //Response response = new Response();
          //Status status = new Status();
          //status.Level = StatusLevel.Error;
          //status.Messages.Add("Delete not supported by the SP3D DataLayer.");
          //response.Append(status);
          //return response;
        }     

        private void LoadConfiguration()
        {
            if (_configuration == null)
            {
                string uri = String.Format(
                    "{0}Configuration.{1}.xml",
                    _settings["XmlPath"],
                    _settings["ApplicationName"]
                );

                XDocument configDocument = XDocument.Load(uri);
                _configuration = configDocument.Element("configuration");
            }
        }

        private IList<IDataObject> LoadDataObjects(string objectType)
        {
            try
            {
                IList<IDataObject> dataObjects = new List<IDataObject>();

                //Get Path from Scope.config ({project}.{app}.config)
                string path = String.Format(
                    "{0}{1}\\{2}.csv",
                     _settings["BaseDirectoryPath"],
                    _settings["SP3DFolderPath"],
                    objectType
                );

                IDataObject dataObject = null;
                TextReader reader = new StreamReader(path);
                while (reader.Peek() >= 0)
                {
                    string csvRow = reader.ReadLine();

                    dataObject = FormDataObject(objectType, csvRow);

                    if (dataObject != null)
                        dataObjects.Add(dataObject);
                }
                reader.Close();

                return dataObjects;
            }
            catch (Exception ex)
            {
                _logger.Error("Error in LoadDataObjects: " + ex);
                throw new Exception("Error while loading data objects of type [" + objectType + "].", ex);
            }
        }

        private IDataObject FormDataObject(string objectType, string csvRow)
        {
            try
            {
                IDataObject dataObject = new GenericDataObject
                {
                    ObjectType = objectType,
                };

                XElement commodityElement = GetCommodityConfig(objectType);

                if (!String.IsNullOrEmpty(csvRow))
                {
                    IEnumerable<XElement> attributeElements = commodityElement.Element("attributes").Elements("attribute");

                    string[] csvValues = csvRow.Split(',');

                    int index = 0;
                    foreach (var attributeElement in attributeElements)
                    {
                        string name = attributeElement.Attribute("name").Value;
                        string dataType = attributeElement.Attribute("dataType").Value.ToLower();
                        string value = csvValues[index++].Trim();

                        // if data type is not nullable, make sure it has a value
                        if (!(dataType.EndsWith("?") && value == String.Empty))
                        {
                            if (dataType.Contains("bool"))
                            {
                                if (value.ToUpper() == "TRUE" || value.ToUpper() == "YES")
                                {
                                    value = "1";
                                }
                                else
                                {
                                    value = "0";
                                }
                            }
                            else if (value == String.Empty && (
                                     dataType.StartsWith("int") ||
                                     dataType == "double" ||
                                     dataType == "single" ||
                                     dataType == "float" ||
                                     dataType == "decimal"))
                            {
                                value = "0";
                            }
                        }

                        dataObject.SetPropertyValue(name, value);
                    }
                }

                return dataObject;
            }
            catch (Exception ex)
            {
                _logger.Error("Error in FormDataObject: " + ex);

                throw new Exception(
                  "Error while forming a dataObject of type [" + objectType + "] from SPPID.",
                  ex
                );
            }
        }

        private XElement GetCommodityConfig(string objectType)
        {
            if (_configuration == null)
            {
                LoadConfiguration();
            }

            XElement commodityConfig = _configuration.Elements("commodity").Where(o => o.Element("name").Value == objectType).First();

            return commodityConfig;
        }

        private Response SaveDataObjects(string objectType, IList<IDataObject> dataObjects)
        {
            try
            {
                Response response = new Response();

                // Create data object directory in case it does not exist
                Directory.CreateDirectory(_settings["SP3DFolderPath"]);

                string path = String.Format(
                 "{0}{1}\\{2}.csv",
                   _settings["BaseDirectoryPath"],
                  _settings["SP3DFolderPath"],
                  objectType
                );

                //TODO: Need to update file, not replace it!
                TextWriter writer = new StreamWriter(path);

                foreach (IDataObject dataObject in dataObjects)
                {
                    Status status = new Status();

                    try
                    {
                        string identifier = GetIdentifier(dataObject);
                        status.Identifier = identifier;

                        List<string> csvRow = FormCSVRow(objectType, dataObject);

                        writer.WriteLine(String.Join(", ", csvRow.ToArray()));
                        status.Messages.Add("Record [" + identifier + "] has been saved successfully.");
                    }
                    catch (Exception ex)
                    {
                        status.Level = StatusLevel.Error;

                        string message = String.Format(
                          "Error while posting dataObject [{0}]. {1}",
                          dataObject.GetPropertyValue("Tag"),
                          ex.ToString()
                        );

                        status.Messages.Add(message);
                    }

                    response.Append(status);
                }

                writer.Close();

                return response;
            }
            catch (Exception ex)
            {
                _logger.Error("Error in LoadDataObjects: " + ex);
                throw new Exception("Error while loading data objects of type [" + objectType + "].", ex);
            }
        }

        private List<string> FormCSVRow(string objectType, IDataObject dataObject)
        {
            try
            {
                List<string> csvRow = new List<string>();

                XElement commodityElement = GetCommodityConfig(objectType);

                IEnumerable<XElement> attributeElements = commodityElement.Element("attributes").Elements("attribute");

                foreach (var attributeElement in attributeElements)
                {
                    string name = attributeElement.Attribute("name").Value;
                    string value = Convert.ToString(dataObject.GetPropertyValue(name));
                    csvRow.Add(value);
                }

                return csvRow;
            }
            catch (Exception ex)
            {
                _logger.Error("Error in FormSPPIDRow: " + ex);

                throw new Exception(
                  "Error while forming a CSV row of type [" + objectType + "] from a DataObject.",
                  ex
                );
            }
        }

        public override IList<IDataObject> Get(string objectType, IList<string> identifiers)
        {
          throw new Exception("Error while getting a count of type ");
            //try
            //{
            //    LoadDataDictionary(objectType);

            //    IList<IDataObject> allDataObjects = LoadDataObjects(objectType);

            //    var expressions = FormMultipleKeysPredicate(identifiers);

            //    if (expressions != null)
            //    {
            //        _dataObjects = allDataObjects.AsQueryable().Where(expressions).ToList();
            //    }

            //    return _dataObjects;
            //}
            //catch (Exception ex)
            //{
            //    _logger.Error("Error in GetList: " + ex);
            //    throw new Exception("Error while getting a list of data objects of type [" + objectType + "].", ex);
            //}
        }

       



    







    }
}







