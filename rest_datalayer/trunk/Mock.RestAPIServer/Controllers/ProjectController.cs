using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Mock.RestAPIServer.Models;

namespace Mock.RestAPIServer.Controllers
{
    public class ProjectController : ApiController
    {
        // GET api/project
        public GenericObject<Project> Get()
        {
            var items = Utility.GetProjects();
            return new GenericObject<Project>() { Items = items, limit = items.Count, total = items.Count };
        }

        // GET api/project/5
        public GenericObject<Project> Get(int id)
        {
            var items = Utility.GetProjects().Where<Project>(x => x.Id == id).ToList();
            return new GenericObject<Project>() { Items = items, limit = items.Count, total = items.Count };
        }

        // POST api/project
        public void Post(Project value)
        {
        }

        // PUT api/project/5
        public void Put(Project value)
        {
        }

        // DELETE api/project/5
        public void Delete(int id)
        {
        }
    }
}
