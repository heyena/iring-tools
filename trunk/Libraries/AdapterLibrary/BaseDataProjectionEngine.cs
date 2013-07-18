using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using org.iringtools.library;
using System;
using System.Web;
using org.iringtools.utility;
using log4net;
using System.IO;

namespace org.iringtools.adapter.projection
{
  public abstract class BaseDataProjectionEngine : IProjectionLayer
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(BaseDataProjectionEngine));
    
    protected static readonly XNamespace XSD_NS = "http://www.w3.org/2001/XMLSchema#";
    protected static readonly XNamespace XSI_NS = "http://www.w3.org/2001/XMLSchema-instance#";

    protected static readonly string XSD_PREFIX = "xsd:";

    protected AdapterSettings _settings = null;
    protected IList<IDataObject> _dataObjects = null;
    protected Properties _uriMaps;

    public bool FullIndex { get; set; }
    public long Count { get; set; }
    public int Start { get; set; }
    public int Limit { get; set; }
    public string BaseURI { get; set; }

    public BaseDataProjectionEngine(AdapterSettings settings)
    {
      _settings = settings;

      _dataObjects = new List<IDataObject>();

      // load uri maps config
      _uriMaps = new Properties();

      string uriMapsFilePath = _settings["AppDataPath"] + "UriMaps.conf";

      if (File.Exists(uriMapsFilePath))
      {
        try
        {
          _uriMaps.Load(uriMapsFilePath);
        }
        catch (Exception e)
        {
          _logger.Info("Error loading [UriMaps.config]: " + e);
        }
      }
    }

    public abstract XDocument ToXml(string graphName, ref List<IDataObject> dataObjects);
    public abstract XDocument ToXml(string graphName, ref List<IDataObject> dataObjects, string className, string classIdentifier);
    public abstract List<IDataObject> ToDataObjects(string graphName, ref XDocument xDocument);
  }
}
