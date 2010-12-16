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
        _appNamespace = String.Format("{0}{1}/{2}",
           _settings["GraphBaseUri"],
           HttpUtility.UrlEncode(_settings["ProjectName"]),
           HttpUtility.UrlEncode(_settings["ApplicationName"])
         );

        _dictionary = _dataLayer.GetDictionary();
        _dataObjects = dataObjects;

        if (_dataObjects != null && _dataObjects.Count == 1)
        {
          xElement = new XElement(_appNamespace + Utility.TitleCase(graphName) + "List",
            new XAttribute(XNamespace.Xmlns + "i", XSI_NS));

          DataObject dataObject = FindGraphDataObject(graphName);

          for (int i = 0; i < _dataObjects.Count; i++)
          {
            XElement rowElement = new XElement(_appNamespace + Utility.TitleCase(dataObject.ObjectName));
            CreateHierarchicalXml(rowElement, dataObject, i);
            xElement.Add(rowElement);
          }
        }
        if (_dataObjects != null && _dataObjects.Count > 1)
        {
          xElement = new XElement(_appNamespace + Utility.TitleCase(graphName) + "List",
           new XAttribute(XNamespace.Xmlns + "i", XSI_NS));

          DataObject dataObject = FindGraphDataObject(graphName);

          for (int i = 0; i < _dataObjects.Count; i++)
          {
            XElement rowElement = new XElement(_appNamespace + Utility.TitleCase(dataObject.ObjectName));
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
      foreach(DataProperty dataProperty in dataObject.DataProperties)
      {
        XElement propertyElement = new XElement(_appNamespace + Utility.TitleCase(dataProperty.PropertyName));
        propertyElement.Add(new XAttribute("dataType", dataProperty.DataType));
        var value = _dataObjects[dataObjectIndex].GetPropertyValue(dataProperty.PropertyName);
        
        if (value != null)
        {
          if (dataProperty.DataType.ToString().ToLower().Contains("date"))
            value = Utility.ToXsdDateTime(value.ToString());

          propertyElement.Value = value.ToString();

          parentElement.Add(propertyElement);
        }
        
      }
    }

    private void CreateIndexXml(XElement parentElement, DataObject dataObject, int dataObjectIndex)
    {
      foreach (KeyProperty keyProperty in dataObject.KeyProperties)
      {
        DataProperty dataProperty = dataObject.DataProperties.Find(dp => dp.PropertyName == keyProperty.KeyPropertyName);

        XElement propertyElement = new XElement(_appNamespace + Utility.TitleCase(dataProperty.PropertyName));
        propertyElement.Add(new XAttribute("dataType", dataProperty.DataType));
        var value = _dataObjects[dataObjectIndex].GetPropertyValue(dataProperty.PropertyName);
        if (value != null)
          propertyElement.Value = value.ToString();

        parentElement.Add(propertyElement);
      }
    }

    public DataObject FindGraphDataObject(string graphName)
    {
      foreach (DataObject dataObject in _dictionary.DataObjects)
      {
        if (dataObject.ObjectName.ToLower() == graphName.ToLower())
        {
          return dataObject;
        }
      }

      throw new Exception("Graph [" + graphName + "] does not exist.");
    }
    #endregion
  }
}
