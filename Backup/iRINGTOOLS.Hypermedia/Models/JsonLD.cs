using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;
using System.ComponentModel;
using Newtonsoft.Json.Serialization;
using System.Reflection;


namespace iRINGTOOLS.Hypermedia.Models
{
    public class JsonLD
    {

        public JsonLD()
        {
        }

        public void Setup()
        {
            listtype = new List<string>();
            listtype.Add("http://rdl.rdlfacade.org/data#R49658319833");
            listtype.Add("http://www.w3.org/2002/07/owl#Thing");


            @id = "http://localhost:54321/adata/abc/12345_000/Lines/PlantArea/66";
            @type = listtype;

            
            name = new Container();
        }

        
        private List<string> listtype;


        [JsonProperty(PropertyName = "name")]
        public Container name { get; set; }

        [JsonProperty("@id", NullValueHandling = NullValueHandling.Ignore)]
        public string @id { get; set; }

        [JsonProperty("@type", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> @type { get; set; }

        [JsonProperty("@value", NullValueHandling = NullValueHandling.Ignore)]
        public string @Value { get; set; }

        [JsonProperty("@language", NullValueHandling = NullValueHandling.Ignore)]
        public string @Language { get; set; }

        [JsonProperty("@container", NullValueHandling = NullValueHandling.Ignore)]
        public string @container { get; set; }

        [JsonProperty("@list", NullValueHandling = NullValueHandling.Ignore)]
        public string @list { get; set; }

        [JsonProperty("@set", NullValueHandling = NullValueHandling.Ignore)]
        public string @set { get; set; }

        [JsonProperty("@reverse", NullValueHandling = NullValueHandling.Ignore)]
        public string @reverse { get; set; }

        [JsonProperty("@index", NullValueHandling = NullValueHandling.Ignore)]
        public string @index { get; set; }

        [JsonProperty("@base", NullValueHandling = NullValueHandling.Ignore)]
        public string @base { get; set; }

        [JsonProperty("@vocab", NullValueHandling = NullValueHandling.Ignore)]
        public string @vocab { get; set; }

        [JsonProperty("@graph", NullValueHandling = NullValueHandling.Ignore)]
        public string @graph { get; set; }
    }



    interface IJsonLinkable
    {
        string Id { get; }
        string Context { get; }
    }

    class JsonRefConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((IJsonLinkable)value).Id);
        }

        public override object ReadJson(JsonReader reader, Type type, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.String)
                throw new Exception("Ref value must be a string.");
            return JsonLinkedContext.GetLinkedValue(serializer, type, reader.Value.ToString());
        }

        public override bool CanConvert(Type type)
        {
            return type.IsAssignableFrom(typeof(IJsonLinkable));
        }
    }

    class JsonRefedConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        public override object ReadJson(JsonReader reader, Type type, object existingValue, JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);
            var value = JsonLinkedContext.GetLinkedValue(serializer, type, (string)jo.PropertyValues().First());
            serializer.Populate(jo.CreateReader(), value);
            return value;
        }

        public override bool CanConvert(Type type)
        {
            return type.IsAssignableFrom(typeof(IJsonLinkable));
        }
    }


    class JsonLinkedContext
    {
        private readonly IDictionary<Type, IDictionary<string, object>> links = new Dictionary<Type, IDictionary<string, object>>();

        public static object GetLinkedValue(JsonSerializer serializer, Type type, string reference)
        {
            var context = (JsonLinkedContext)serializer.Context.Context;
            IDictionary<string, object> links;
            if (!context.links.TryGetValue(type, out links))
                context.links[type] = links = new Dictionary<string, object>();
            object value;
            if (!links.TryGetValue(reference, out value))
                links[reference] = value = FormatterServices.GetUninitializedObject(type);
            return value;
        }
    }


}