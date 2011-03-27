using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Ciloci.Flee;
using log4net;
using Ninject;
using org.iringtools.datalayer.excel;
using org.iringtools.library;
using org.iringtools.utility;
using Excel = Microsoft.Office.Interop.Excel;

namespace org.iringtools.adapter.datalayer
{
  public class ExcelDataObject : Dictionary<string, object>, IDataObject
  {

    public string SheetName { get; set; }

    public object GetPropertyValue(string propertyName)
    {
      if (this.ContainsKey(propertyName))
      {
        return this[propertyName];
      }
      else
      {
        //throw new Exception("Property [" + propertyName + "] does not exist.");
        return null;
      }
    }

    public IList<IDataObject> GetRelatedObjects(string relatedObjectType)
    {
      switch (relatedObjectType)
      {
        default:
          throw new Exception("Related object [" + relatedObjectType + "] does not exist.");
      }
    }

    public void SetPropertyValue(string propertyName, object value)
    {
      this[propertyName] = value;
    }
  }

  public class ExcelDataLayer : BaseDataLayer, IDataLayer2
  {
    private static readonly ILog _logger = LogManager.GetLogger(typeof(ExcelDataLayer));
    private AdapterSettings _settings = null;
    private ExcelProvider _provider = null;

    [Inject]
    public ExcelDataLayer(AdapterSettings settings)
    {
      try
      {
        _settings = settings;        
        _provider = new ExcelProvider(settings);
      }
      catch (Exception e)
      {
        _logger.Error("Error in ExcelDataLayer contructor.", e);
      }
    }

    public override IList<IDataObject> Create(string objectType, IList<string> identifiers)
    {
      try
      {
        ExcelWorksheet cfWorksheet = _provider.GetWorksheet(objectType);

        IList<IDataObject> dataObjects = new List<IDataObject>();

        objectType = objectType.Substring(objectType.LastIndexOf('.') + 1);
        
        if (identifiers != null && identifiers.Count > 0)
        {
          foreach (string identifier in identifiers)
          {
            ExcelDataObject dataObject = new ExcelDataObject()
            {
              SheetName = objectType
            };

            if (!String.IsNullOrEmpty(identifier))
            {
              dataObject.SetPropertyValue(cfWorksheet.Identifier, identifier);
            }

            dataObjects.Add(dataObject);
          }
        }

        return dataObjects;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in CreateList: " + ex);
        throw new Exception("Error while creating a list of data objects of type [" + objectType + "].", ex);
      }
    }

    public override Response Delete(string objectType, DataFilter filter)
    {
      try
      {
        string identifier = _provider.GetIdentifier(objectType);

        IList<string> identifiers = new List<string>();
        IList<IDataObject> dataObjects = Get(objectType, filter, 0, 0);

        foreach (IDataObject dataObject in dataObjects)
        {
          identifiers.Add((string)dataObject.GetPropertyValue(identifier));
        }

        return Delete(objectType, identifiers);
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Delete: " + ex);
        throw new Exception("Error while deleting data objects of type [" + objectType + "].", ex);
      }
    }

    public override Response Delete(string objectType, IList<string> identifiers)
    {
      Response response = new Response();

      if (identifiers == null || identifiers.Count == 0)
      {
        Status status = new Status();
        status.Level = StatusLevel.Warning;
        status.Messages.Add("Nothing to delete.");
        response.Append(status);
        return response;
      }      

      try
      {        
        IList<IDataObject> dataObjects = new List<IDataObject>();

        if (identifiers != null && identifiers.Count > 0)
        {
          foreach (string identifier in identifiers)
          {
            Status status = new Status();
            status.Identifier = identifier;
                        
            try
            {
              _provider.Delete(objectType, identifier);
                            
              status.Messages.Add("Data object [" + identifier + "] deleted successfully.");
            }
            catch (Exception ex)
            {
              _logger.Error("Error in Delete: " + ex);
              status.Level = StatusLevel.Error;
              status.Messages.Add("Error while deleting data object [" + identifier + "]." + ex);
            }

            response.Append(status);
          }
        }

        return response;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in DeletList: " + ex);
        throw new Exception("Error while delete a list of data objects of type [" + objectType + "].", ex);
      }
    }

