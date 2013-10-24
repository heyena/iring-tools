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
                        <path>rdl:Rd9c631e5-543f-4b98-8684-901e710f953f/tpl:R53360319163(0)/tpl:RF8B2CB1FF4F34B3D9D2FCFB3FC025BB5/rdl:R85074893353/tpl:RD7841CFC6A15488CBAA45414A54AB8C1(0)/tpl:R2EA408134E3C4A22A408AF1648A75317/rdl:R22683180655/tpl:R94082855849/tpl:R1427286232D34EE797D125795B0854A5</path>
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
