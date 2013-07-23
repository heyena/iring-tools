using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using log4net;
using Ninject;
using System.Web;
using org.iringtools.utility;
using org.iringtools.library;

namespace org.iringtools.adapter.projection
{
  public class DataProjectionEngine : BaseDataProjectionEngine
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(DataProjectionEngine));

    private XNamespace _objectNamespace = null;
    private string _objectType = String.Empty;

    private string[] arrSpecialcharlist;
    private string[] arrSpecialcharValue;

    [Inject]
    public DataProjectionEngine(AdapterSettings settings, DataDictionary dictionary) : base(settings)
    {
      _dictionary = dictionary;

      if (settings["SpCharList"] != null && settings["SpCharValue"] != null)
      {
          arrSpecialcharlist = settings["SpCharList"].ToString().Split(',');
          arrSpecialcharValue = settings["SpCharValue"].ToString().Split(',');
      }
    }

    public override XDocument ToXml(string objectType, ref List<IDataObject> dataObjects)
    {
      XElement xElement = null;

      try
      {
        string baseUri = _settings["GraphBaseUri"];
        string project = _settings["ProjectName"];
        string app = _settings["ApplicationName"];
        string appBaseUri = Utility.FormEndpointBaseURI(_uriMaps, baseUri, project, app);

        _objectType = objectType;
        _objectNamespace = appBaseUri + objectType + "/";        
        _dataObjects = dataObjects;

        if (_dataObjects != null && (_dataObjects.Count == 1 || FullIndex))
        {
          xElement = new XElement(_objectNamespace + Utility.TitleCase(objectType) + "List");
          DataObject dataObject = FindGraphDataObject(objectType);

          for (int i = 0; i < _dataObjects.Count; i++)
          {
            XElement rowElement = new XElement(_objectNamespace + Utility.TitleCase(dataObject.objectName));
            CreateHierarchicalXml(rowElement, dataObject, i);
            xElement.Add(rowElement);
          }
        }

        if (_dataObjects != null && (_dataObjects.Count > 1 && !FullIndex))
        {
          xElement = new XElement(_objectNamespace + Utility.TitleCase(objectType) + "List");

          XAttribute total = new XAttribute("total", this.Count);
          xElement.Add(total);

          DataObject dataObject = FindGraphDataObject(objectType);

          for (int i = 0; i < _dataObjects.Count; i++)
          {
            XElement rowElement = new XElement(_objectNamespace + Utility.TitleCase(dataObject.objectName));
            CreateIndexXml(rowElement, dataObject, i);
            xElement.Add(rowElement);
          }
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }

      return new XDocument(xElement);
    }

    public override XDocument ToXml(string objectType, ref List<IDataObject> dataObjects, string className, string classIdentifier)
    {
      return ToXml(objectType, ref dataObjects);
    }

    public override List<IDataObject> ToDataObjects(string objectType, ref XDocument xml)
    {
      try
      {
        List<IDataObject> dataObjects = new List<IDataObject>();
        DataObject objectDefinition = FindGraphDataObject(objectType);

        if (objectDefinition != null)
        {
          XNamespace ns = xml.Root.Attribute("xmlns").Value;
          string objectName = Utility.TitleCase(objectDefinition.objectName);

          XElement rootEl = xml.Element(ns + objectName + "List");          
          IEnumerable<XElement> objEls = from el in rootEl.Elements(ns + objectName) select el;

          foreach (XElement objEl in objEls)
          {
            SerializableDataObject dataObject = new SerializableDataObject();
            dataObject.Type = objectDefinition.objectName;

            if (objectDefinition.hasContent)
            {
              XElement xElement = objEl.Element(ns + "content");

              if (xElement != null)
              {
                string base64Content = xElement.Value;

                if (!String.IsNullOrEmpty(base64Content))
                {
                  ((IContentObject)dataObject).Content = base64Content.ToMemoryStream();
                }
              }
            }

            foreach (DataProperty property in objectDefinition.dataProperties)
            {
              string propertyName = property.propertyName;
              XElement valueEl = objEl.Element(ns + Utility.TitleCase(propertyName));

              if (valueEl != null)
              {
                string value = valueEl.Value;

                if (value != null)
                {
                  dataObject.SetPropertyValue(propertyName, value);
                }
              }
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
    private void CreateHierarchicalXml(XElement parentElement, DataObject dataObject, int dataObjectIndex)
    {
      foreach(DataProperty dataProperty in dataObject.dataProperties)
      {
        if (!dataProperty.isHidden)
        {
          object value = _dataObjects[dataObjectIndex].GetPropertyValue(dataProperty.propertyName);
        
          if (value != null)
          {
            if (dataProperty.dataType == DataType.Char ||
                  dataProperty.dataType == DataType.DateTime ||
                  dataProperty.dataType == DataType.String ||
                  dataProperty.dataType == DataType.TimeStamp)
            {
              string valueStr = Convert.ToString(value);

              if (dataProperty.dataType == DataType.DateTime)
                valueStr = Utility.ToXsdDateTime(valueStr);

              value = valueStr;
            }

            XElement propertyElement = new XElement(_objectNamespace + Utility.TitleCase(dataProperty.propertyName));
            propertyElement.Value = value.ToString();
            parentElement.Add(propertyElement);
          }
        }
      }

      foreach (DataRelationship dataRelationship in dataObject.dataRelationships)
      {
        XElement relationshipElement = new XElement(_objectNamespace + Utility.TitleCase(dataRelationship.relationshipName));
        parentElement.Add(relationshipElement);
      }
    }

    private void CreateIndexXml(XElement parentElement, DataObject dataObject, int dataObjectIndex)
    {
      string uri = _objectNamespace.ToString();

      if (!uri.EndsWith("/"))
        uri += "/";

      int keyCounter = 0;

      foreach (KeyProperty keyProperty in dataObject.keyProperties)
      {
        DataProperty dataProperty = dataObject.dataProperties.Find(dp => dp.propertyName == keyProperty.keyPropertyName);

        var value = _dataObjects[dataObjectIndex].GetPropertyValue(dataProperty.propertyName);
        if (value != null)
        {
          value = Utility.ConvertSpecialCharOutbound(value.ToString(), arrSpecialcharlist, arrSpecialcharValue);  //Handling special Characters here.
          XElement propertyElement = new XElement(_objectNamespace + Utility.TitleCase(dataProperty.propertyName), value);
          parentElement.Add(propertyElement);
          keyCounter++;

          if (keyCounter == dataObject.keyProperties.Count)
            uri += value;
          else
            uri += value + dataObject.keyDelimeter;
        }
      }

      List<DataProperty> indexProperties = dataObject.dataProperties.FindAll(dp => dp.showOnIndex == true);

      foreach (DataProperty indexProperty in indexProperties)
      {
        var value = _dataObjects[dataObjectIndex].GetPropertyValue(indexProperty.propertyName);
        if (value != null)
        {
          XElement propertyElement = new XElement(_objectNamespace + Utility.TitleCase(indexProperty.propertyName), value);
          parentElement.Add(propertyElement);
        }
      }

      XAttribute uriAttribute = new XAttribute("uri", uri);
      parentElement.Add(uriAttribute);
    }

    public DataObject FindGraphDataObject(string objectType)
    {
      foreach (DataObject dataObject in _dictionary.dataObjects)
      {
        if (dataObject.objectName.ToLower() == objectType.ToLower())
        {
          return dataObject;
        }
      }

      throw new Exception("Object type [" + objectType + "] not found.");
    }
    #endregion
  }
}
