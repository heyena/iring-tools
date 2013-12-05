using System;
using System.Collections.Generic;
using System.Xml.Linq;
using log4net;
using Ninject;
using System.Web;
using org.iringtools.utility;
using org.iringtools.library;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;
using org.iringtools.adapter.datalayer;
using System.Text;

namespace org.iringtools.adapter.projection
{
  public class JsonProjectionEngine : BaseDataProjectionEngine
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(JsonProjectionEngine));
    private string[] arrSpecialcharlist;
    private string[] arrSpecialcharValue;

    [Inject]
    public JsonProjectionEngine(AdapterSettings settings, DataDictionary dictionary)
      : base(settings)
    {
      _dictionary = dictionary;

      if (_settings["SpCharList"] != null && _settings["SpCharValue"] != null)
      {
        arrSpecialcharlist = _settings["SpCharList"].ToString().Split(',');
        arrSpecialcharValue = _settings["SpCharValue"].ToString().Split(',');
      }
    }

    public override XDocument ToXml(string graphName, ref List<IDataObject> dataObjects)
    {
      try
      {
        string app = _settings["ApplicationName"].ToLower();
        string proj = _settings["ProjectName"].ToLower();

        string resource = graphName.ToLower();
        string previousItemResourceName = graphName.ToLower();

        DataItems dataItems = new DataItems()
        {
          total = this.Count,
          start = this.Start,
          items = new List<DataItem>()
        };

        if (dataObjects.Count > 0)
        {
          DataObject dataObject = FindGraphDataObject(graphName);
          dataItems.version = dataObject.version;

          if (dataObject == null)
          {
            return new XDocument();
          }

          bool showNullValue = _settings["ShowJsonNullValues"] != null &&
            _settings["ShowJsonNullValues"].ToString() == "True";

          for (int i = 0; i < dataObjects.Count; i++)
          {
            IDataObject dataObj = dataObjects[i];

            if (dataObj != null)
            {
              if (i == 0)
              {
                dataItems.type = graphName;
              }

              DataItem dataItem = new DataItem()
              {
                properties = new Dictionary<string, object>(),
              };

              if (dataObj is GenericDataObject)
              {
                  dataItem.hasContent = ((GenericDataObject)dataObj).HasContent;
                  dataItem.type = ((GenericDataObject)dataObj).ObjectType;
                  if (previousItemResourceName != dataItem.type.ToLower())
                  {
                    dataObject = FindGraphDataObject(dataItem.type);
                    previousItemResourceName = dataItem.type.ToLower();
                  }
              }

              bool isContentObject = false;
              if (dataObj is IContentObject)
              {
                  dataItem.hasContent = true;
                  isContentObject = true;
              }

              if (isContentObject)
              {
                  MemoryStream stream = ((IContentObject)dataObj).Content.ToMemoryStream();
                  byte[] data = stream.ToArray();
                  string base64Content = Convert.ToBase64String(data);
                  dataItem.content = base64Content;
              }

              foreach (KeyProperty keyProperty in dataObject.keyProperties)
              {
                DataProperty dataProperty = dataObject.dataProperties.Find(x => keyProperty.keyPropertyName.ToLower() == x.propertyName.ToLower());

                if (dataProperty != null)
                {
                  object value = dataObj.GetPropertyValue(keyProperty.keyPropertyName);

                  if (value != null)
                  {
                    if (dataProperty.dataType == DataType.Char ||
                        dataProperty.dataType == DataType.DateTime ||
                        dataProperty.dataType == DataType.Date ||
                        dataProperty.dataType == DataType.String ||
                        dataProperty.dataType == DataType.TimeStamp)
                    {
                      string valueStr = Convert.ToString(value);
                      valueStr = Utility.ConvertSpecialCharOutbound(valueStr, arrSpecialcharlist, arrSpecialcharValue);  //Handling special Characters here.

                      if (dataProperty.dataType == DataType.DateTime ||
                          dataProperty.dataType == DataType.Date)
                        valueStr = Utility.ToXsdDateTime(valueStr);

                      value = valueStr;
                    }
                  }
                  else
                  {
                    value = string.Empty;
                  }

                  if (!string.IsNullOrEmpty(dataItem.id))
                  {
                    dataItem.id += dataObject.keyDelimeter;
                  }

                  dataItem.id += value;
                }
              }

              foreach (DataProperty dataProperty in dataObject.dataProperties)
              {
                if (!dataProperty.isHidden)
                {
                  object value = dataObj.GetPropertyValue(dataProperty.propertyName);

                  if (value != null)
                  {
                    if (dataProperty.dataType == DataType.Char ||
                          dataProperty.dataType == DataType.DateTime ||
                          dataProperty.dataType == DataType.Date ||
                          dataProperty.dataType == DataType.String ||
                          dataProperty.dataType == DataType.TimeStamp)
                    {
                      string valueStr = Convert.ToString(value);

                      if (dataProperty.dataType == DataType.DateTime ||
                          dataProperty.dataType == DataType.Date)
                        valueStr = Utility.ToXsdDateTime(valueStr);

                      value = valueStr;
                    }

                    dataItem.properties.Add(dataProperty.propertyName, value);
                  }
                }
                else if (showNullValue)
                {
                  dataItem.properties.Add(dataProperty.propertyName, null);
                }
              }

              if (_settings["DisplayLinks"].ToLower() == "true")
              {
                string itemHref = String.Format("{0}/{1}", BaseURI, dataItem.id);

                dataItem.links = new List<Link> 
                {
                  new Link {
                    href = itemHref,
                    rel = "self"
                  }
                };

                foreach (DataRelationship dataRelationship in dataObject.dataRelationships)
                {
                  long relObjCount = 0;
                  bool validateLinks = (_settings["ValidateLinks"].ToLower() == "true");

                  if (validateLinks)
                  {
                    //TODO:
                    //relObjCount = _dataLayer.GetRelatedCount(dataObj, dataRelationship.relatedObjectName);
                  }

                  // only add link for related object that has data
                  if (!validateLinks || relObjCount > 0)
                  {
                    string relObj = dataRelationship.relatedObjectName.ToLower();
                    string relName = dataRelationship.relationshipName.ToLower();

                    Link relLink = new Link()
                    {
                      href = String.Format("{0}/{1}", itemHref, relName),
                      rel = relObj
                    };

                    dataItem.links.Add(relLink);
                  }
                }
              }

              dataItems.items.Add(dataItem);
            }
          }
        }

        dataItems.limit = dataItems.items.Count;

        if (dataItems.limit == 0) //Blank data item must have atleast version and type
        {
          DataObject dataObject = FindGraphDataObject(graphName);
          dataItems.version = dataObject.version;
          dataItems.type = graphName;
        }

        string xml = Utility.SerializeDataContract<DataItems>(dataItems);
        XElement xElement = XElement.Parse(xml);
        return new XDocument(xElement);
      }
      catch (Exception e)
      {
        _logger.Error("Error creating JSON content: " + e);
        throw e;
      }
    }

    public override XDocument ToXml(string graphName, ref List<IDataObject> dataObjects, string className, string classIdentifier)
    {
      return ToXml(graphName, ref dataObjects);
    }

    public override List<IDataObject> ToDataObjects(string graphName, ref XDocument xml)
    {
      try
      {
        List<IDataObject> dataObjects = new List<IDataObject>();
        DataObject objectType = FindGraphDataObject(graphName);
        string previousItemResourceName = graphName;

        if (objectType != null)
        {
          DataItems dataItems = Utility.DeserializeDataContract<DataItems>(xml.ToString());

          foreach (DataItem dataItem in dataItems.items)
          {
            if (dataItem.type != null && previousItemResourceName != dataItem.type) // handling differnet type of DataItems in same collection
            {
              objectType = FindGraphDataObject(dataItem.type);
              previousItemResourceName = dataItem.type;
            }

            if (dataItem.id != null)
            {
              dataItem.id = Utility.ConvertSpecialCharInbound(dataItem.id, arrSpecialcharlist, arrSpecialcharValue);  //Handling special Characters here.
            }
            else // if id doesn't exist, make it from key properties.
            {
              if (objectType.keyProperties.Count == 1)
              {
                string keyProp = objectType.keyProperties[0].keyPropertyName;
                //object id = dataItem.properties[keyProp];

                //if (id == null || id.ToString() == string.Empty)
                //{
                //  throw new Exception("Value of key property: " + keyProp + " cannot be null.");
                //}

                //dataItem.id = id.ToString();
                if (dataItem.properties.ContainsKey(keyProp))
                {
                  object id = dataItem.properties[keyProp];

                  if (id != null)
                  {
                    dataItem.id = id.ToString();
                  }
                }
              }
              else
              {
                StringBuilder builder = new StringBuilder();

                foreach (KeyProperty keyProp in objectType.keyProperties)
                {
                  string propName = objectType.keyProperties[0].keyPropertyName;
                  object propValue = dataItem.properties[propName];

                  // it is acceptable to have some key property values to be null but not all
                  if (propValue == null)
                    propValue = string.Empty;

                  builder.Append(objectType.keyDelimeter + propValue);
                }

                builder.Remove(0, objectType.keyDelimeter.Length);

                if (builder.Length == 0)
                {
                  throw new Exception("Invalid identifier.");
                }
                
                dataItem.id = builder.ToString();
              }
            }

            SerializableDataObject dataObject = new SerializableDataObject();
            dataObject.Type = objectType.objectName;
            dataObject.Id = dataItem.id;

            if (objectType.hasContent)
            {
                string base64Content = dataItem.content;

                if (!String.IsNullOrEmpty(base64Content))
                {
                    dataObject.Content = base64Content.ToMemoryStream();
                    dataObject.Content.Position = 0;
                    dataObject.HasContent = true;
                    dataObject.ContentType = dataItem.contentType;
                }
            }

            //
            // set key properties from id
            //
            if (objectType.keyProperties.Count == 1)
            {
              if (!string.IsNullOrEmpty(dataItem.id))
              {
                dataObject.SetPropertyValue(objectType.keyProperties[0].keyPropertyName, dataItem.id);
              }
            }
            else if (objectType.keyProperties.Count > 1)
            {
              string[] idParts = dataItem.id.Split(new string[] { objectType.keyDelimeter }, StringSplitOptions.None);

              for (int i = 0; i < objectType.keyProperties.Count; i++)
              {
                string keyProp = objectType.keyProperties[i].keyPropertyName;
                string keyValue = idParts[i];

                if (!string.IsNullOrEmpty(keyValue))
                {
                  dataObject.SetPropertyValue(keyProp, keyValue);
                }
              }
            }

            //
            // set data properties
            //
            foreach (var pair in dataItem.properties)
            {
              dataObject.SetPropertyValue(pair.Key, pair.Value);
            }

            dataObjects.Add(dataObject);
          }
        }

        return dataObjects;
      }
      catch (Exception e)
      {
        string message = "Error marshalling data items to data objects." + e;
        _logger.Error(message);
        throw new Exception(message);
      }
    }

    #region helper methods
    private DataObject FindGraphDataObject(string dataObjectName)
    {
      foreach (DataObject dataObject in _dictionary.dataObjects)
      {
        if (dataObject.objectName.ToLower() == dataObjectName.ToLower())
        {
          return dataObject;
        }
      }

      throw new Exception("DataObject [" + dataObjectName + "] does not exist.");
    }

    private bool IsNumeric(DataType dataType)
    {
      return (dataType == DataType.Decimal ||
              dataType == DataType.Single ||
              dataType == DataType.Double ||
              dataType == DataType.Int16 ||
              dataType == DataType.Int32 ||
              dataType == DataType.Int64);
    }
    #endregion
  }
}
