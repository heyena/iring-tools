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
    private XNamespace _appNamespace = null;

    [Inject]
    public DataProjectionEngine(AdapterSettings settings, IDataLayer dataLayer, DataDictionary dictionary)
    {
      _settings = settings;
      _dataLayer = dataLayer;
    }

    public override XDocument ToXml(string graphName, ref IList<IDataObject> dataObjects)
    {
      XElement xElement = null;

      try
      {
        _appNamespace = String.Format("{0}{1}/{2}/{3}",
           _settings["GraphBaseUri"],
           HttpUtility.UrlEncode(_settings["ProjectName"]),
           HttpUtility.UrlEncode(_settings["ApplicationName"]),
           HttpUtility.UrlEncode(graphName)
         );

        _dictionary = _dataLayer.GetDictionary();
        _dataObjects = dataObjects;

        if (_dataObjects != null && _dataObjects.Count == 1)
        {
          xElement = new XElement(_appNamespace + Utility.TitleCase(graphName) + "List");

          DataObject dataObject = FindGraphDataObject(graphName);

          for (int i = 0; i < _dataObjects.Count; i++)
          {
            XElement rowElement = new XElement(_appNamespace + Utility.TitleCase(dataObject.objectName));
            CreateHierarchicalXml(rowElement, dataObject, i);
            xElement.Add(rowElement);
          }
        }
        if (_dataObjects != null && _dataObjects.Count > 1)
        {
          xElement = new XElement(_appNamespace + Utility.TitleCase(graphName) + "List");

          DataObject dataObject = FindGraphDataObject(graphName);

          for (int i = 0; i < _dataObjects.Count; i++)
          {
            XElement rowElement = new XElement(_appNamespace + Utility.TitleCase(dataObject.objectName));
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

    public override IList<IDataObject> ToDataObjects(string graphName, ref XDocument xml)
    {
      throw new NotImplementedException();
    }

    #region helper methods
    private void CreateHierarchicalXml(XElement parentElement, DataObject dataObject, int dataObjectIndex)
    {
      foreach(DataProperty dataProperty in dataObject.dataProperties)
      {
        XElement propertyElement = new XElement(_appNamespace + Utility.TitleCase(dataProperty.propertyName));
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
      string uri = _appNamespace.ToString() + "/";

      foreach (KeyProperty keyProperty in dataObject.keyProperties)
      {
        DataProperty dataProperty = dataObject.dataProperties.Find(dp => dp.propertyName == keyProperty.keyPropertyName);

        var value = _dataObjects[dataObjectIndex].GetPropertyValue(dataProperty.propertyName);
        if (value != null)
          uri += dataObject.keyDelimeter + value;
      }

      parentElement.Value = uri;
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
