using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AttributeRouting;
using AttributeRouting.Web.Http;
using org.iringtools.services;

namespace iRINGTOOLS.Hypermedia.Controllers
{
    [RoutePrefix("generate")]
    public class TipGeneratorController : ApiController
    {
        private HMCommonDataService _hmCommonService = null;

        public TipGeneratorController()
        {
            _hmCommonService = new HMCommonDataService();
        }

        [GET("{app}/{project}/{resource}")]
        public void GetList(string app, string project, string resource)
        {
            try
            {
                _hmCommonService.GenerateTIP(project, app, resource);
            }
            catch (Exception ex)
            {
                ExceptionHandler.Handle(ex);
            }
        }
    }
}
