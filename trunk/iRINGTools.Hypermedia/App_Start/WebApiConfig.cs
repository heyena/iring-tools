using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace iRINGTOOLS.Hypermedia
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {

           // config.Filters.Add(new AuthorizeAttribute());

            //config.Routes.MapHttpRoute(
            //    name: "DefaultApi",
            //    routeTemplate: "api/{controller}/{id}",
            //    defaults: new { id = RouteParameter.Optional }
            //);

            //config.Routes.MapHttpRoute(
            //    name: "DefaultApi",
            //    routeTemplate: "/{app}/{project}/{resource}?format={format}&start={start}&limit={limit}&sortOrder={sortOrder}&sortBy={sortBy}&indexStyle={indexStyle}",
            //    defaults: new { id = RouteParameter.Optional }
            //);

            var appXmlType = config.Formatters.XmlFormatter.SupportedMediaTypes.FirstOrDefault(t => t.MediaType == "application/xml");
            config.Formatters.XmlFormatter.SupportedMediaTypes.Remove(appXmlType);

            //JsonSerializerSettings jSettings = new Newtonsoft.Json.JsonSerializerSettings();
            //GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings = jSettings;

           // config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = Resolver;


            //var json = config.Formatters.JsonFormatter;
            //json.SerializerSettings.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.Objects;
            //config.Formatters.Remove(config.Formatters.XmlFormatter);


        }

        //private static readonly DefaultContractResolver Resolver = new DefaultContractResolver
        //{
        //    IgnoreSerializableAttribute = true
        //};
    }
}
