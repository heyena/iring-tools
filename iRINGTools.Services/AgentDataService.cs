using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using System.ServiceModel.Activation;
using log4net;
using org.iringtools.library;
using System.ServiceModel.Web;
using System.ComponentModel;
using org.iringtools.AgentLibrary;
using System.Configuration;
using System.IO;
using System.Xml.Linq;
using System.Net;

namespace org.iringtools.services
{
    [ServiceContract(Namespace = "http://www.iringtools.org/service")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class AgentDataService
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(UserSecurityService));
        private AgentProvider _agentProvider = null;
        private CustomError _CustomError = null;

        public AgentDataService()
        {
            _agentProvider = new AgentProvider(ConfigurationManager.AppSettings);
        }

        [Description("Gets all jobs from the data base.")]
        [WebGet(UriTemplate = "/alljobs?format={format}")]
        public void GetAllJobs(string format) 
        {
            try
            {
                if (string.IsNullOrEmpty(format))
                { format = "xml"; }


                org.iringtools.AgentLibrary.Agent.Jobs jobs = _agentProvider.GetAllJobs();
                _agentProvider.FormatOutgoingMessage<org.iringtools.AgentLibrary.Agent.Jobs>(jobs, format, true);
            }

            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetScope, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
        }

        [Description("Gets job for a given scope and app from the data base.")]
        [WebGet(UriTemplate = "/job?scope={scope}&app={app}&format={format}")]
        public void GetJob(string scope, string app, string format)
        {
            try
            {
                if (string.IsNullOrEmpty(format))
                { format = "xml"; }


                org.iringtools.AgentLibrary.Agent.Job job = _agentProvider.GetJob(scope, app);
                _agentProvider.FormatOutgoingMessage<org.iringtools.AgentLibrary.Agent.Job>(job, format, true);
            }

            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetScope, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
        }