    public override IList<IDataObject> Get(string objectType, DataFilter filter, int pageSize, int pageNumber)
    {      
      try
      {
        List<IDataObject> dataObjects = _provider.GetDataObjects(objectType);
        
        // Apply filter
        if (filter != null && filter.Expressions.Count > 0)
        {
          string variable = "dataObject";
          string linqExpression = string.Empty;
          switch (objectType)
          {
            default:
              linqExpression = filter.ToLinqExpression<IDataObject>(variable);
              break;
          }

          if (linqExpression != String.Empty)
          {
            ExpressionContext context = new ExpressionContext();
            context.Variables.DefineVariable(variable, typeof(ExcelDataObject));

            for (int i = 0; i < dataObjects.Count; i++)
            {
              context.Variables[variable] = dataObjects[i];
              var expression = context.CompileGeneric<bool>(linqExpression);
              if (!expression.Evaluate())
              {
                dataObjects.RemoveAt(i--);
              }
            }
          }
        }

        // Apply paging
        if (pageSize > 0 && pageNumber > 0)
        {
          if (dataObjects.Count > (pageSize * (pageNumber - 1) + pageSize))
          {
            dataObjects = dataObjects.GetRange(pageSize * (pageNumber - 1), pageSize);
          }
          else if (pageSize * (pageNumber - 1) > dataObjects.Count)
          {
            dataObjects = dataObjects.GetRange(pageSize * (pageNumber - 1), dataObjects.Count);
          }
          else
          {
            return null;
          }
        }

        return dataObjects;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetList: " + ex);
        throw new Exception("Error while getting a list of data objects of type [" + objectType + "].", ex);
      }
    }

    public override IList<IDataObject> Get(string objectType, IList<string> identifiers)
    {      
      try
      {

        return _provider.GetDataObjects(objectType, identifiers);                
        
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetList: " + ex);
        throw new Exception("Error while getting a list of data objects of type [" + objectType + "].", ex);
      }
    }

    public override DataDictionary GetDictionary()
    {
      try
      {
        DataDictionary dataDictionary = new DataDictionary()
        {
          dataObjects = new List<DataObject>()
        };

        foreach (ExcelWorksheet worksheet in _provider.GetWorksheets())
        {
          DataObject dataObject = new DataObject()
          {
            objectName = worksheet.Label,
            tableName = worksheet.Name,
            dataProperties = new List<DataProperty>()
          };

          dataDictionary.dataObjects.Add(dataObject);

          foreach (ExcelColumn column in worksheet.Columns)
          {
            DataProperty dataProperty = new DataProperty()
            {
              propertyName = column.Label,
              columnName = column.Name,
              dataType = column.DataType
            };

            if (worksheet.Identifier == column.Label)
            {
              dataObject.addKeyProperty(dataProperty);
            }
            else
            {
              dataObject.dataProperties.Add(dataProperty);
            }
          }
        }

        return dataDictionary;
      }
      catch (Exception e)
      {
        throw new Exception("Error while creating dictionary.", e);
      }
    }

    public override IList<string> GetIdentifiers(string objectType, DataFilter filter)
    {
      try
      {
        string identifier = _provider.GetIdentifier(objectType);

        List<string> identifiers = new List<string>();
        IList<IDataObject> dataObjects = Get(objectType, filter, 0, 0);

        foreach (IDataObject dataObject in dataObjects)
        {
          identifiers.Add((string)dataObject.GetPropertyValue(identifier));
        }

        return identifiers;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetIdentifiers: " + ex);
        throw new Exception("Error while getting a list of identifiers of type [" + objectType + "].", ex);
      }
    }

    public override long GetCount(string objectType, DataFilter filter)
    {
      try
      {

        List<string> identifiers = new List<string>();
        IList<IDataObject> dataObjects = Get(objectType, filter, 0, 0);

        return dataObjects.Count;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in GetIdentifiers: " + ex);
        throw new Exception("Error while getting a list of identifiers of type [" + objectType + "].", ex);
      }
    }

    public override Response Post(IList<IDataObject> dataObjects)
    {
      Response response = new Response();
      
      if (dataObjects == null || dataObjects.Count == 0)
      {
        Status status = new Status();
        status.Level = StatusLevel.Warning;
        status.Messages.Add("Nothing to update.");
        response.Append(status);
        return response;
      }

      try
      {
        
        foreach (IDataObject dataObject in dataObjects)
        {
          _provider.Update((ExcelDataObject)dataObject);          
        }

        return response;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in PostList: " + ex);

        object sample = dataObjects.FirstOrDefault();
        string objectType = (sample != null) ? sample.GetType().Name : String.Empty;
        throw new Exception("Error while posting data objects of type [" + objectType + "].", ex);
      }
    }

    public override Response Configure(XElement configuration)
    {
      Response _response = new Response();
      _response.Messages = new Messages();
      try
      {
        _provider.Configure(Utility.DeserializeDataContract<ExcelConfiguration>(configuration.ToString()));
                
        _response.Messages.Add("DataLayer configuration Saved successfully");
        _response.Level = StatusLevel.Success;
      }
      catch (Exception ex)
      {
        _response.Messages.Add("Failed to Save datalayer Configuration");
        _response.Messages.Add(ex.Message);
        _response.Level = StatusLevel.Error;
      }
      return _response;
    }

    public override IList<IDataObject> GetRelatedObjects(IDataObject dataObject, string relatedObjectType)
    {
      throw new NotImplementedException();
    }
  }

}
