using System;
using System.Collections.Generic;
using System.Xml.Linq;
using log4net;
using Ninject;
using System.Web;
using org.iringtools.utility;
using org.iringtools.library;
using System.Text.RegularExpressions;

namespace org.iringtools.adapter.projection
{
  public class HtmlProjectionEngine : BaseDataProjectionEngine
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(DataProjectionEngine));
    private DataDictionary _dictionary = null;

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

        XElement head = new XElement("head");
        html.Add(head);

        XElement style = new XElement("style");
        head.Add(style);

        style.Add(new XAttribute("type", "text/css"));

        Regex regex = new Regex(@"\s+");
        string css = Utility.ReadString(_settings["DefaultStyleSheet"]);
        style.Add(regex.Replace(css, " "));

        XElement body = new XElement("body");
        html.Add(body);

        XElement table = new XElement("table");
        body.Add(table);

        XElement headers = new XElement("tr");
        table.Add(headers);

        DataObject dataObject = FindGraphDataObject(graphName);

        foreach (DataProperty dataProperty in dataObject.dataProperties)
        {
          headers.Add(new XElement("th", dataProperty.propertyName));
        }

        for (int i = 0; i < dataObjects.Count; i++)
        {
          IDataObject dataObj = dataObjects[i];

          XElement row = new XElement("tr");
          table.Add(row);

          if (i % 2 == 0)
          {
            row.Add(new XAttribute("class", "even"));
          }
          else
          {
            row.Add(new XAttribute("class", "odd"));
          }

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

            XElement cell = new XElement("td", value);
            row.Add(cell);

            if (IsNumeric(dataProperty.dataType))
            {
              cell.Add(new XAttribute("class", "right"));
            }
          }
        }

        return doc;
      }
      catch (Exception e)
      {
        _logger.Error("Error creating HTML content: " + e);
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
