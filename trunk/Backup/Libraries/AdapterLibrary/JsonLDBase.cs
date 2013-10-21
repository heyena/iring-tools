using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace org.iringtools.adapter
{
    public class JsonLDBase
    {
        [JsonProperty("@graph", NullValueHandling = NullValueHandling.Ignore)]
        public object @graph { get; set; }

        public JsonLDContext JLDContext;

        public JsonLDBase()
        {
            JLDContext = new JsonLDContext();
        }

        public void Serialize()
        {
            @graph = JLDContext.Serialize();
        }

    }
}