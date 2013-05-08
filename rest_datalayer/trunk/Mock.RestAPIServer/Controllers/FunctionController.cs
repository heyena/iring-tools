using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Mock.RestAPIServer.Models;

namespace Mock.RestAPIServer.Controllers
{
    public class FunctionController : ApiController
    {
        // GET api/function
        public GenericObject<Function> Get()
        {
            var items = Utility.GetFunctions();
            return new GenericObject<Function>() { Items = items, limit = items.Count, total = items.Count };
            
        }

        public GenericObject<Function> Get(int id)
        {
            return this.Get(id,null,0,0);

        }

        // GET api/project/5
        public GenericObject<Function> Get(int id,string name,int limit,int start)
        {
            var items = Utility.GetFunctions().Where<Function>(x => x.Id == id);

            if (name != null)
                items = items.Where<Function>(x => x.Name == name);

            if (limit !=0 )
                items = items.Skip(start).Take(limit);

            var itemList = items.ToList();

            return new GenericObject<Function>() { Items = itemList, limit = itemList.Count, total = itemList.Count };

        }

        // POST api/function
        public GenericObject<Function> Post(Function value)
        {
            return this.Get(1, null, 0, 0);
        }

        // PUT api/function/5
        public GenericObject<Function> Put(Function value)
        {
            return this.Get(1, null, 0, 0);
        }

        // DELETE api/function/5
        public void Delete(int id)
        {
        }
    }
}
