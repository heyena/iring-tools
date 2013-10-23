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
        private CommonDataService _commonService = null;

        public TipGeneratorController()
        {
            _commonService = new CommonDataService();
        }

        [GET("{app}/{project}/{resource}")]
        public void GetList(string app, string project, string resource)
        {
            try
            {

                _commonService.GenerateTIP(project, app, resource);

                var client = new TipRESTClient();
                client.EndPoint = @"http://localhost:8080/services/tip/findparameters";
                client.ContentType = "application/xml";

                string str = @"<?xml version='1.0'?><tipRequest xmlns=""http://www.iringtools.org/tipmapping"">
                <parameterMaps>
                    <parameterMap>
                        <path>rdl:R19192462550/tpl:R5E442F596F5C45CC8BECBEA04EC8D889(1)/tpl:R0BA20392093943E7896A5B36CAB56394/rdl:R21605619301/tpl:RBFF16CD8F3BB4BFE8539B683D6782F0B/tpl:R1A8A7FE96A464C889D1825DE45EA9657</path>
                    </parameterMap>
                     <parameterMap>

            <path>rdl:R19192462550/tpl:R5E442F596F5C45CC8BECBEA04EC8D889/tpl:RA9880F55D688411695965B5215657423/rdl:R21605619301/tpl:RBFF16CD8F3BB4BFE8539B683D6782F0B/tpl:R1A8A7FE96A464C889D1825DE45EA9657</path>
                    </parameterMap>
                </parameterMaps>
            </tipRequest>";


                client.PostData = str;

                client.Method = HttpVerb.POST;

                var json = client.MakeRequest();
            }
            catch (Exception ex)
            {
                ExceptionHandler.Handle(ex);
            }
        }
    }
}
