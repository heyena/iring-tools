using System;
using System.Web.Mvc;
using Newtonsoft.Json;

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

      var settings = new JsonSerializerSettings
      {
        NullValueHandling = NullValueHandling.Ignore,
        DefaultValueHandling = DefaultValueHandling.Ignore,
        DateFormatHandling = DateFormatHandling.MicrosoftDateFormat
      };
      var serializedObject = JsonConvert.SerializeObject(Data, Newtonsoft.Json.Formatting.Indented, settings);
      response.Write(serializedObject);
    }
  }
}