using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace org.iringtools.adapter
{
    public class Container
    {

        public Container()
        {
            //@type = "http://www.w3.org/2001/XMLSchema#string";
            //@Value = "66";
        }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string name { get; set; }

        [JsonProperty("homepage", NullValueHandling = NullValueHandling.Ignore)]
        public string homepage { get; set; }

        [JsonProperty("@id", NullValueHandling = NullValueHandling.Ignore)]
        public string @id { get; set; }

        [JsonProperty("@type", NullValueHandling = NullValueHandling.Ignore)]
        public string @type { get; set; }

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