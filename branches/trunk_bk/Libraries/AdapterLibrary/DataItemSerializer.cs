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

    public MemoryStream SerializeToMemoryStream<T>(T graph, bool useDataContractSerializer, bool jsonLD = false )
    {
        MemoryStream stream = new MemoryStream();
        _converter.bJsonLd = jsonLD;
       
        try
        {
        if (useDataContractSerializer)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            serializer.WriteObject(stream, graph);
        }
        else
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue;
            serializer.RegisterConverters(new JavaScriptConverter[] { _converter });
            string json = serializer.Serialize(graph);
  
            byte[] byteArray = Encoding.UTF8.GetBytes(json);
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
      string json = string.Empty;

      try
      {
        MemoryStream stream = SerializeToMemoryStream<T>(graph, useDataContractSerializer);
        stream.Position = 0;
        StreamReader reader = new StreamReader(stream);
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
          DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
          byte[] byteArray = Encoding.Default.GetBytes(json);
          MemoryStream stream = new MemoryStream(byteArray);
          graph = (T)serializer.ReadObject(stream);
        }
        else
        {
          JavaScriptSerializer serializer = new JavaScriptSerializer();
          serializer.MaxJsonLength = int.MaxValue;
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
    private string _hasContentFieldName = "_HAS_CONTENT_";
    private string _contentFieldName = "_CONTENT_";
    private string _contentTypeFieldName = "_CONTENT_TYPE_";
    private bool _displayLinks = false;

    public bool bJsonLd = false;

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
        DataItem dataItem = (DataItem)obj;
        Dictionary<string, object> result = new Dictionary<string, object>();

        if (dataItem != null)
        {
            //FKM
            if (bJsonLd)
            {
               //FKM don't need it for now
                result[_idFieldName] = dataItem.id;
            }
            else
            {
                result[_idFieldName] = dataItem.id;
            }

            if (result.Keys.Contains(_hasContentFieldName))
                result[_hasContentFieldName] = dataItem.hasContent;

            //if (dataItem.hasContent)
            //{
            //    if (dataItem.content != null && dataItem.content.Length > 0)
            //    {
            //        result[_contentFieldName] = dataItem.content;
            //    }
            //}

            foreach (var property in dataItem.properties)
            {
                object value = property.Value;

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

      DataItem dataItem = new DataItem()
      {
        properties = new Dictionary<string, object>(),
      };

      if (dictionary.Keys.Contains(_hasContentFieldName) && bool.Parse(dictionary[_hasContentFieldName].ToString()))
      {
        dataItem.hasContent = true;
      }

      if (dictionary.Keys.Contains(_contentFieldName))
      {
          if (dictionary[_contentFieldName] != null && dictionary[_contentTypeFieldName] != null)
          {
              dataItem.content = dictionary[_contentFieldName].ToString();
              dataItem.contentType = dictionary[_contentTypeFieldName].ToString();
              dataItem.hasContent = true;
          }
          else
          {
              throw new Exception("Invalid content posted, ensure _CONTENT_ and _CONTENT_TYPE_ are used together.");
          }
      }

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
