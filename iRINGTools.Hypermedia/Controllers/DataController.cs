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
        private CommonDataService _commonService = null;

        public DataController()
        {
            _commonService = new CommonDataService();
        }

        //[GET("links")]
        //[GET("urls")]
        //[POST("postedlinks")]
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        //[GET("{project}/{app}/{resource}/{format=jsonld}/{start=0}/{limit=0}/{sortOrder?}/{sortBy?}/{indexStyle?}")]
        [GET("{app}/{project}/{resource}/{format?}/{start?}/{limit?}/{sortOrder?}/{sortBy?}/{indexStyle?}")]
        public HttpResponseMessage GetList(string app, string project, string resource, string format = "jsonld", int start = 0, int limit = 0, string sortOrder = null, string sortBy = null, string indexStyle = null)
        {
            try
            {
                _commonService.GetList(project, app, resource, format, start, limit, sortOrder, sortBy, indexStyle);
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
                return response;
                
            }
            catch (Exception ex)
            {
                ExceptionHandler.Handle(ex);
                //HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest);
                return null;// response;
            }
        }


        //[Description("Gets a specific object definition from data dictionary. Valid formats are XML, JSON.")]
        //[GET("{app}/{project}/dictionary/{format?}")]
        //public void GetObjectType(string project, string app, string format = "json")
        //{
        //    try
        //    {
        //        _commonService.GetDictionary(project, app, format);
        //    }
        //    catch (Exception ex)
        //    {
        //        //ExceptionHandler(ex);
        //    }
        //}

        [Description("Gets contexts of an application.")]
        [GET("{app}/contexts/{format=json}")]
        public HttpResponseMessage GetContexts(string app, string format)
        {
            try
            {
                _commonService.GetContexts(app, format);
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
                return response;
            }
            catch (Exception ex)
            {
                ExceptionHandler.Handle(ex);
                //HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest);
                return null;// response;
            }
        }

        [Description("Gets version of the service.")]
        [GET("version/{format?}")]
        public HttpResponseMessage GetVersion(string format = "json")
        {
            try
            {
                _commonService.GetVersion(format);
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
                return response;
            }
            catch (Exception ex)
            {
                ExceptionHandler.Handle(ex);
                //HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest);
                return null;// response;
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
