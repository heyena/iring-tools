using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace iRINGTOOLS.Hypermedia.Models
{
    public class JsonLDBase
    {
        [JsonProperty("@graph", NullValueHandling = NullValueHandling.Ignore)]
        public object @graph { get; set; }

        public JsonLDBase()
        {
            if (@graph == null)
            {
                @graph = new JsonLDContext().Serialize();
            }
        }

    }
}