using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using System.IO;
using System.Runtime.Serialization.Json;
using org.iringtools.library;
using System.Collections.ObjectModel;

namespace org.iringtools.adapter
{
  public class DataItemSerializer
  {
    private DataItemConverter _converter;

    public DataItemSerializer()
    {
      _converter = new DataItemConverter(null, null, false);
    }

    public DataItemSerializer(string idFieldName, string linksFieldName, bool displayLinks)
    {
      _converter = new DataItemConverter(idFieldName, linksFieldName, displayLinks);
    }

    public MemoryStream SerializeToMemoryStream<T>(T graph, bool useDataContractSerializer)
    {
      var stream = new MemoryStream();

      try
      {
        if (useDataContractSerializer)
        {
          var serializer = new DataContractJsonSerializer(typeof(T));
          serializer.WriteObject(stream, graph);
        }
        else
        {
          var serializer = new JavaScriptSerializer();
          serializer.RegisterConverters(new JavaScriptConverter[] { _converter });
          var json = serializer.Serialize(graph);
          var byteArray = Encoding.UTF8.GetBytes(json);
          stream = new MemoryStream(byteArray);
        }
      }
      catch (Exception exception)
      {
        throw new Exception("Error serializing [" + typeof(T).Name + "].", exception);
      }

      return stream;
    }

    public string Serialize<T>(T graph, bool useDataContractSerializer)
    {
      var json = string.Empty;

      try
      {
        var stream = SerializeToMemoryStream<T>(graph, useDataContractSerializer);
        stream.Position = 0;
        var reader = new StreamReader(stream);
        json = reader.ReadToEnd();
        reader.Close();
      }
      catch (Exception exception)
      {
        throw new Exception("Error serializing [" + typeof(T).Name + "].", exception);
      }

      return json;
    }

    public T Deserialize<T>(string json, bool useDataContractSerializer)
    {
      T graph;

      try
      {
        if (useDataContractSerializer)
        {
          var serializer = new DataContractJsonSerializer(typeof(T));
          var byteArray = Encoding.Default.GetBytes(json);
          var stream = new MemoryStream(byteArray);
          graph = (T)serializer.ReadObject(stream);
        }
        else
        {
          var serializer = new JavaScriptSerializer();
          serializer.RegisterConverters(new JavaScriptConverter[] { _converter });
          graph = (T)serializer.Deserialize<T>(json);
        }
      }
      catch (Exception exception)
      {
        throw new Exception("Error deserializing [" + typeof(T).Name + "].", exception);
      }

      return graph;
    }
  }

  public class DataItemConverter : JavaScriptConverter
  {
    private string _idFieldName = "_ID_";
    private string _linksFieldName = "_LINKS_";
    private bool _displayLinks = false;

    public DataItemConverter(string idFieldName, string linksFieldName, bool displayLinks)
    {
      if (!string.IsNullOrEmpty(idFieldName))
        _idFieldName = idFieldName;

      if (!string.IsNullOrEmpty(linksFieldName))
        _linksFieldName = linksFieldName;

      _displayLinks = displayLinks;
    }

    public override IEnumerable<Type> SupportedTypes
    {
      get { return new ReadOnlyCollection<Type>(new List<Type>(new Type[] { typeof(DataItem) })); }
    }

    public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
    {
      var dataItem = (DataItem)obj;
      var result = new Dictionary<string, object>();

      if (dataItem != null)
      {
        result[_idFieldName] = dataItem.id;

        foreach (var property in dataItem.properties)
        {
          result[property.Key] = property.Value;
        }

        if (_displayLinks)
        {
          result[_linksFieldName] = dataItem.links;
        }
      }

      return result;
    }

    public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
    {
      if (dictionary == null)
        return null;

      var dataItem = new DataItem()
      {
        properties = new Dictionary<string, string>()
      };

      foreach (var pair in dictionary)
      {
        if (pair.Key == _idFieldName)
        {
          dataItem.id = Convert.ToString(pair.Value);
        }
        else if (pair.Key != _linksFieldName)
        {
          dataItem.properties[pair.Key] = Convert.ToString(pair.Value);
        }
      }

      return dataItem;
    }
  }
}
