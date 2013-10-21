using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using System.Dynamic;
using Newtonsoft.Json.Converters;
using org.iringtools.library;
using System.ServiceModel.Web;
using org.iringtools.nhibernate;
using System.Web.Configuration;
using Newtonsoft.Json.Serialization;
using System.Reflection;

namespace org.iringtools.adapter
{

    public class ContractResolver : DefaultContractResolver
    {
        public string name { get; set; }

        public ContractResolver(string name)
        {
            this.name = name;
        }
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            if (member.DeclaringType == typeof(JsonLD) && member.Name == "name")
            {
                property.PropertyName = name;
            }

            return property;
        }
    }

    public class JsonLDContext
    {
        public JsonLDContext()
        {
            @graph = new List<JsonLD>();
            //JsonLD ct1 = new JsonLD();
            //JsonLD ct2 = new JsonLD();
            //ct1.Setup();
            //ct2.Setup();
            //@graph.Add(ct1);
            //@graph.Add(ct2);
        }

        public void AddGraph(JsonLD JLD)
        {
            @graph.Add(JLD);
        }

        [JsonProperty("@graph", NullValueHandling = NullValueHandling.Ignore)]
        public List<JsonLD> @graph { get; set; }

        public object Serialize()
        {
            var settings = new JsonSerializerSettings();
            settings.ContractResolver = new ContractResolver("http://msn.com/loop/Goon");
            var @context = JsonConvert.SerializeObject(@graph, Formatting.None, settings);
            object obj = JsonConvert.DeserializeObject(@context);
            return obj;
        }

    }

}