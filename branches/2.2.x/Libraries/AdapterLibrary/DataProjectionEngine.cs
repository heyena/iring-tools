using System;
using System.Collections.Generic;
using System.Xml.Linq;
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
    private DataDictionary _dictionary = null;
    private XNamespace _graphNamespace = null;

    [Inject]
    public DataProjectionEngine(AdapterSettings settings, DataDictionary dictionary) : base(settings)
    {
      _dictionary = dictionary;
    }

    public override XDocument ToXml(string graphName, ref IList<IDataObject> dataObjects)
    {
      XElement xElement = null;

      try
      {
        string baseUri = _settings["GraphBaseUri"];
        string project = _settings["ProjectName"];
        string app = _settings["ApplicationName"];
        string appBaseUri = Utility.FormAppBaseURI(_uriMaps, baseUri, project, app);
        _graphNamespace = appBaseUri + graphName + "/";
        
        //_dictionary = _dataLayer.GetDictionary();
        _dataObjects = dataObjects;

        if (_dataObjects != null && (_dataObjects.Count == 1 || FullIndex))
        {
          xElement = new XElement(_graphNamespace + Utility.TitleCase(graphName) + "List");

          DataObject dataObject = FindGraphDataObject(graphName);

          for (int i = 0; i < _dataObjects.Count; i++)
          {
            XElement rowElement = new XElement(_graphNamespace + Utility.TitleCase(dataObject.objectName));
            CreateHierarchicalXml(rowElement, dataObject, i);
            xElement.Add(rowElement);
          }
        }
        if (_dataObjects != null && (_dataObjects.Count > 1 && !FullIndex))
        {
          xElement = new XElement(_graphNamespace + Utility.TitleCase(graphName) + "List");

          XAttribute total = new XAttribute("total", this.Count);
          xElement.Add(total);

          DataObject dataObject = FindGraphDataObject(graphName);

          for (int i = 0; i < _dataObjects.Count; i++)
          {
            XElement rowElement = new XElement(_graphNamespace + Utility.TitleCase(dataObject.objectName));
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

    public override XDocument ToXml(string graphName, ref IList<IDataObject> dataObjects, string className, string classIdentifier)
    {
      return ToXml(graphName, ref dataObjects);
    }

    public override IList<IDataObject> ToDataObjects(string graphName, ref XDocument xml)
    {
      throw new NotImplementedException();
    }

    #region helper methods
    private void CreateHierarchicalXml(XElement parentElement, DataObject dataObject, int dataObjectIndex)
    {
      foreach(DataProperty dataProperty in dataObject.dataProperties)
      {
        XElement propertyElement = new XElement(_graphNamespace + Utility.TitleCase(dataProperty.propertyName));
        propertyElement.Add(new XAttribute("dataType", dataProperty.dataType));
        var value = _dataObjects[dataObjectIndex].GetPropertyValue(dataProperty.propertyName);
        
        if (value != null)
        {
          if (dataProperty.dataType.ToString().ToLower().Contains("date"))
            value = Utility.ToXsdDateTime(value.ToString());

          propertyElement.Value = value.ToString();

          parentElement.Add(propertyElement);
        }
        
      }
    }

    private void CreateIndexXml(XElement parentElement, DataObject dataObject, int dataObjectIndex)
    {
      string uri = _graphNamespace.ToString() + "/";

      int keyCounter = 0;
      foreach (KeyProperty keyProperty in dataObject.keyProperties)
      {
        DataProperty dataProperty = dataObject.dataProperties.Find(dp => dp.propertyName == keyProperty.keyPropertyName);

        var value = _dataObjects[dataObjectIndex].GetPropertyValue(dataProperty.propertyName);
        if (value != null)
        {
          XElement propertyElement = new XElement(_graphNamespace + Utility.TitleCase(dataProperty.propertyName), value);
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
          XElement propertyElement = new XElement(_graphNamespace + Utility.TitleCase(indexProperty.propertyName), value);
          parentElement.Add(propertyElement);
        }
      }

      XAttribute uriAttribute = new XAttribute("uri", uri);
      parentElement.Add(uriAttribute);
    }

    public DataObject FindGraphDataObject(string dataObjectName)
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
    #endregion
  }
}
