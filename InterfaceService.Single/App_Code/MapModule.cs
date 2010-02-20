using System;
using System.Web;

public class MapModule : IHttpModule
{

    private HttpApplication context = null;

    public void Dispose()
    {
        context.BeginRequest -= new EventHandler(context_BeginRequest);
    }

    public void Init(HttpApplication context)
    {
        this.context = context;
        this.context.BeginRequest += new EventHandler(context_BeginRequest);
    }

    void context_BeginRequest(object sender, EventArgs e)
    {
        HttpApplication app = (HttpApplication)sender;
        app.Response.ContentType = "text/plain";

        if (app.Request.Path.EndsWith("sparql"))
            app.Server.MapPath("InterfaceService/sparql");
    }
}
