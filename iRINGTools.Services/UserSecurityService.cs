using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using System.ServiceModel.Activation;
using org.iringtools.library;
using log4net;
using org.iringtools.UserSecurity;
using System.Configuration;
using System.ServiceModel.Web;
using System.ComponentModel;

namespace org.iringtools.services
{
    [ServiceContract(Namespace = "http://www.iringtools.org/service")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class UserSecurityService
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(UserSecurityService));
        private UserSecurityProvider _userSecurityProvider = null;
        private CustomError _CustomError = null;

        public UserSecurityService()
        {
            _userSecurityProvider = new UserSecurityProvider(ConfigurationManager.AppSettings);
        }

        [Description("Get all users from the database.")]
        [WebGet(UriTemplate = "/users?format={format}")]
        public void GetAllUsers(string format)
        {
            try
            {
                OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
                context.ContentType = "application/xml";

                Users users = _userSecurityProvider.GetAllUsers();
                _userSecurityProvider.FormatOutgoingMessage<Users>(users, format, true);
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
        }

        [Description("Get all sites from the database.")]
        [WebGet(UriTemplate = "/sites")]
        public void GetSites()
        {
            try
            {
                string format = "Json";
                OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
                context.ContentType = "application/xml";

                Sites sites = _userSecurityProvider.GetSites();
                _userSecurityProvider.FormatOutgoingMessage<Sites>(sites, format, true);
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
        }

        [Description("Get site by site ID from the database.")]
        [WebGet(UriTemplate = "/sites/{iSiteId}")]
        public void GetSite(int iSiteId)
        {
            try
            {
                string format = "Json";
                OutgoingWebResponseContext context = WebOperationContext.Current.OutgoingResponse;
                context.ContentType = "application/xml";

                Site site = _userSecurityProvider.GetSite(iSiteId);
                _userSecurityProvider.FormatOutgoingMessage<Site>(site, format, true);
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
        }

    }
}