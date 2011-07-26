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
  public class HtmlProjectionEngine : BaseDataProjectionEngine
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(DataProjectionEngine));
    private DataDictionary _dictionary = null;
    private XNamespace _appNamespace = null;

    [Inject]
    public HtmlProjectionEngine(AdapterSettings settings, DataDictionary dictionary)
    {
      _settings = settings;
      _dictionary = dictionary;
    }

    public override XDocument ToXml(string graphName, ref IList<IDataObject> dataObjects)
    {
      try
      {
        XDocumentType docType = new XDocumentType(
          "html", "-//W3C//DTD XHTML 1.0 Strict//EN", "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd", null);

        XDocument doc = new XDocument();
        doc.AddFirst(docType);

        XElement html = new XElement("html");
        doc.Add(html);

        XElement body = new XElement("body");
        html.Add(body);

        XElement table = new XElement("table");
        body.Add(table);

        table.Add(new XAttribute("border", 1));

        XElement headers = new XElement("tr");
        table.Add(headers);

        DataObject dataObject = FindGraphDataObject(graphName);

        foreach (DataProperty dataProperty in dataObject.dataProperties)
        {
          headers.Add(new XElement("th", dataProperty.propertyName));
        }

        foreach (IDataObject dataObj in dataObjects)
        {
          XElement row = new XElement("tr");
          table.Add(row);

          foreach (DataProperty dataProperty in dataObject.dataProperties)
          {
            string value = Convert.ToString(dataObj.GetPropertyValue(dataProperty.propertyName));

            if (String.IsNullOrEmpty(value))
            {
              value = "&nbsp;";
            }
            else if (dataProperty.dataType.ToString().ToLower().Contains("date"))
            {
              value = Utility.ToXsdDateTime(value);
            }

            XElement cellValue = new XElement("td", value);
            row.Add(cellValue);
          }
        }

        return doc;
      }
      catch (Exception e)
      {
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
    #endregion
  }
}
