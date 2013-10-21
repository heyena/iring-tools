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


namespace org.iringtools.adapter
{
    public class JsonLD
    {

        public JsonLD()
        {
            listtype = new List<string>();
            name = new Container();
        }

        //public void Setup()
        //{
        //    listtype = new List<string>();
        //   // listtype.Add("http://rdl.rdlfacade.org/data#R49658319833");
        //   // listtype.Add("http://www.w3.org/2002/07/owl#Thing");


        //   // @id = "http://localhost:54321/adata/abc/12345_000/Lines/PlantArea/66";
        //   // @type = listtype;

            
        //    name = new Container();
        //}

        public void AddListType(string str)
        {
            listtype.Add(str);
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

}