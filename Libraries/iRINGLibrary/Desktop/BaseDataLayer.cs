using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using org.iringtools.adapter;
using log4net;
using System.Linq.Expressions;

namespace org.iringtools.library
{
  public abstract class BaseDataLayer : IDataLayer2
  {
    protected AdapterSettings _settings = null;
    protected DataObject _dataObjectDefinition = null;
    protected XElement _configuration = null;

    protected static readonly ILog _logger = LogManager.GetLogger(typeof(IDataLayer));

    public virtual IList<IDataObject> Create(string objectType, IList<string> identifiers)
    {
      try
      {
        if (identifiers == null || identifiers.Count == 0)
        {
          throw new Exception("Identifiers must be specified.");
        }

        LoadDataDictionary(objectType);

        IList<IDataObject> dataObjects = Get(objectType, identifiers);

        foreach (string identifier in identifiers)
        {
          IDataObject dataObject = null;

          var predicate = FormKeyPredicate(identifier);

          if (predicate != null)
          {
            dataObject = dataObjects.AsQueryable().Where(predicate).FirstOrDefault();
          }

          if (dataObject == null)
          {
            dataObject = new GenericDataObject
            {
              ObjectType = objectType,
            };

            SetKeys(dataObject, identifier);

            dataObjects.Add(dataObject);
          }
        }

        return dataObjects;
      }
      catch (Exception ex)
      {
        _logger.Error("Error in Create: " + ex);

        throw new Exception(
          "Error while creating a list of data objects of type [" + objectType + "].",
          ex
        );
      }
    }

    #region Abstract Public Interface Methods
    public abstract long GetCount(string objectType, DataFilter filter);

    public abstract IList<string> GetIdentifiers(string objectType, DataFilter filter);

    public abstract IList<IDataObject> Get(string objectType, IList<string> identifiers);

    public abstract IList<IDataObject> Get(string objectType, DataFilter filter, int pageSize, int startIndex);

    public abstract Response Post(IList<IDataObject> dataObjects);

    public abstract Response Delete(string objectType, IList<string> identifiers);

    public abstract Response Delete(string objectType, DataFilter filter);

    public abstract DataDictionary GetDictionary();

    public abstract IList<IDataObject> GetRelatedObjects(IDataObject dataObject, string relatedObjectType);

    #endregion

    public virtual Response Configure(XElement configuration)
    {
      throw new NotImplementedException();
    }

    public virtual XElement GetConfiguration()
    {
      throw new NotImplementedException();
    }

    protected void LoadDataDictionary(string objectType)
    {
      if (_dataObjectDefinition == null)
      {
        DataDictionary dataDictionary = GetDictionary();

        _dataObjectDefinition =
          dataDictionary.dataObjects.Find(
              o => o.objectName.ToUpper() == objectType.ToUpper()
          );
      }
    }

    protected Expression<Func<IDataObject, bool>> FormMultipleKeysPredicate(IList<string> identifiers)
    {
      Expression<Func<IDataObject, bool>> predicate = null;

      List<Expression> expressions = new List<Expression>();
      foreach (string identifier in identifiers)
      {
        List<Expression> identifierExpressions = BuildKeyFilter(identifier);

        if (identifierExpressions.Count > 1)
        {
          Expression expression = identifierExpressions.First();
          expression.OpenGroupCount++;

          expression = identifierExpressions.Last();
          expression.CloseGroupCount++;
          expression.LogicalOperator = LogicalOperator.Or;
        }
        else
        {
          Expression expression = identifierExpressions.Last();
          expression.LogicalOperator = LogicalOperator.Or;
        }

        expressions.AddRange(identifierExpressions);
      }

      DataFilter dataFilter = new DataFilter
      {
        Expressions = expressions,
      };

      predicate = dataFilter.ToPredicate(_dataObjectDefinition);

      return predicate; 
    }

    protected Expression<Func<IDataObject, bool>> FormKeyPredicate(string identifier)
    {
      Expression<Func<IDataObject, bool>> predicate = null;

      List<Expression> expressions = BuildKeyFilter(identifier);

      DataFilter dataFilter = new DataFilter
      {
        Expressions = expressions,
      };

      predicate = dataFilter.ToPredicate(_dataObjectDefinition);

      return predicate; 
    }

    protected List<Expression> BuildKeyFilter(string identifier)
    {
      List<Expression> expressions = new List<Expression>();

      string[] delimiter = new string[] { _dataObjectDefinition.keyDelimeter };
      string[] identifierParts = identifier.Split(delimiter, StringSplitOptions.None);

      if (identifierParts.Count() == _dataObjectDefinition.keyProperties.Count)
      {
        Expression expression = null;
        int i = 0;
        foreach (KeyProperty keyProperty in _dataObjectDefinition.keyProperties)
        {
          string identifierPart = identifierParts[i];

          expression = new Expression
          {
            PropertyName = keyProperty.keyPropertyName,
            RelationalOperator = RelationalOperator.EqualTo,
            Values = new Values
            {
              identifierPart,
            }
          };

          expressions.Add(expression);
          i++;
        }
      }

      return expressions;
    }

    protected void SetKeys(IDataObject dataObject, string identifier)
    {
      string[] delimiter = new string[] { _dataObjectDefinition.keyDelimeter };
      string[] identifierParts = identifier.Split(delimiter, StringSplitOptions.None);

      if (identifierParts.Count() == _dataObjectDefinition.keyProperties.Count)
      {
        int i = 0;
        foreach (KeyProperty keyProperty in _dataObjectDefinition.keyProperties)
        {
          string identifierPart = identifierParts[i];
          if (!String.IsNullOrEmpty(identifierPart))
          {
            dataObject.SetPropertyValue(keyProperty.keyPropertyName, identifierPart);
          }
          i++;
        }
      }
    }

    protected string GetIdentifier(IDataObject dataObject)
    {
      string[] identifierParts = new string[_dataObjectDefinition.keyProperties.Count];

      int i = 0;
      foreach (KeyProperty keyProperty in _dataObjectDefinition.keyProperties)
      {
        identifierParts[i] = dataObject.GetPropertyValue(keyProperty.keyPropertyName).ToString();
        i++;
      }

      string identifier = String.Join(_dataObjectDefinition.keyDelimeter, identifierParts);

      return identifier;
    }
  }
}
