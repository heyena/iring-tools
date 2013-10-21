using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TipMapGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new TipRestClient();
            client.EndPoint = @"http://localhost:54322/data/def/12345_000/lines"; ;
            client.Method = HttpVerb.GET;
            //client.PostData = "{postData: value}";
            //client.Method = HttpVerb.POST;
            //client.PostData = "{postData: value}";
        //http://localhost:54322/data/def/12345_000/lines?format=jsonld&start=2
            //var json = client.MakeRequest();
            var json = client.MakeRequest("?format=jsonld&start=2");

            Console.Write(json.ToString());
            Console.Read();
         

        }
    }
}
