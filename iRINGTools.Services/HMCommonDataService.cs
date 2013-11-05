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
using org.iringtools.utility;
using org.iringtools.library.tip;

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

        new public void GetObjectType(string project, string app, string objectType, string format)
        {
            format = "jsonld";

            DataDictionary dictionary = _abstractPrivder.GetDictionary(project, app);

            DataObject dataObject = dictionary.dataObjects.Find(o => o.objectName.ToLower() == objectType.ToLower());

            if (dataObject == null)
                throw new FileNotFoundException();

            _abstractPrivder.FormatOutgoingMessage<DataObject>(dataObject, format, true);
        }

        new public void GetPicklists(string project, string app, string format)
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

        public void GetPicklist(string project, string app, string name, string format, int start, int limit)
        {
            format = "jsonld";
            try
            {
                Picklists obj = _abstractPrivder.GetPicklist(project, app, name, format, start, limit);

                XElement xml = obj.ToXElement<Picklists>();

                _abstractPrivder.FormatOutgoingMessage(xml, format);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Search(string project, string app, string resource, string query, string format, int start, int limit, string sortOrder, string sortBy, string indexStyle)
        {
            try
            {
                NameValueCollection parameters = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters;

                bool fullIndex = false;
                if (indexStyle != null && indexStyle.ToUpper() == "FULL")
                    fullIndex = true;

                XDocument xDocument = _abstractPrivder.Search(project, app, resource, ref format, query, start, limit, sortOrder, sortBy, fullIndex, parameters);
                _abstractPrivder.FormatOutgoingMessage(xDocument.Root, format);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        new public void GetWithFilter(string project, string app, string resource, string format, int start, int limit, string indexStyle, Stream stream)
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

        new public void GetRelatedItem(string project, string app, string resource, string id, string related, string relatedId, string format)
        {
            try
            {
                format = "jsonld";
                XDocument xDocument = _abstractPrivder.GetRelatedItem(project, app, resource, id, related, relatedId, ref format);
                _abstractPrivder.FormatOutgoingMessage(xDocument.Root, format);
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

        new public void UpdateRelatedItem(string project, string app, string resource, string parentid, string relatedresource, string id, string format, Stream stream)
        {
            Response response = new Response();

            try
            {
                format = "jsonld";

                DataItems dataItems = _abstractPrivder.FormatIncomingMessage(stream);

                if (dataItems != null && dataItems.items != null && dataItems.items.Count > 0)
                {
                    dataItems.items[0].id = id;
                    XElement xElement = dataItems.ToXElement();
                    response = _abstractPrivder.UpdateRelated(project, app, resource, parentid, relatedresource, format, new XDocument(xElement));
                }
                else
                {
                    throw new Exception("No items to update or invalid payload.");
                }
 
            }
            catch (Exception e)
            {
                response.Level = StatusLevel.Error;
                response.Messages.Add(e.Message);
            }

            PrepareResponse(ref response);
            _abstractPrivder.FormatOutgoingMessage<Response>(response, format, false);
        }


        public void UpdateItem(string project, string app, string resource, string id, string format, Stream stream)
        {
            Response response = new Response();

            try
            {
                format = "jsonld";

                DataItems dataItems = _abstractPrivder.FormatIncomingMessage(stream);

                if (dataItems != null && dataItems.items != null && dataItems.items.Count > 0)
                {
                    dataItems.items[0].id = id;
                    XElement xElement = dataItems.ToXElement();
                    response = _abstractPrivder.Update(project, app, resource, format, new XDocument(xElement));
                }
                else
                {
                    throw new Exception("No items to update or invalid payload.");
                }
            }
            catch (Exception e)
            {
                response.Level = StatusLevel.Error;
                response.Messages.Add(e.Message);
            }

            PrepareResponse(ref response);
            _abstractPrivder.FormatOutgoingMessage<Response>(response, format, false);
        }

        public void DeleteRelatedItem(string project, string app, string resource, string parentid, string relatedresource, string id, string format)
        {
            Response response = new Response();

            try
            {
                format = "jsonld";

                response = _abstractPrivder.DeleteRelated(project, app, resource, parentid, relatedresource, id, format);
            }
            catch (Exception ex)
            {
                response.Level = StatusLevel.Error;
                response.Messages.Add(ex.Message);
            }

            PrepareResponse(ref response);
            _abstractPrivder.FormatOutgoingMessage<Response>(response, format, false);
        }

        public void DeleteItem(string project, string app, string resource, string id, string format)
        {
            Response response = new Response();

            try
            {
                format = "jsonld";

                response = _abstractPrivder.DeleteItem(project, app, resource, id, format);

            }
            catch (Exception ex)
            {
                response.Level = StatusLevel.Error;
                response.Messages.Add(ex.Message);
            }

            PrepareResponse(ref response);
            _abstractPrivder.FormatOutgoingMessage<Response>(response, format, false);
        }

        public void CreateRelatedItem(string project, string app, string resource, string format, string parentid, string relatedResource, Stream stream)
        {
            Response response = new Response();

            try
            {
                format = "jsonld";

                XElement xElement = _abstractPrivder.FormatIncomingMessage(stream, format);

                response = _abstractPrivder.UpdateRelated(project, app, resource, parentid, relatedResource, format, new XDocument(xElement));
            }
            catch (Exception e)
            {
                response.Level = StatusLevel.Error;
                response.Messages.Add(e.Message);
            }

            PrepareResponse(ref response);
            _abstractPrivder.FormatOutgoingMessage<Response>(response, format, false);
        }

        public void CreateItem(string project, string app, string resource, string format, Stream stream)
        {
            Response response = new Response();

            try
            {
                format = "jsonld";

                XElement xElement = _abstractPrivder.FormatIncomingMessage(stream, format);

                response = _abstractPrivder.Create(project, app, resource, format, new XDocument(xElement));
            }
            catch (Exception e)
            {
                response.Level = StatusLevel.Error;
                response.Messages.Add(e.Message);
            }

            PrepareResponse(ref response);
            _abstractPrivder.FormatOutgoingMessage<Response>(response, format, false);
        }

        public void UpdateList(string project, string app, string resource, string format, Stream stream)
        {
            Response response = new Response();

            try
            {
                format = "jsonld";

                 XElement xElement = _abstractPrivder.FormatIncomingMessage(stream, format);

                 response = _abstractPrivder.Update(project, app, resource, format, new XDocument(xElement));

            }
            catch (Exception e)
            {
                response.Level = StatusLevel.Error;
                response.Messages.Add(e.Message);
            }

            _abstractPrivder.FormatOutgoingMessage<Response>(response, format, false);
        }

        public void UpdateRelatedList(string project, string app, string resource, string parentid, string relatedResource, string format, Stream stream)
        {
            Response response = new Response();

            try
            {
                format = "jsonld";


                XElement xElement = _abstractPrivder.FormatIncomingMessage(stream, format);

                response = _abstractPrivder.UpdateRelated(project, app, resource, parentid, relatedResource, format, new XDocument(xElement));

            }
            catch (Exception e)
            {
                response.Level = StatusLevel.Error;
                response.Messages.Add(e.Message);
            }

            _abstractPrivder.FormatOutgoingMessage<Response>(response, format, false);
        }

        private void PrepareResponse(ref Response response)
        {
            if (response.Level == StatusLevel.Success)
            {
                response.StatusCode = HttpStatusCode.OK;
            }
            else
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
            }

            if (response.Messages != null)
            {
                foreach (string msg in response.Messages)
                {
                    response.StatusText += msg;
                }
            }

            foreach (Status status in response.StatusList)
            {
                foreach (string msg in status.Messages)
                {
                    response.StatusText += msg;
                }
            }
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

        public void GetRelatedList(string project, string app, string resource, string id, string related, string format, int start, int limit, string sortOrder, string sortBy, string indexStyle)
        {
            try
            {
                format = "jsonld";

                NameValueCollection parameters = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters;

                bool fullIndex = false;
                if (indexStyle != null && indexStyle.ToUpper() == "FULL")
                    fullIndex = true;

                XDocument xDocument = _abstractPrivder.GetRelatedList(project, app, resource, id, related, ref format, start, limit, sortOrder, sortBy, fullIndex, parameters);
                _abstractPrivder.FormatOutgoingMessage(xDocument.Root, format);
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