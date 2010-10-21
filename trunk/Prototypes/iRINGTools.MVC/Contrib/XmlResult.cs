using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;


namespace org.iringtools.client.Contrib
{
  public class XmlResult : ActionResult
  {
    private object _objectToSerialize = null;
    private bool _useDataContract = false;

    public XmlResult(object objectToSerialize): this(objectToSerialize, true)
    {             
    }

    public XmlResult(object objectToSerialize, bool useDataContract)
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
          var xs = new DataContractSerializer(_objectToSerialize.GetType());
          context.HttpContext.Response.ContentType = "text/xml";
          var writer = XmlWriter.Create(context.HttpContext.Response.Output);          
          xs.WriteObject(writer, _objectToSerialize);
          writer.Close();
        }
        else
        {
          var xs = new XmlSerializer(_objectToSerialize.GetType());
	        context.HttpContext.Response.ContentType = "text/xml";
	        xs.Serialize(context.HttpContext.Response.Output, _objectToSerialize);
        }
      }
    }
  }
}