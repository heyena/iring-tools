using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using org.iringtools.utility;

namespace org.iringtools.library
{
  public static class DataObjectExtensions
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(DataObjectExtensions));

  
    public static IList<IDataObject> GetDataObjectsWithVirtualProperties(this IList<IDataObject> dataObjects, DataObject objectType)
    {
      //TODO Handle null 
      _logger.DebugFormat("Adding virtual properties to object");

      try
      {
        foreach (DataProperty dataPropertyDef in objectType.dataProperties.Where(x => x.isVirtual == true))
        {
          IDataObject dataObject = null;

          for (int i = 0; i < dataObjects.Count; i++)
          {
            dataObject = dataObjects[i];

            // check if property already exist or not
          //  if (dataObject.GetPropertyValue(dataPropertyDef.propertyName) != null)
           //   continue;

            VirtualProperty vp = ((VirtualProperty)dataPropertyDef);
            StringBuilder valueBuilder = new StringBuilder();
            for (int j = 0; j < vp.virtualPropertyValues.Count; j++)
            {
              VirtualPropertyValue vpValue = vp.virtualPropertyValues[j];

              if (vpValue.type == VirtualPropertyValueType.Constant)
              {
                valueBuilder.Append(vpValue.valueText);
              }
              else if (vpValue.type == VirtualPropertyValueType.Property)
              {
                string val = Convert.ToString(dataObject.GetPropertyValue(vpValue.propertyName.ToUpper()));
                if (vpValue.length == -1 || val.Length <= vpValue.length)
                  valueBuilder.Append(val);
                else
                  valueBuilder.Append(val.Substring(0, vpValue.length));
              }

              if (j + 1 < vp.virtualPropertyValues.Count)
                valueBuilder.Append(vp.delimiter);
            }

            object value = valueBuilder.ToString();
            

            if (typeof(SerializableDataObject).IsAssignableFrom(dataObject.GetType()))
              ((SerializableDataObject)dataObject).Dictionary[dataPropertyDef.propertyName]= value;
            else if (typeof(GenericDataObject).IsAssignableFrom(dataObject.GetType()))
              ((GenericDataObject)dataObject).Dictionary[dataPropertyDef.propertyName]= value;
            else if (typeof(GenericContentObject).IsAssignableFrom(dataObject.GetType()))
              ((GenericContentObject)dataObject).Dictionary[dataPropertyDef.propertyName] = value;
            else //create new GenericDataObject object from exiting IDataObject and then add the new properties
            {
              IDataObject dataObjectNew = GetNewObject(dataObject, objectType);
              ((GenericDataObject)dataObjectNew).Dictionary[dataPropertyDef.propertyName] = value;
              dataObjects[i] = dataObjectNew;
            }
          }
        }
      }
      catch (Exception e)
      {
        _logger.Error("Error in adding virtual proprties" + e.Message);
        throw e;
      }

      return dataObjects;
    }

    private static IDataObject GetNewObject(IDataObject dataObject, DataObject objectType)
    {
        IDataObject dataObjectNew = new GenericDataObject() { ObjectType = objectType.objectName };
        IDictionary<string, object> dictionary = ((GenericDataObject)dataObjectNew).Dictionary;
        foreach (DataProperty dp in objectType.dataProperties.Where(x => !x.isVirtual))
        {
          dictionary.Add(dp.propertyName, dataObject.GetPropertyValue(dp.propertyName));
        }
        return dataObjectNew;
    }
  }
    
}

