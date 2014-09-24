using System;
using System.ServiceModel;
using System.ServiceModel.Activation;
using log4net;
using org.iringtools.adapter;
using org.iringtools.library;
using System.Configuration;
using System.ComponentModel;
using System.ServiceModel.Web;

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


        [Description("Gets all applications available from the data base.")]
        [WebGet(UriTemplate = "/getDBDictionary/{applicatyionId}?format={format}")]
        public void GetDBDictionary(string applicatyionId, string format)
        {
            try
            {
                if (string.IsNullOrEmpty(format))
                { format = "xml"; }

                DatabaseDictionary applications = _dictionaryProvider.GetDBDictionary(applicatyionId);
                _dictionaryProvider.FormatOutgoingMessage<DatabaseDictionary>(applications, format, true);
            }

            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetScope, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
        }

    }
}