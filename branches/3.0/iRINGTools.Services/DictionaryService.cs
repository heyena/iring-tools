using System;
using System.ServiceModel;
using System.ServiceModel.Activation;
using log4net;
using org.iringtools.adapter;
using org.iringtools.library;
using System.Configuration;
using System.ComponentModel;
using System.ServiceModel.Web;
using System.Xml.Linq;
using System.IO;
using System.Net;

namespace org.iringtools.services
{
    [ServiceContract(Namespace = "http://www.iringtools.org/service")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class DictionaryService
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(DictionaryService));
        private DictionaryProvider _dictionaryProvider = null;
        private CustomError _CustomError = null;

        public DictionaryService()
        {
            _dictionaryProvider = new DictionaryProvider(ConfigurationManager.AppSettings);
        }


        [Description("Getting DatabaseDictionary from database by application id")]
        [WebGet(UriTemplate = "/GetDictionary?applicationId={applicationId}&format={format}")]
        public void GetDictionary(Guid applicationId, string format)
        {
            try
            {
                if (string.IsNullOrEmpty(format))
                { format = "xml"; }

                org.iringtools.library.DatabaseDictionary dictionary = _dictionaryProvider.GetDictionary(applicationId);
                _dictionaryProvider.FormatOutgoingMessage<org.iringtools.library.DatabaseDictionary>(dictionary, format, true);
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
        }

        [Description("Insert Dictionary to the data base.")]
        [WebInvoke(Method = "POST", UriTemplate = "/InsertDictionary?format={format}")]
        public void InsertDictionary(string format, Stream stream)
        {
            if (string.IsNullOrEmpty(format))
            { format = "xml"; }

            Response response = new Response();
            try
            {
                format = MapContentType(format);
                if (format == "raw")
                {
                    throw new Exception("");
                }
                else
                {
                    XElement xElement = _dictionaryProvider.FormatIncomingMessage<org.iringtools.library.DatabaseDictionary>(stream, format);
                    response = _dictionaryProvider.InsertDictionary(new XDocument(xElement));
                }
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
            PrepareResponse(ref response);
            _dictionaryProvider.FormatOutgoingMessage<Response>(response, format, true);
        }


        #region Private Methods
        private string MapContentType(string format)
        {
            IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
            string contentType = request.ContentType;

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

            if (contentType != null)
            {
                if (contentType.ToLower().Contains("application/xml"))
                {
                    format = "xml";
                }
                else if (contentType.ToLower().Contains("json"))
                {
                    format = "json";
                }
                else
                {
                    format = "raw";
                }
            }

            return format;
        }

        private void PrepareResponse(ref Response response)
        {
            if (response.Level == StatusLevel.Success)
            {
                response.StatusCode = HttpStatusCode.OK;
            }
            else
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
            }

            if (response.Messages != null)
            {
                foreach (string msg in response.Messages)
                {
                    response.StatusText += msg;
                }
            }

            foreach (Status status in response.StatusList)
            {
                foreach (string msg in status.Messages)
                {
                    response.StatusText += msg;
                }
            }
        }

        #endregion

    }
}