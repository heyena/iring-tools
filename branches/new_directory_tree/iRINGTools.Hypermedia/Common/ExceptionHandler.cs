using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel.Web;
using System.IO;
using System.Net;
using System.Collections;


namespace org.iringtools.services
{
    public static class ExceptionHandler
    {
        #region Private Methods
        public static void Handle(Exception ex)
        {
            OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
            string statusText = string.Empty;

            if (ex is FileNotFoundException)
            {
                context.StatusCode = HttpStatusCode.NotFound;
            }
            else if (ex is UnauthorizedAccessException)
            {
                context.StatusCode = HttpStatusCode.Unauthorized;
            }
            else
            {
                context.StatusCode = HttpStatusCode.InternalServerError;

                if (ex is WebFaultException && ex.Data != null)
                {
                    foreach (DictionaryEntry entry in ex.Data)
                    {
                        statusText += ex.Data[entry.Key].ToString();
                    }
                }
            }

            if (string.IsNullOrEmpty(statusText))
            {
                statusText = ex.Source + ": " + ex.ToString();
            }

            HttpContext.Current.Response.ContentType = "text/html";
            HttpContext.Current.Response.Write(statusText);
        }

        //private void FormatOutgoingMessage<T>(T graph, string format, bool useDataContractSerializer)
        //{
        //    if (format.ToUpper() == "JSON")
        //    {
        //        string json = Utility.SerializeJson<T>(graph, useDataContractSerializer);

        //        HttpContext.Current.Response.ContentType = "application/json; charset=utf-8";
        //        HttpContext.Current.Response.Write(json);
        //    }
        //    else
        //    {
        //        string xml = Utility.Serialize<T>(graph, useDataContractSerializer);

        //        HttpContext.Current.Response.ContentType = "application/xml";
        //        HttpContext.Current.Response.Write(xml);
        //    }
        //}
        #endregion
    }
}