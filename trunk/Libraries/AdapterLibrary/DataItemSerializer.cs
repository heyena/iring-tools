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
    AdapterSettings _settings;

    public DataItemSerializer(AdapterSettings settings)
    {
      _settings = settings;
    }

    public MemoryStream Serialize<T>(T graph, bool useDataContractSerializer)
    {
      MemoryStream stream = new MemoryStream();

      try
      {
        if (!useDataContractSerializer)
        {
          JavaScriptSerializer serializer = new JavaScriptSerializer();
          serializer.RegisterConverters(new JavaScriptConverter[] { new DataItemConverter(_settings) });
          string json = serializer.Serialize(graph);
          byte[] byteArray = Encoding.Default.GetBytes(json);
          stream = new MemoryStream(byteArray);
        }
        else
        {
          DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
          serializer.WriteObject(stream, graph);
          string json = Encoding.Default.GetString(stream.ToArray());
          stream.Close();
        }
      }
      catch (Exception exception)
      {
        throw new Exception("Error serializing [" + typeof(T).Name + "].", exception);
      }

      return stream;
    }
  }

  public class DataItemConverter : JavaScriptConverter
  {
    AdapterSettings _settings;

    public DataItemConverter(AdapterSettings settings)
    {
      _settings = settings;
    }

    public override IEnumerable<Type> SupportedTypes
    {
      get { return new ReadOnlyCollection<Type>(new List<Type>(new Type[] { typeof(DataItem) })); }
    }

    public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
    {
      string jsonIdField = _settings["JsonIdField"];
      string displayLinks = _settings["DisplayLinks"];

      DataItem dataItem = (DataItem)obj;
      Dictionary<string, object> result = new Dictionary<string, object>();

      if (dataItem != null)
      {
        string idFieldName = _settings["JsonIdField"];
        result[idFieldName] = dataItem.id;

        foreach (var property in dataItem.properties)
        {
          result[property.Key] = property.Value;
        }

        if (displayLinks.ToLower() == "true")
        {
          result["links"] = dataItem.links;
        }
      }

      return result;
    }

    public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
    {
      throw new NotImplementedException();
    }
  }
}
