using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Http;
using Mock.RestAPIServer.Models;

namespace Mock.RestAPIServer.Controllers
{
    public class SchemaController : ApiController
    {
        //
        // GET: /Schema/

        public IDictionary<string, object> Get(string id)
        {
            string resource = id;
            IDictionary<string, object> schema = new Dictionary<string,object>();

            if (resource.ToUpper() == "FUNCTION")
            {
                schema.Add("links", new Links() { key = new List<string>() { "Id" }, self = "/api/Function/{Id}", relation = new List<object>() });
                schema.Add("Id", new ObjectDefination() { type = "number", size = "0" });
                schema.Add("Name", new ObjectDefination() { type = "string", size = "255" });
            }
            else if (resource.ToUpper() == "PROJECT")
            {
                schema.Add("links", new Links() { key = new List<string>() { "Id", "Name" }, self = "/api/Project/{Id}_{Name}", relation = new List<object>() });
                schema.Add("Id", new ObjectDefination() { type = "number", size = "0" });
                schema.Add("Name", new ObjectDefination() { type = "string", size = "25" });
                schema.Add("Description", new ObjectDefination() { type = "string", size = "255" });
                schema.Add("UpdateOn", new ObjectDefination() { type = "date", size = "0" });

            }
            return schema; 
        }

    }
}
