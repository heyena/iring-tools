using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using iRINGTools.Data;

namespace iRINGTools.Tests
{
  
  public class TestDataObject : IDataObject
  {
    private Dictionary<string, object> _dictionary = new Dictionary<string,object>();

    public string ObjectType { get; set; }

    public int Id { get; set; }

    public object GetPropertyValue(string propertyName)
    {
      if (_dictionary.ContainsKey(propertyName))
      {
        return _dictionary[propertyName];
      }
      else
      {
        return string.Empty;
      }
    }

    public void SetPropertyValue(string propertyName, object value)
    {
      _dictionary[propertyName] = value;
    }
  }

  public class TestDataLayer: IDataLayer
  {
    private Configuration _configuration;
    private LazyList<IDataObject> _dataObjects = new LazyList<IDataObject>();
    
    public string Name { get; private set; }

    public TestDataLayer()
    {
      Name = "Test DataLayer";

      Dictionary dictionary = GetDictionary();
      Random r = new Random();
      foreach (var row in dictionary.DictionaryObjects)
      {
        for (int i = 1; i <= 10; i++)
        {
          var obj = new TestDataObject();
          obj.Id = i;
          obj.ObjectType = "LINES";
          obj.SetPropertyValue("TAG", "TAG" + i.ToString());
          obj.SetPropertyValue("AREA", "AREA" + i.ToString());
          obj.SetPropertyValue("UOM_NOMDIAMETER", "kPM");
          obj.SetPropertyValue("NOMDIAMETER", r.Next(100));
          obj.SetPropertyValue("PIDNUMBER", "PIDNUMBER" + i.ToString());

          _dataObjects.Add(obj);
        }
      }
    }

    public TestDataLayer(Configuration configuration)
      : this()
    {
      _configuration = configuration;
    }

    public IList<IDataObject> Create(string objectType, IList<string> identifiers)
    {
      IList<IDataObject> dataObjects = new List<IDataObject>();

      for (int i = 1; i <= 5; i++)
      {
        TestDataObject dataObject = new TestDataObject();
        dataObject.ObjectType = objectType;
        dataObject.SetPropertyValue("TAG",identifiers[i]);

        _dataObjects.Add(dataObject);
      }

      return dataObjects;
    }

    public IList<IDataObject> Get(string objectType, IList<string> identifiers)
    {
      IList<IDataObject> dataObjects = new List<IDataObject>();

      foreach (IDataObject item in _dataObjects)
      {
        if (identifiers == null)
        {
          dataObjects.Add(item);
        }
        else
        {
          foreach (var id in identifiers)
          {
            if (item.GetPropertyValue("TAG").ToString() == id)
            {
              dataObjects.Add(item);
              break;
            }
          }
        }
      }

      return dataObjects;
    }

    public Response Post(IList<IDataObject> dataObjects)
    {
      var response = new Response();

      foreach (var dataObject in dataObjects)
      {
        Status status = new Status();
        status.Messages = new List<string>();

        string identifier = dataObject.GetPropertyValue("TAG").ToString();
        status.Identifier = identifier;

        _dataObjects.Add(dataObject);

        response.Append(status);
      }

      return response;
    }

    public Response Delete(string objectType, IList<string> identifiers)
    {
      var response = new Response();

      foreach (IDataObject dataObject in _dataObjects)
      {
        Status status = new Status();
        status.Messages = new List<string>();

        string identifier = dataObject.GetPropertyValue("TAG").ToString();
        status.Identifier = identifier;

        foreach (var id in identifiers)
        {
          if (identifier == id)
          {
            _dataObjects.Remove(dataObject);
            response.Append(status);
            break;
          }
        }
      }

      return response;
    }

    public Dictionary GetDictionary()
    {
      return new TestDictionary();
    }

    public IList<IDataObject> GetRelatedObjects(IDataObject dataObject, string relatedObjectType)
    {
      throw new NotImplementedException();
    }

    public long GetCount(string objectType, DataFilter filter)
    {
      return _dataObjects != null ? _dataObjects.Count : 0;
    }

    public IList<string> GetIdentifiers(string objectType, DataFilter filter)
    {
      var results = from o in _dataObjects
                    select o.GetPropertyValue("TAG").ToString();

      return results.ToList();
    }

    public IList<IDataObject> Get(string objectType, DataFilter filter, int pageSize, int startIndex)
    {
      return Get(objectType, null);
    }

    public Response Delete(string objectType, DataFilter filter)
    {
      throw new NotImplementedException();
    }


    public IQueryable<IDataObject> DataObjects
    {
      get
      { 
        return _dataObjects.AsQueryable(); 
      }
    }
  }
}