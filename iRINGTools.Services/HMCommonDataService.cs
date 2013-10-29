using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using org.iringtools.adapter;
using System.Configuration;
using System.Xml.Linq;
using System.Collections.Specialized;
using org.iringtools.library;

namespace org.iringtools.services
{
    public class HMCommonDataService : CommonDataService
    {
        private AbstractProvider _abstractPrivder = null;

        public HMCommonDataService()
        {
            _abstractPrivder = new AbstractProvider(ConfigurationManager.AppSettings);

        }

        public void GenerateTIP(string project, string app, string resource)
        {
            XDocument xDocument = _abstractPrivder.GenerateTIP(project, app, resource);
        }

        public void GetVersion(string format)
        {
            format = MapContentType(null, null, format);

            VersionInfo version = _abstractPrivder.GetVersion();

            _abstractPrivder.FormatOutgoingMessage<VersionInfo>(version, format, true);
        }

        new public void GetContexts(string app, string format)
        {
            format = MapContentType(null, null, format);

            Contexts contexts = _abstractPrivder.GetContexts(app);

            _abstractPrivder.FormatOutgoingMessage<Contexts>(contexts, format, true);
        }

        new public void GetList(string project, string app, string resource, string format, int start, int limit, string sortOrder, string sortBy, string indexStyle)
        {
            
            try
            {
                // get list of contents is not allowed in this service
                if (string.IsNullOrEmpty(format) || !(format.ToLower() == "dto" || format.ToLower() == "rdf" ||
                  format.ToLower().Contains("xml") || format.ToLower().Contains("json") || format.ToLower().Contains("jsonld")))
                {
                    format = "jsonld";
                }

                NameValueCollection parameters = new NameValueCollection();
                parameters.Add("format", format);

                bool fullIndex = false;
                if (indexStyle != null && indexStyle.ToUpper() == "FULL")
                    fullIndex = true;

                XDocument xDocument = _abstractPrivder.GetList(project, app, resource, ref format, start, limit, sortOrder, sortBy, fullIndex, parameters);
                _abstractPrivder.FormatOutgoingMessage(xDocument.Root, format);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string MapContentType(string project, string app, string format)
        {

            // if it's a known format then return it
            if (format != null && (format.ToLower() == "raw" || format.ToLower() == "dto" || format.ToLower() == "rdf" ||
              format.ToLower().Contains("xml") || format.ToLower().Contains("json")))
            {
                return format;
            }

            if (string.IsNullOrEmpty(format))
            {
                format = "json";
            }

            return format;
        }
    }
}