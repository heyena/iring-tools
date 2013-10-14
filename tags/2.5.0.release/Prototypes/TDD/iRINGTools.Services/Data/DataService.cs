using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using iRINGTools.Data;

namespace iRINGTools.Services
{
  public class DataService: IDataService
  {
    private IAdapterService _adapterService;
    private IProjectionEngineRepository _projectionEngineRepository;

    public DataService(
      IProjectionEngineRepository projectionEngineRepository,
      IAdapterService adapterService)
    {
      _projectionEngineRepository = projectionEngineRepository;
      if (_projectionEngineRepository == null)
        throw new InvalidOperationException("ProjectionEngine Repository cannot be null");
      
      _adapterService = adapterService;
      if (_adapterService == null)
        throw new InvalidOperationException("Adapter Service cannot be null");
    }

    #region ProjectionEngine Repository

    /// <summary>
    /// Get the ProjectionEngine from DB
    /// </summary>
    /// <returns>ProjectionEngineCollection</returns>
    public IList<IProjectionEngine> GetProjectionEngines()
    {
      return _projectionEngineRepository.GetProjectionEngines().ToList();
    }

    /// <summary>
    /// Returns a single ProjectionEngine by Name
    /// </summary>
    /// <param name="formatName">The ProjectionEngine Format Name</param>
    /// <returns>ProjectionEngine</returns>
    public IProjectionEngine GetProjectionEngine(string formatName)
    {
      IProjectionEngine result = _projectionEngineRepository.GetProjectionEngines()
          .WithProjectionEngineFormat(formatName)
          .SingleOrDefault();

      return result;
    }

    #endregion

    #region Data Service

    public IList<string> GetIdentifiers()
    {
      throw new NotImplementedException();
    }

    private DataFilter BuildFilter(NameValueCollection parameters) 
    {
      DataFilter filter = new DataFilter();

      List<DataExpression> expressions = new List<DataExpression>();
      foreach (string key in parameters.AllKeys)
      {
        string[] expectedParameters = { 
              "format", 
              "start", 
              "limit", 
              "sortBy", 
              "sortOrder",
              "indexStyle",
            };

        if (!expectedParameters.Contains(key, StringComparer.CurrentCultureIgnoreCase))
        {
          string value = parameters[key];

          DataExpression expression = new DataExpression
          {
            PropertyName = key,
            RelationalOperator = RelationalOperator.EqualTo,
            Values = new List<string> { value },
            IsCaseSensitive = false,
          };

          expressions.Add(expression);
        }
      }
      filter.Expressions = expressions;

      if (!String.IsNullOrEmpty(parameters["sortBy"]))
      {
        OrderExpression orderBy = new OrderExpression
        {
          PropertyName = parameters["sortBy"],
        };

        if (String.Compare(SortOrder.Desc.ToString(), parameters["sortOrder"], true) == 0)
        {
          orderBy.SortOrder = SortOrder.Desc;
        }
        else
        {
          orderBy.SortOrder = SortOrder.Asc;
        }

        filter.OrderExpressions.Add(orderBy);
      }

      return filter;
    }

    //DataFilter List
    public XDocument GetDataProjection(
      string scopeName,
      string applicationName,
      string graphName,
      DataFilter filter,
      string formatName,
      int start,
      int limit,
      bool fullIndex)
    {
      Application application = _adapterService.GetApplication(scopeName, applicationName);
      IDataLayer dataLayer = application.DataLayerItem.CreateDataLayer(application.Configuration);

      string dataObjectName = string.Empty;

      GraphMap graphMap = application.Mapping.GraphMapByName(graphName);
      DictionaryObject dictionaryObject = application.Dictionary.DictionaryObjectByName(graphName);

      if (graphMap != null)
      {
        dataObjectName = graphMap.DictionaryObjectName;
      }
      else if (dictionaryObject != null)
      {
        graphName = dictionaryObject.ObjectName;
        dataObjectName = dictionaryObject.ObjectName;
      }

      if (limit == 0)
        limit = 100;

      IList<IDataObject> dataObjects = dataLayer.Get(dataObjectName, filter, limit, start);

      IProjectionEngine projectionEngine = GetProjectionEngine(formatName);
      projectionEngine.Count = dataLayer.GetCount(dataObjectName, filter);
      projectionEngine.FullIndex = fullIndex;

      return projectionEngine.ToXml(application, graphName, ref dataObjects);
    }

    //Individual
    public XDocument GetDataProjection(
      string scopeName,
      string applicationName,
      string graphName,
      string className,
      string classIdentifier,
      string formatName,
      bool fullIndex)
    {
      Application application = _adapterService.GetApplication(scopeName, applicationName);
      IDataLayer dataLayer = application.DataLayerItem.CreateDataLayer(application.Configuration);

      string dataObjectName = string.Empty;
      
      GraphMap graphMap = application.Mapping.GraphMapByName(graphName);
      DictionaryObject dictionaryObject = application.Dictionary.DictionaryObjectByName(graphName);

      if (graphMap != null)
      {
        dataObjectName = graphMap.DictionaryObjectName;
      }
      else if (dictionaryObject != null)
      {
        graphName = dictionaryObject.ObjectName;
        dataObjectName = dictionaryObject.ObjectName;
      }

      DataFilter filter = new DataFilter();
      
      IList<string> identifiers = dataLayer.GetIdentifiers(dictionaryObject.ObjectName, filter);

      IList<IDataObject> dataObjects = dataLayer.Get(dataObjectName, identifiers);
      IDataObject firstDataObject = dataObjects.First();

      IProjectionEngine projectionEngine = GetProjectionEngine(formatName);
      return projectionEngine.ToXml(application, graphName, className, classIdentifier, ref firstDataObject);
    }

    //List
    public XDocument GetDataProjection(
      string scopeName,
      string applicationName,
      string graphName,
      string formatName,
      int start,
      int limit,
      bool fullIndex,
      NameValueCollection parameters)
    {
      Application application = _adapterService.GetApplication(scopeName, applicationName);
      IDataLayer dataLayer = application.DataLayerItem.CreateDataLayer(application.Configuration);

      string dataObjectName = string.Empty;

      GraphMap graphMap = application.Mapping.GraphMapByName(graphName);
      DictionaryObject dictionaryObject = application.Dictionary.DictionaryObjectByName(graphName);

      if (graphMap != null)
      {
        dataObjectName = graphMap.DictionaryObjectName;
      }
      else if (dictionaryObject != null)
      {
        graphName = dictionaryObject.ObjectName;
        dataObjectName = dictionaryObject.ObjectName;
      }

      if (limit == 0)
        limit = 100;

      DataFilter filter = BuildFilter(parameters);

      IList<IDataObject> dataObjects = dataLayer.Get(dataObjectName, filter, limit, start);

      IProjectionEngine projectionEngine = GetProjectionEngine(formatName);
      projectionEngine.Count = dataLayer.GetCount(dataObjectName, filter);
      projectionEngine.FullIndex = fullIndex;

      return projectionEngine.ToXml(application, graphName, ref dataObjects);
    }

    #endregion
  }
}
