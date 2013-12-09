using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.ComponentModel;
using System.ServiceModel.Web;
using org.iringtools.services;

using AttributeRouting;
using AttributeRouting.Web.Http;
using org.iringtools.library;
using org.iringtools.adapter;
using System.Configuration;
using System.IO;
using System.Collections;
using System.Web;

namespace org.iringtools.services
{
    [RoutePrefix("data")]
    public class DataController : ApiController
    {
        private HMCommonDataService _hmCommonService = null;

        public DataController()
        {
            _hmCommonService = new HMCommonDataService();
        }

        [GET("{app}/{project}/{resource}/{format?}/{start?}/{limit?}/{sortOrder?}/{sortBy?}/{indexStyle?}")]
        public HttpResponseMessage GetList(string app, string project, string resource, string format = "jsonld", int start = 0, int limit = 0, string sortOrder = null, string sortBy = null, string indexStyle = null)
        {
            try
            {
                _hmCommonService.GetList(project, app, resource, format, start, limit, sortOrder, sortBy, indexStyle);
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
                return response;
                
            }
            catch (Exception ex)
            {
                ExceptionHandler.Handle(ex);
                return null;
            }
        }

        [Description("Gets contexts of an application.")]
        [GET("{app}/contexts/{format=json}")]
        public HttpResponseMessage GetContexts(string app, string format)
        {
            try
            {
                _hmCommonService.GetContexts(app, format);
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
                return response;
            }
            catch (Exception ex)
            {
                ExceptionHandler.Handle(ex);
                return null;
            }
        }

        [Description("Gets version of the service.")]
        [GET("version/{format?}")]
        public HttpResponseMessage GetVersion(string format = "json")
        {
            try
            {
                _hmCommonService.GetVersion(format);
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
                return response;
            }
            catch (Exception ex)
            {
                ExceptionHandler.Handle(ex);
                return null;
            }
        }

        // POST api/data
        public void Post([FromBody]string value)
        {
        }

        // PUT api/data/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/data/5
        public void Delete(int id)
        {
        }

    }
}
