using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Web;

namespace iRINGTools.Data
{
  public class ProjectionEngine : IProjectionEngine
  {
    public int Id { get; set; }
    public string Format { get; set; }
    public long Count { get; set; }
    public bool FullIndex { get; set; }

    public ProjectionEngine()
    {
    }

    public XDocument ToXml(Application application, string graphName, ref IList<IDataObject> dataObjects)
    {
      XElement xElement = null;

      try
      {
        XNamespace appNamespace = String.Format("{0}{1}/{2}/{3}",
           application.GraphBaseUri,
           HttpUtility.UrlEncode(application.Scope.Name),
           HttpUtility.UrlEncode(application.Name),
           HttpUtility.UrlEncode(graphName)
        );

        if (dataObjects != null && (dataObjects.Count == 1 || FullIndex))
        {
          xElement = new XElement(appNamespace + Utility.TitleCase(graphName) + "List");

          DictionaryObject dictionaryObject = application.Dictionary.DictionaryObjectByName(graphName);

          for (int i = 0; i < dataObjects.Count; i++)
          {
            XElement rowElement = new XElement(appNamespace + Utility.TitleCase(dictionaryObject.ObjectName));
            CreateHierarchicalXml(appNamespace, rowElement, dictionaryObject, dataObjects[i]);
            xElement.Add(rowElement);
          }
        }
        if (dataObjects != null && (dataObjects.Count > 1 && !FullIndex))
        {
          xElement = new XElement(appNamespace + Utility.TitleCase(graphName) + "List");

          XAttribute total = new XAttribute("total", this.Count);
          xElement.Add(total);

          DictionaryObject dictionaryObject = application.Dictionary.DictionaryObjectByName(graphName);

          for (int i = 0; i < dataObjects.Count; i++)
          {
            XElement rowElement = new XElement(appNamespace + Utility.TitleCase(dictionaryObject.ObjectName));
            CreateIndexXml(appNamespace, rowElement, dictionaryObject, dataObjects[i]);
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

    public XDocument ToXml(Application application, string graphName, string className, string classIdentifier, ref IDataObject dataObject)
    {
      IList<IDataObject> dataObjects = new List<IDataObject> { dataObject };
      return ToXml(application, graphName, ref dataObjects);
    }

    public IList<IDataObject> ToDataObjects(Application application, string graphName, ref System.Xml.Linq.XDocument xDocument)
    {
      throw new NotImplementedException();
    }

    #region helper methods
    private void CreateHierarchicalXml(XNamespace appNamespace, XElement parentElement, DictionaryObject dataObject, IDataObject dataObjectIndex)
    {
      foreach (DataProperty dataProperty in dataObject.DataProperties)
      {
        XElement propertyElement = new XElement(appNamespace + Utility.TitleCase(dataProperty.PropertyName));
        propertyElement.Add(new XAttribute("dataType", dataProperty.DataType));
        var value = dataObjectIndex.GetPropertyValue(dataProperty.PropertyName);

        if (value != null)
        {
          if (dataProperty.DataType.ToString().ToLower().Contains("date"))
            value = Utility.ToXsdDateTime(value.ToString());

          propertyElement.Value = value.ToString();

          parentElement.Add(propertyElement);
        }

      }
    }

    private void CreateIndexXml(XNamespace appNamespace, XElement parentElement, DictionaryObject dataObject, IDataObject dataObjectIndex)
    {
      string uri = appNamespace.ToString() + "/";

      foreach (KeyProperty keyProperty in dataObject.KeyProperties)
      {
        DataProperty dataProperty = dataObject.DataProperties
          .Where(dp => dp.PropertyName == keyProperty.KeyPropertyName)
          .SingleOrDefault();

        var value = dataObjectIndex.GetPropertyValue(dataProperty.PropertyName);
        if (value != null)
        {
          XElement propertyElement = new XElement(appNamespace + Utility.TitleCase(dataProperty.PropertyName), value);
          parentElement.Add(propertyElement);

          uri += value;
        }
      }

      List<DataProperty> indexProperties = dataObject.DataProperties
        .Where(dp => dp.ShowOnIndex == true)
        .ToList();

      foreach (var indexProperty in indexProperties)
      {
        var value = dataObjectIndex.GetPropertyValue(indexProperty.PropertyName);
        if (value != null)
        {
          XElement propertyElement = new XElement(appNamespace + Utility.TitleCase(indexProperty.PropertyName), value);
          parentElement.Add(propertyElement);
        }
      }

      XAttribute uriAttribute = new XAttribute("uri", uri);
      parentElement.Add(uriAttribute);
    }
    #endregion
  }
}
