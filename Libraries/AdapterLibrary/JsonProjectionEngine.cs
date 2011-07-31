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

namespace org.iringtools.adapter.projection
{
  public class JsonProjectionEngine : BaseDataProjectionEngine
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(JsonProjectionEngine));
    private DataDictionary _dictionary = null;

    [Inject]
    public JsonProjectionEngine(AdapterSettings settings, DataDictionary dictionary)
    {
      _settings = settings;
      _dictionary = dictionary;
    }

    public override XDocument ToXml(string graphName, ref IList<IDataObject> dataObjects)
    {
      try
      {
        DataItems dataItems = new DataItems()
        {
          total = this.Count,
          items = new List<DataItem>()
        };

        if (dataObjects.Count > 0)
        {
          dataItems.type = dataObjects[0].GetType().Name;
        }

        DataObject dataObject = FindGraphDataObject(graphName);

        for (int i = 0; i < dataObjects.Count; i++)
        {
          IDataObject dataObj = dataObjects[i];

          DataItem dataItem = new DataItem()
          {
            properties = new Dictionary<string, string>()
          };

          foreach (DataProperty dataProperty in dataObject.dataProperties)
          {
            string value = Convert.ToString(dataObj.GetPropertyValue(dataProperty.propertyName));

            if (value == null)
            {
              value = String.Empty;
            }
            else if (dataProperty.dataType == DataType.DateTime)
            {
              value = Utility.ToXsdDateTime(value);
            }

            dataItem.properties.Add(dataProperty.propertyName, value);
            
            if (dataObject.isKeyProperty(dataProperty.propertyName))
            {
              dataItem.id = value;
            }            
          }

          dataItems.items.Add(dataItem);
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

    public override XDocument ToXml(string graphName, ref IList<IDataObject> dataObjects, string className, string classIdentifier)
    {
      return ToXml(graphName, ref dataObjects);
    }

    public override IList<IDataObject> ToDataObjects(string graphName, ref XDocument xml)
    {
      throw new NotImplementedException();
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
