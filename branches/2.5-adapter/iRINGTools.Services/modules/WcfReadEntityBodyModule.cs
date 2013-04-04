using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace org.iringtools.services.modules
{
  public class WcfReadEntityBodyModule : IHttpModule
  {
    public void Dispose() { }

    public void Init(HttpApplication context)
    {
      context.BeginRequest += context_BeginRequest;
    }

    void context_BeginRequest(object sender, EventArgs e)
    {
      //This will force the HttpContext.Request.ReadEntityBody to be "Classic" and will ensure compatibility with .NET 4 application in a .NET 4.5 environment.
if not in .NET 4.5 environment remove module from web.config 
      var stream = (sender as HttpApplication).Request.InputStream;
    }    
  }
}