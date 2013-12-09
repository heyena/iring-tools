// Copyright (c) 2010, iringtools.org //////////////////////////////////////////
// All rights reserved.
//------------------------------------------------------------------------------
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the ids-adi.org nor the
//       names of its contributors may be used to endorse or promote products
//       derived from this software without specific prior written permission.
//------------------------------------------------------------------------------
// THIS SOFTWARE IS PROVIDED BY ids-adi.org ''AS IS'' AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL ids-adi.org BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
////////////////////////////////////////////////////////////////////////////////

using System.ComponentModel;
using System.Configuration;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Xml.Linq;
using log4net;
using org.iringtools.library;
using org.iringtools.utility;
using org.iringtools.adapter;
using System.Collections.Specialized;
using System;
using System.IO;
using System.Net;
using System.Web.Script.Serialization;
using System.Web;
using System.ServiceModel.Channels;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Collections.Generic;
using net.java.dev.wadl;
using System.Collections;

namespace org.iringtools.services
{
    [ServiceContract(Namespace = "http://www.iringtools.org/service")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class AbstractDataService
    {
        private HMCommonDataService _hmCommonService = null;

        public AbstractDataService()
        {
            _hmCommonService = new HMCommonDataService();
        }

        [Description("Gets version of the service.")]
        [WebGet(UriTemplate = "/version?format={format}")]
        public void GetVersion(string format)
        {
            _hmCommonService.GetVersion(format);
        }

        [Description("Gets contexts of an application.")]
        [WebGet(UriTemplate = "/{app}/contexts?format={format}")]
        public void GetContexts(string app, string format)
        {
            _hmCommonService.GetContexts(app, format);
        }

        [Description("Gets the wadl for an application.")]
        [WebGet(UriTemplate = "/{app}?wadl")]
        public void GetAppWADL(string app)
        {
            _hmCommonService.GetAppWADL(app);
        }

        [Description("Gets the wadl for an endpoint.")]
        [WebGet(UriTemplate = "/{app}/{project}?wadl")]
        public void GetScopeWADL(string app, string project)
        {
            _hmCommonService.GetScopeWADL(app, project);
        }

        [Description("Gets data dictionary. Only JSONLD format.")]
        [WebGet(UriTemplate = "/{app}/{project}/dictionary?format={format}")]
        public void GetTipDictionary(string project, string app, string format)
        {
            _hmCommonService.GetTipDictionary(project, app, format);
        }

        [Description("Gets a specific object definition from data dictionary. Only JSONLD format.")]
        [WebGet(UriTemplate = "/{app}/{project}/dictionary/{objectType}?format={format}")]
        public void GetObjectType(string project, string app, string objectType, string format)
        {
            try
            {
                _hmCommonService.GetObjectType(project, app, objectType, format);
            }
            catch (Exception ex)
            {
                ExceptionHandler(ex);
            }
        }

        [Description("Gets resource items. Only JSONLD format.")]
        [WebGet(UriTemplate = "/{app}/{project}/{resource}?format={format}&start={start}&limit={limit}&sortOrder={sortOrder}&sortBy={sortBy}&indexStyle={indexStyle}&related={related}")]
        public void GetList(string project, string app, string resource, string format, int start, int limit, string sortOrder, string sortBy, string indexStyle, bool related)
        {
            try
            {
                _hmCommonService.GetList(project, app, resource, format, start, limit, sortOrder, sortBy, indexStyle);
            }
            catch (Exception ex)
            {
                ExceptionHandler(ex);
            }
        }

        [Description("Gets a specific resource item. Only JSONLD format..")]
        [WebGet(UriTemplate = "/{app}/{project}/{resource}/{id}?format={format}&related={related}")]
        public void GetItem(string project, string app, string resource, string id, string format, bool related)
        {
            try
            {
                _hmCommonService.GetItem(project, app, resource, id, format, related);
            }
            catch (Exception ex)
            {
                ExceptionHandler(ex);
            }
        }

        [Description("Gets picklist items.")]
        [WebGet(UriTemplate = "/{app}/{project}/picklists?format={format}")]
        public void GetPicklists(string project, string app, string format)
        {
            try
            {
                _hmCommonService.GetPicklists(project, app, format);
            }
            catch (Exception ex)
            {
                ExceptionHandler(ex);
            }
        }

        [Description("Gets an XML or JSON of Picklists in the specified project, application in the format specified. Only JSONLD format.")]
        [WebGet(UriTemplate = "/{app}/{project}/picklists/{name}?format={format}&start={start}&limit={limit}")]
        public void GetPicklist(string project, string app, string name, string format, int start, int limit)
        {
            try
            {
                _hmCommonService.GetPicklist(project, app, name, format, start, limit);
            }
            catch (Exception ex)
            {
                ExceptionHandler(ex);
            }
        }

        [Description("Gets resource items with filter. Only JSONLD format..")]
        [WebInvoke(Method = "POST", UriTemplate = "/{app}/{project}/{resource}/filter?format={format}&start={start}&limit={limit}&indexStyle={indexStyle}")]
        public void GetWithFilter(string project, string app, string resource, string format, int start, int limit, string indexStyle, Stream stream)
        {
            try
            {
                _hmCommonService.GetWithFilter(project, app, resource, format, start, limit, indexStyle, stream);
            }
            catch (Exception ex)
            {
                ExceptionHandler(ex);
            }
        }

        [Description("Gets an XML projection of the specified scope and resource. Only JSONLD format.")]
        [WebGet(UriTemplate = "/{app}/{project}/{resource}/search?q={query}&format={format}&start={start}&limit={limit}&sortOrder={sortOrder}&sortBy={sortBy}&indexStyle={indexStyle}")]
        public void Search(string project, string app, string resource, string query, string format, int start, int limit, string sortOrder, string sortBy, string indexStyle)
        {
            try
            {
                _hmCommonService.Search(project, app, resource, query, format, start, limit, sortOrder, sortBy, indexStyle);
            }
            catch (Exception ex)
            {
                ExceptionHandler(ex);
            }
        }

        //NOTE: Due to uri conflict, this template serves both part 7 individual and non-part7 related items.
        [Description("Gets related resource items. Only JSONLD format.")]
        [WebGet(UriTemplate = "/{app}/{project}/{resource}/{id}/{related}?format={format}&start={start}&limit={limit}&sortOrder={sortOrder}&sortBy={sortBy}&indexStyle={indexStyle}")]
        public void GetRelatedList(string project, string app, string resource, string id, string related, string format, int start, int limit, string sortOrder, string sortBy, string indexStyle)
        {
            try
            {
                _hmCommonService.GetRelatedList(project, app, resource, id, related, format, start, limit, sortOrder, sortBy, indexStyle);
            }
            catch (Exception ex)
            {
                ExceptionHandler(ex);
            }
        }

        [Description("Gets a specific related resource item. Only JSONLD format.")]
        [WebGet(UriTemplate = "/{app}/{project}/{resource}/{id}/{related}/{relatedId}?format={format}")]
        public void GetRelatedItem(string project, string app, string resource, string id, string related, string relatedId, string format)
        {
            try
            {
                _hmCommonService.GetRelatedItem(project, app, resource, id, related, relatedId, format);
            }
            catch (Exception ex)
            {
                ExceptionHandler(ex);
            }
        }

        [Description("Updates resource item(s). Only JSONLD format.")]
        [WebInvoke(Method = "PUT", UriTemplate = "/{app}/{project}/{resource}?format={format}")]
        public void UpdateList(string project, string app, string resource, string format, Stream stream)
        {
            _hmCommonService.UpdateList(project, app, resource, format, stream);
        }

        [Description("Updates related resource item(s). Only JSONLD format.")]
        [WebInvoke(Method = "PUT", UriTemplate = "/{app}/{project}/{resource}/{parentid}/{relatedResource}?format={format}")]
        public void UpdateRelatedList(string project, string app, string resource, string parentid, string relatedResource, string format, Stream stream)
        {
            _hmCommonService.UpdateRelatedList(project, app, resource, parentid, relatedResource, format, stream);
        }

        [Description("Updates resource item(s). Only JSONLD format.")]
        [WebInvoke(Method = "PUT", UriTemplate = "/{app}/{project}/{resource}/{id}?format={format}")]
        public void UpdateItem(string project, string app, string resource, string id, string format, Stream stream)
        {
            _hmCommonService.UpdateItem(project, app, resource, id, format, stream);
        }

        [Description("Updates related resource item(s). Only JSONLD format.")]
        [WebInvoke(Method = "PUT", UriTemplate = "/{app}/{project}/{resource}/{parentid}/{relatedresource}/{id}?format={format}")]
        public void UpdateRelatedItem(string project, string app, string resource, string parentid, string relatedresource, string id, string format, Stream stream)
        {
            _hmCommonService.UpdateRelatedItem(project, app, resource, parentid, relatedresource, id, format, stream);
        }

        [Description("Creates new resource item(s). Only JSONLD format.")]
        [WebInvoke(Method = "POST", UriTemplate = "/{app}/{project}/{resource}?format={format}")]
        public void CreateItem(string project, string app, string resource, string format, Stream stream)
        {
            _hmCommonService.CreateItem(project, app, resource, format, stream);
        }

        [Description("Creates new related resource item(s). Only JSONLD format.")]
        [WebInvoke(Method = "POST", UriTemplate = "/{app}/{project}/{resource}/{parentid}/{relatedResource}?format={format}")]
        public void CreateRelatedItem(string project, string app, string resource, string format, string parentid, string relatedResource, Stream stream)
        {
            _hmCommonService.CreateRelatedItem(project, app, resource, format, parentid, relatedResource, stream);
        }

        [Description("Deletes a resource item. Only JSONLD format.")]
        [WebInvoke(Method = "DELETE", UriTemplate = "/{app}/{project}/{resource}/{id}?format={format}")]
        public void DeleteItem(string project, string app, string resource, string id, string format)
        {
            try
            {
                _hmCommonService.DeleteItem(project, app, resource, id, format);
            }
            catch (Exception ex)
            {
                ExceptionHandler(ex);
            }
        }

        [Description("Deletes a resource in the specified application.")]
        [WebInvoke(Method = "DELETE", UriTemplate = "/{app}/{project}/{resource}/{parentid}/{relatedresource}/{id}?format={format}")]
        public void DeleteRelatedItem(string project, string app, string resource, string parentid, string relatedresource, string id, string format)
        {
            try
            {
                _hmCommonService.DeleteRelatedItem(project, app, resource, parentid, relatedresource, id, format);
            }
            catch (Exception ex)
            {
                ExceptionHandler(ex);
            }
        }

        #region Async request queue
        [Description("Gets status of a asynchronous request.")]
        [WebGet(UriTemplate = "/requests/{id}")]
        public void GetRequestStatus(string id)
        {
            _hmCommonService.GetRequestStatus(id);
        }
        #endregion

        #region Private Methods
        private void ExceptionHandler(Exception ex)
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

        private void FormatOutgoingMessage<T>(T graph, string format, bool useDataContractSerializer)
        {
            if (format.ToUpper() == "JSON")
            {
                string json = Utility.SerializeJson<T>(graph, useDataContractSerializer);

                HttpContext.Current.Response.ContentType = "application/json; charset=utf-8";
                HttpContext.Current.Response.Write(json);
            }
            else
            {
                string xml = Utility.Serialize<T>(graph, useDataContractSerializer);

                HttpContext.Current.Response.ContentType = "application/xml";
                HttpContext.Current.Response.Write(xml);
            }
        }
        #endregion
    }
}