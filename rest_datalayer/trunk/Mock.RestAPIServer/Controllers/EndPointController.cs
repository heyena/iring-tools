using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MK = Mock.RestAPIServer.Models;

namespace Mock.RestAPIServer.Controllers
{
    public class EndPointController : ApiController
    {
        // GET api/EndPoint
        public MK.EndPoint Get()
        {
            return new MK.EndPoint();
        }
      
    }
}
