using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace org.iringtools.web.Helpers
{
   public class JsonNetResult : JsonResult
  {
    public override void ExecuteResult(ControllerContext context)
    {
      if (context == null)
        throw new ArgumentNullException("context");

      var response = context.HttpContext.Response;

      response.ContentType = !String.IsNullOrEmpty(ContentType) ? ContentType : "application/json";

      if (ContentEncoding != null)
        response.ContentEncoding = ContentEncoding;

      if (Data == null)
        return;

      // If you need special handling, you can call another form of SerializeObject below
      var serializedObject = JsonConvert.SerializeObject(
        Data, 
        Formatting.Indented, 
        new JsonSerializerSettings 
        { 
          NullValueHandling = NullValueHandling.Ignore,
          DefaultValueHandling = DefaultValueHandling.Ignore,
          DateFormatHandling = DateFormatHandling.MicrosoftDateFormat
          //ContractResolver = new CamelCasePropertyNamesContractResolver() 
        }
      );
      
      response.Write(serializedObject);
    }
  }
}
