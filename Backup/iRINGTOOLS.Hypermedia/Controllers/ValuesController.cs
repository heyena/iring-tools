using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using org.iringtools.services;
using iRINGTOOLS.Hypermedia.Models;
using System.ComponentModel;
using System.ServiceModel.Web;
using Newtonsoft.Json;
using org.iringtools.nhibernate;
using System.Web.Configuration;
using org.iringtools.library;
using AttributeRouting;
using AttributeRouting.Web.Http;

namespace iRINGTOOLS.Hypermedia.Controllers
{
    public class SimpleModel
    {
        public string id {get;set;}
        public string name { get; set; }
    }

    //[RoutePrefix("mock")]
    public class ValuesController : ApiController
    {
        private CommonDataService _commonService = null;


        public ValuesController()
        {
          _commonService = new CommonDataService();
        }

        //[Description("Gets the wadl for an endpoint.")]
        //[WebGet(UriTemplate = "/{app}/{project}?wadl")]
        //public string GetScopeWADL(string app, string project)
        //{
        //    _commonService.GetScopeWADL(app, project);
        //    return "Hello";
        //}




        // GET api/values
        public IEnumerable<string> Get()
        {
            

            return new string[] { "value1", "value2" };
        }

        public CommonDataService Get(int id)
        {
            //JsonLDContext JD = new JsonLDContext();
            ////return JD.JsonSerializer();
            //return JD;

            try
            {
                _commonService.GetList("12345_000", "def", "lines", "jsonld", 0, 0, null, null, null);
                // _commonService.GetList(project, app, resource, format, start, limit, sortOrder, sortBy, indexStyle);
                return _commonService;

            }
            catch (Exception ex)
            {
                // ExceptionHandler(ex);
                return null;
            }
        }

        //public object Get(int id)
        //{
        //    // JsonLDContext JD = new JsonLDContext();
        //    // return JD;
        //    JsonLDBase JD = new JsonLDBase();

        //    return JD;
        //}

        //public string Get(int id)
        //{
        //    return MyData();
        //}


        

        //public CommonDataService Get(int id)
        //{
        //   // _commonService.GetScopeWADL("ABC", "12345_000");
        //    //_commonService.GetContexts("ABC", "JSON");
        //    _commonService.GetAppWADL("ABC");
        //    //return JsonConvert.SerializeObject(_commonService);
        //    return _commonService;
        //}

        // POST api/values
        //MOCK TIP Mapping Service
        [POST("mock")]
        public List<SimpleModel> Post([FromBody]List<SimpleModel> model)
        {
            return model;

        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}