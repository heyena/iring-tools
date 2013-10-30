using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using org.iringtools.adapter;
using System.Configuration;
using System.Xml.Linq;
using System.Collections.Specialized;
using org.iringtools.library;
using System.ServiceModel.Web;
using System.Net;
using System.IO;

namespace org.iringtools.services
{
    public class HMCommonDataService : CommonDataService
    {
        private AbstractProvider _abstractPrivder = null;

        public HMCommonDataService()
        {
            _abstractPrivder = new AbstractProvider(ConfigurationManager.AppSettings);

        }

        public void GenerateTIP(string project, string app, string resource)
        {
            TipMapping context = _abstractPrivder.GenerateTIP(project, app, resource);
            _abstractPrivder.FormatOutgoingMessage<TipMapping>(context, "json", true);
        }

        new public void GetVersion(string format)
        {

            VersionInfo version = _abstractPrivder.GetVersion();

            _abstractPrivder.FormatOutgoingMessage<VersionInfo>(version, "json", true);
        }

        public void GetTipDictionary(string project, string app, string format)
        {
            try
            {
                format = "jsonld";

                if (IsAsync())
                {
                    string statusUrl = _abstractPrivder.AsyncGetTipDictionary(project, app);
                    WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.Accepted;
                    WebOperationContext.Current.OutgoingResponse.Headers["location"] = statusUrl;
                }
                else
                {
                    TipMapping tipDictionary = _abstractPrivder.GetTipDictionary(project, app, format);
                    _abstractPrivder.FormatOutgoingMessage<TipMapping>(tipDictionary, format, true);
                }
            }
            catch (Exception ex)
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.InternalServerError;
                HttpContext.Current.Response.ContentType = "text/plain";
                HttpContext.Current.Response.Write(ex.Message);
            }
        }

        public void GetPicklists(string project, string app, string format)
        {
            try
            {
                format = "jsonld";
                IList<PicklistObject> objs = _abstractPrivder.GetPicklists(project, app, format);
                _abstractPrivder.FormatOutgoingMessage<IList<PicklistObject>>(objs, format, false);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GetWithFilter(string project, string app, string resource, string format, int start, int limit, string indexStyle, Stream stream)
        {
            try
            {
                format = "jsonld";

                bool fullIndex = false;

                if (indexStyle != null && indexStyle.ToUpper() == "FULL")
                    fullIndex = true;

                if (IsAsync())
                {
                    DataFilter filter = _abstractPrivder.FormatIncomingMessage<DataFilter>(stream, format, true);
                    string statusUrl = _abstractPrivder.AsyncGetWithFilter(project, app, resource, format,
                      start, limit, fullIndex, filter);
                    WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.Accepted;
                    WebOperationContext.Current.OutgoingResponse.Headers["location"] = statusUrl;
                }
                else
                {
                    DataFilter filter = _abstractPrivder.FormatIncomingMessage<DataFilter>(stream, format, true);
                    XDocument xDocument = null;

                    xDocument = _abstractPrivder.GetWithFilter(project, app, resource, filter, ref format, start, limit, fullIndex);

                    _abstractPrivder.FormatOutgoingMessage(xDocument.Root, format);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        new public void GetContexts(string app, string format)
        {

            Contexts contexts = _abstractPrivder.GetContexts(app);

            _abstractPrivder.FormatOutgoingMessage<Contexts>(contexts, "json", true);
        }

        new public void GetList(string project, string app, string resource, string format, int start, int limit, string sortOrder, string sortBy, string indexStyle)
        {
            
            try
            {
                format = "jsonld";

                NameValueCollection parameters = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters;

                bool fullIndex = false;
                if (indexStyle != null && indexStyle.ToUpper() == "FULL")
                    fullIndex = true;

                XDocument xDocument = _abstractPrivder.GetList(project, app, resource, ref format, start, limit, sortOrder, sortBy, fullIndex, parameters);
                _abstractPrivder.FormatOutgoingMessage(xDocument.Root, format);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        new public void GetItem(string project, string app, string resource, string id, string format)
        {
            try
            {
                format = "jsonld";
                object content = _abstractPrivder.GetItem(project, app, resource, String.Empty, id, ref format, false);
                _abstractPrivder.FormatOutgoingMessage(content, format);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool IsAsync()
        {
            bool async = false;
            string asyncHeader = WebOperationContext.Current.IncomingRequest.Headers["async"];

            if (asyncHeader != null && asyncHeader.ToLower() == "true")
            {
                async = true;
            }

            return async;
        }
    }
}