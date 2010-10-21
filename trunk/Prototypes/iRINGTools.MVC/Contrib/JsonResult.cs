using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Xml;
using System.Xml.Serialization;

namespace org.iringtools.client.Contrib
{
  public class MyJsonResult : JsonResult
  {
    private object _objectToSerialize = null;
    private bool _useDataContract = false;

    public MyJsonResult(object objectToSerialize)
      : this(objectToSerialize, true)
    {
    }

    public MyJsonResult(object objectToSerialize, bool useDataContract)
    {
      _objectToSerialize = objectToSerialize;
      _useDataContract = useDataContract;
    }

    public override void ExecuteResult(ControllerContext context)
    {
      if (_objectToSerialize != null)
      {

        if (_useDataContract)
        { 
          
          var xs = new DataContractJsonSerializer(_objectToSerialize.GetType());
          context.HttpContext.Response.ContentType = "application/json";
                    
          var writer = XmlWriter.Create(context.HttpContext.Response.Output);
          xs.WriteObject(writer, _objectToSerialize);
          writer.Close();
        }
        else
        {
          //var xs = new JsonSerializer(_objectToSerialize.GetType());
          //context.HttpContext.Response.ContentType = "application/json";
          //xs.Serialize(context.HttpContext.Response.Output, _objectToSerialize);
        }
      }
    }
  }
}