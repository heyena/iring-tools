using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.library;
using System.Net;
using System.IO;
using org.iringtools.utility;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace FileDownloader
{
    class Program
    {
        [STAThread] 
        static void Main(string[] args)
        {
            string baseUrl = String.Empty;
            string relativeUrl = String.Empty;
            string filePath = String.Empty;

            try
            {
                if (args.Length >= 3)
                {
                    baseUrl = args[0];            //http:/localhost:54321/adapter
                    relativeUrl = args[1];        //"Scope/app/resourcebytes"
                    filePath = args[2];           //Local file path where you want to download the file.

                    string uri = baseUrl + relativeUrl;

                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                    request.Method = "GET";

                    // request.MediaType = "application/x-msaccess";               
                    StreamReader reader = new System.IO.StreamReader(request.GetResponse().GetResponseStream());

                    var bytes = default(byte[]);
                    using (var memstream = new MemoryStream())
                    {
                        reader.BaseStream.CopyTo(memstream);
                        bytes = memstream.ToArray();
                    }

                    System.IO.File.WriteAllBytes(filePath, bytes);
                    reader.Close();
                }
            }
            catch(Exception e)
            {
             
            }
        }
    }
}