        [Description("Insert job to the data base.")]
        [WebInvoke(Method = "POST", UriTemplate = "/insertJob?format={format}")]
        public void InsertJob(string format, Stream stream)
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
                    XElement xElement = _agentProvider.FormatIncomingMessage<org.iringtools.AgentLibrary.Agent.Jobs>(stream, format);
                    response = _agentProvider.InsertJob(new XDocument(xElement));
                }
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
            PrepareResponse(ref response);
            _agentProvider.FormatOutgoingMessage<Response>(response, format, true);
        }

        [Description("Update jobs to the data base.")]
        [WebInvoke(Method = "PUT", UriTemplate = "/updateJob/{jobId}?format={format}")]
        public void UpdateJob(string jobId, string format, Stream stream) // Completed.
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
                    XElement xElement = _agentProvider.FormatIncomingMessage<org.iringtools.AgentLibrary.Agent.Jobs>(stream, format);
                    response = _agentProvider.UpdateJob(jobId, new XDocument(xElement));
                }
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
            PrepareResponse(ref response);
            _agentProvider.FormatOutgoingMessage<Response>(response, format, true);
        }

        [Description("Delete job from the data base.")]
        [WebInvoke(Method = "DELETE", UriTemplate = "/deleteJob/{jobId}?format={format}")]
        public void DeleteJob(string jobId, string format) // Completed.
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
                    response = _agentProvider.DeleteJob(jobId);
                }
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
            PrepareResponse(ref response);
            _agentProvider.FormatOutgoingMessage<Response>(response, format, true);
        }

        [Description("Gets job client info from the data base.")]
        [WebGet(UriTemplate = "/jobclientinfo?jobid={jobId}&format={format}")]
        public void GetJoblientInfo(string jobId, string format)
        {
            try
            {
                if (string.IsNullOrEmpty(format))
                { format = "xml"; }


                org.iringtools.AgentLibrary.Agent.JobClientInfos jobClientInfo = _agentProvider.GetJobClientInfo(jobId);
                _agentProvider.FormatOutgoingMessage<org.iringtools.AgentLibrary.Agent.JobClientInfos>(jobClientInfo, format, true);
            }

            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetScope, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
        }

        [Description("Insert job client info to the data base.")]
        [WebInvoke(Method = "POST", UriTemplate = "/insertJobClientInfo?format={format}")]
        public void InsertJobClientInfo(string format, Stream stream)
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
                    XElement xElement = _agentProvider.FormatIncomingMessage<org.iringtools.AgentLibrary.Agent.JobClientInfos>(stream, format);
                    response = _agentProvider.InsertJobClientInfo(new XDocument(xElement));
                }
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
            PrepareResponse(ref response);
            _agentProvider.FormatOutgoingMessage<Response>(response, format, true);
        }

        [Description("Update job client info in the data base.")]
        [WebInvoke(Method = "PUT", UriTemplate = "/updateJobClientInfo/{jobId}?format={format}")]
        public void UpdateJobClientInfo(string jobId, string format, Stream stream) 
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
                    XElement xElement = _agentProvider.FormatIncomingMessage<org.iringtools.AgentLibrary.Agent.JobClientInfos>(stream, format);
                    response = _agentProvider.UpdateJobClientInfo(jobId, new XDocument(xElement));
                }
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
            PrepareResponse(ref response);
            _agentProvider.FormatOutgoingMessage<Response>(response, format, true);
        }

        [Description("Delete job client info from the data base.")]
        [WebInvoke(Method = "DELETE", UriTemplate = "/deleteJobClientInfo/{jobId}?format={format}")]
        public void DeleteJobClientInfo(string jobId, string format) // Completed.
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
                    response = _agentProvider.DeleteJobClientInfo(jobId);
                }
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
            PrepareResponse(ref response);
            _agentProvider.FormatOutgoingMessage<Response>(response, format, true);
        }

        [Description("Gets schedule from the data base.")]
        [WebGet(UriTemplate = "/schedule?scheduleId={scheduleId}&format={format}")]
        public void GetSchedule(string scheduleId, string format)
        {
            try
            {
                if (string.IsNullOrEmpty(format))
                { format = "xml"; }


                org.iringtools.AgentLibrary.Agent.Schedule schedule = _agentProvider.GetSchedule(scheduleId);
                _agentProvider.FormatOutgoingMessage<org.iringtools.AgentLibrary.Agent.Schedule>(schedule, format, true);
            }

            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetScope, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
        }

        [Description("Insert schedule in the data base.")]
        [WebInvoke(Method = "POST", UriTemplate = "/insertSchedule?format={format}")]
        public void InsertSchedule(string format, Stream stream)
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
                    XElement xElement = _agentProvider.FormatIncomingMessage<org.iringtools.AgentLibrary.Agent.Schedules>(stream, format);
                    response = _agentProvider.InsertSchedule(new XDocument(xElement));
                }
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
            PrepareResponse(ref response);
            _agentProvider.FormatOutgoingMessage<Response>(response, format, true);
        }

        [Description("Update schedule in the data base.")]
        [WebInvoke(Method = "PUT", UriTemplate = "/updateSchedule/{scheduleId}?format={format}")]
        public void UpdateSchedule(string scheduleId, string format, Stream stream)
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
                    XElement xElement = _agentProvider.FormatIncomingMessage<org.iringtools.AgentLibrary.Agent.Schedules>(stream, format);
                    response = _agentProvider.UpdateSchedule(scheduleId, new XDocument(xElement));
                }
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
            PrepareResponse(ref response);
            _agentProvider.FormatOutgoingMessage<Response>(response, format, true);
        }

        [Description("Delete schedule from the data base.")]
        [WebInvoke(Method = "DELETE", UriTemplate = "/deleteSchedule/{scheduleId}?format={format}")]
        public void DeleteSchedule(string scheduleId, string format) 
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
                    response = _agentProvider.DeleteSchedule(scheduleId);
                }
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
            PrepareResponse(ref response);
            _agentProvider.FormatOutgoingMessage<Response>(response, format, true);
        }

        [Description("Gets jobschedule from the data base.")]
        [WebGet(UriTemplate = "/jobschedule?jobId={jobId}&scheduleId={scheduleId}&format={format}")]
        public void GetJobSchedule(string jobId, string scheduleId, string format)
        {
            try
            {
                if (string.IsNullOrEmpty(format))
                { format = "xml"; }


                org.iringtools.AgentLibrary.Agent.JobSchedule jobschedule = _agentProvider.GetJobSchedule(jobId, scheduleId);
                _agentProvider.FormatOutgoingMessage<org.iringtools.AgentLibrary.Agent.JobSchedule>(jobschedule, format, true);
            }

            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetScope, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
        }

        [Description("Insert jobschedule in the data base.")]
        [WebInvoke(Method = "POST", UriTemplate = "/insertJobSchedule?format={format}")]
        public void InsertJobSchedule(string format, Stream stream)
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
                    XElement xElement = _agentProvider.FormatIncomingMessage<org.iringtools.AgentLibrary.Agent.JobSchedules>(stream, format);
                    response = _agentProvider.InsertJobSchedule(new XDocument(xElement));
                }
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
            PrepareResponse(ref response);
            _agentProvider.FormatOutgoingMessage<Response>(response, format, true);
        }

        [Description("Update jobschedule in the data base.")]
        [WebInvoke(Method = "PUT", UriTemplate = "/updateJobSchedule?jobId={jobId}&scheduleId={scheduleId}&format={format}")]
        public void UpdateJobSchedule(string jobId, string scheduleId, string format, Stream stream)
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
                    XElement xElement = _agentProvider.FormatIncomingMessage<org.iringtools.AgentLibrary.Agent.JobSchedules>(stream, format);
                    response = _agentProvider.UpdateJobSchedule(jobId, scheduleId, new XDocument(xElement));
                }
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
            PrepareResponse(ref response);
            _agentProvider.FormatOutgoingMessage<Response>(response, format, true);
        }

        [Description("Delete jobschedule from the data base.")]
        [WebInvoke(Method = "DELETE", UriTemplate = "/deleteJobSchedule?jobId={jobId}&scheduleId={scheduleId}&format={format}")]
        public void DeleteJobSchedule(string jobId, string scheduleId, string format)
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
                    response = _agentProvider.DeleteJobSchedule(jobId, scheduleId);
                }
            }
            catch (Exception ex)
            {
                CustomErrorLog objCustomErrorLog = new CustomErrorLog();
                _CustomError = objCustomErrorLog.customErrorLogger(ErrorMessages.errGetUISettings, ex, _logger);
                objCustomErrorLog.throwJsonResponse(_CustomError);
            }
            PrepareResponse(ref response);
            _agentProvider.FormatOutgoingMessage<Response>(response, format, true);
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