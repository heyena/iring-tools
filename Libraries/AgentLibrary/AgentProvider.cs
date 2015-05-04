using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.adapter;
using log4net;
using System.Collections.Specialized;
using Ninject;
using System.Runtime.Serialization;
using System.IO;
using org.iringtools.library;
using System.Xml.Linq;
using org.iringtools.utility;
using System.ServiceModel.Web;
using System.Data.Linq;
using System.Web;
using System.Data;
using org.iringtools.mapping;
using org.iringtools.AgentLibrary;

namespace org.iringtools.AgentLibrary
{
    public class AgentProvider : BaseProvider
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AdapterProvider));
        private string _connSecurityDb;
        
        [Inject]
        public AgentProvider(NameValueCollection settings)
            : base(settings)
        {
            try
            {
                // We have _settings collection available here.
                _connSecurityDb = settings["SecurityConnection"];
              
            }
            catch (Exception e)
            {
                _logger.Error("Error initializing adapter provider: " + e.Message);
            }
        }

        public org.iringtools.AgentLibrary.Agent.Jobs GetAllJobs(string userName, int siteId, int platformId, int isExchange)
        {
            org.iringtools.AgentLibrary.Agent.Jobs jobs = new org.iringtools.AgentLibrary.Agent.Jobs();
            try
            {
                NameValueList nvl = new NameValueList();
                nvl.Add(new ListItem() { Name = "@UserName", Value = Convert.ToString(userName)});
                nvl.Add(new ListItem() { Name = "@SiteId", Value = Convert.ToString(siteId)});
                nvl.Add(new ListItem() { Name = "@PlatformId", Value = Convert.ToString(platformId)});
                nvl.Add(new ListItem() { Name = "@IsExchange", Value = Convert.ToString(isExchange)});
               
                string xmlString = DBManager.Instance.ExecuteXmlQuery(_connSecurityDb, "spgAllJobs", nvl);
                xmlString = HttpUtility.HtmlDecode(xmlString);
                jobs = utility.Utility.Deserialize<org.iringtools.AgentLibrary.Agent.Jobs>(xmlString, true);
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting  Jobs: " + ex);
            }
            return jobs;
        }

        public org.iringtools.AgentLibrary.Agent.Job GetJob(Guid jobId)
        {
           
            org.iringtools.AgentLibrary.Agent.Job job = new org.iringtools.AgentLibrary.Agent.Job();
            try
            {
                NameValueList nvl = new NameValueList();
                nvl.Add(new ListItem() { Name = "@JobId", Value = Convert.ToString(jobId) });
             
                string xmlString = DBManager.Instance.ExecuteXmlQuery(_connSecurityDb, "spgJob", nvl);
                xmlString = HttpUtility.HtmlDecode(xmlString);
                job = utility.Utility.Deserialize<org.iringtools.AgentLibrary.Agent.Job>(xmlString, true);
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting  Job: " + ex);
            }
            return job;
        }

        public Response InsertJob(XDocument xml)
        {
            Response response = new Response();
            response.Messages = new Messages();
            string cachePageSize = "";

            try
            {               
                org.iringtools.AgentLibrary.Agent.Job job = Utility.DeserializeDataContract<org.iringtools.AgentLibrary.Agent.Job>(xml.ToString());

                string schedulesXml = job.schedules.ToXElement().ToString().Replace("xmlns=", "xmlns1=");//this is done, because in stored procedure it causes problem

                using (var dc = new DataContext(_connSecurityDb))
                {
                    if (job == null)
                        PrepareErrorResponse(response, "Please enter valid Job!");
                    else
                    {
                        if (string.IsNullOrEmpty(_settings["CachePageSize"]))
                            cachePageSize = "1000";
                        else
                            cachePageSize = _settings["CachePageSize"];

                        NameValueList nvl = new NameValueList();
             
                        if (job.DataObjectId != null)
                           nvl.Add(new ListItem() { Name = "@DataObjectIds", Value = Convert.ToString(job.DataObjectId) });
                        else
                            nvl.Add(new ListItem() { Name = "@DataObjectIds", Value = DBNull.Value });

                        nvl.Add(new ListItem() { Name = "@Is_exchange", Value = Convert.ToString(job.Is_Exchange) });
                        if (job.Xid != null)
                            nvl.Add(new ListItem() { Name = "@Xid", Value = job.Xid });
                        else
                            nvl.Add(new ListItem() { Name = "@Xid", Value = DBNull.Value });
                        nvl.Add(new ListItem() { Name = "@Cache_Page_Size", Value = cachePageSize});
                        nvl.Add(new ListItem() { Name = "@PlatformId", Value = Convert.ToString(job.PlatformId) });
                        nvl.Add(new ListItem() { Name = "@SiteId", Value = Convert.ToString(job.SiteId) });
                        nvl.Add(new ListItem() { Name = "@Active", Value = Convert.ToString(job.Active) });
                        nvl.Add(new ListItem() { Name = "@Schedules", Value = schedulesXml });

                        string output = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spiJob", nvl);

                        switch (output)
                        {
                            case "1":
                                PrepareSuccessResponse(response, "jobadded");
                                break;
                            case "0":
                                PrepareSuccessResponse(response, "duplicatejob");
                                break;
                            default:
                                PrepareErrorResponse(response, output);
                                break;
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                _logger.Error("Error adding Job: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public Response UpdateJob(string jobId, XDocument xml)
        {
            Response response = new Response();
            response.Messages = new Messages();
            string cachePageSize = "";
            try
            {
                org.iringtools.AgentLibrary.Agent.Job job = Utility.DeserializeDataContract<org.iringtools.AgentLibrary.Agent.Job>(xml.ToString());
                string schedulesXml = job.schedules.ToXElement().ToString().Replace("xmlns=", "xmlns1=");//this is done, because in stored procedure it causes problem
               
                using (var dc = new DataContext(_connSecurityDb))
                {
                    if (job == null)
                        PrepareErrorResponse(response, "Please enter Job!");
                    else
                    {
                        if (string.IsNullOrEmpty(_settings["CachePageSize"]))
                            cachePageSize = "1000";
                        else
                            cachePageSize = _settings["CachePageSize"];

                        NameValueList nvl = new NameValueList();
                        nvl.Add(new ListItem() { Name = "@JobId", Value = jobId });
                       
                        if (job.DataObjectId != null)
                            nvl.Add(new ListItem() { Name = "@DataObjectId", Value = job.DataObjectId });
                        else
                            nvl.Add(new ListItem() { Name = "@DataObjectId", Value = DBNull.Value });

                        nvl.Add(new ListItem() { Name = "@Is_exchange", Value = Convert.ToString(job.Is_Exchange) });

                        if (job.Xid != null)
                            nvl.Add(new ListItem() { Name = "@Xid", Value = job.Xid });
                        else
                            nvl.Add(new ListItem() { Name = "@Xid", Value = DBNull.Value });

                        nvl.Add(new ListItem() { Name = "@Cache_Page_Size", Value = cachePageSize });
                        nvl.Add(new ListItem() { Name = "@PlatformId", Value = Convert.ToString(job.PlatformId) });
                        nvl.Add(new ListItem() { Name = "@SiteId", Value = Convert.ToString(job.SiteId) });
                        nvl.Add(new ListItem() { Name = "@Active", Value = Convert.ToString(job.Active) });
                        nvl.Add(new ListItem() { Name = "@Schedules", Value = schedulesXml });
                        string output = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spuJob", nvl);

                        switch (output)
                        {
                            case "1":
                                PrepareSuccessResponse(response, "jobupdated");
                                break;
                            default:
                                PrepareErrorResponse(response, output);
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error updating Job: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public Response UpdateJobRecords(string jobId, XDocument xml)
        {
            Response response = new Response();
            response.Messages = new Messages();
            try
            {
                org.iringtools.AgentLibrary.Agent.Job job = Utility.DeserializeDataContract<org.iringtools.AgentLibrary.Agent.Job>(xml.ToString());
                //string schedulesXml = job.schedules.ToXElement().ToString().Replace("xmlns=", "xmlns1=");//this is done, because in stored procedure it causes problem

                using (var dc = new DataContext(_connSecurityDb))
                {
                    if (job == null)
                        PrepareErrorResponse(response, "Please enter Job!");
                    else
                    {
                      
                        NameValueList nvl = new NameValueList();
                        nvl.Add(new ListItem() { Name = "@JobId", Value = jobId });

                        if (job.TotalRecords > 0)
                            nvl.Add(new ListItem() { Name = "@TotalRecords", Value = job.TotalRecords });
                        else
                            nvl.Add(new ListItem() { Name = "@TotalRecords", Value = DBNull.Value });

                        if (job.CachedRecords > 0)
                            nvl.Add(new ListItem() { Name = "@CachedRecords", Value = job.CachedRecords });
                        else
                            nvl.Add(new ListItem() { Name = "@CachedRecords", Value = DBNull.Value });

                        nvl.Add(new ListItem() { Name = "@Status", Value = DBNull.Value });

                        string Status = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spuJobRecords", nvl);

                        switch (Status)
                        {
                            case "Ready":
                                PrepareSuccessResponse(response, "jobrecordsupdated");
                                break;
                            case "Busy":
                                PrepareSuccessResponse(response, "jobrecordsupdated");
                                break;
                            case "Terminate":
                                PrepareSuccessResponse(response, "jobrecordsupdated");
                                break;
                            default:
                                PrepareErrorResponse(response, Status);
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error updating JobRecords: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public Response DeleteJob(string jobId)
        {
            Response response = new Response();
            response.Messages = new Messages();

        try
            {
                using (var dc = new DataContext(_connSecurityDb))
                {
                    if (string.IsNullOrEmpty(jobId))
                        PrepareErrorResponse(response, "Please enter jobid!");
                    else
                    {
                        NameValueList nvl = new NameValueList();
                        nvl.Add(new ListItem() { Name = "@JobId", Value = jobId });

                        string output = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spdJob", nvl);

                        switch (output)
                        {
                            case "1":
                                PrepareSuccessResponse(response, "jobdeleted");
                                break;
                            default:
                                PrepareErrorResponse(response, output);
                                break;
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                _logger.Error("Error deleting Job: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

       
        public org.iringtools.AgentLibrary.Agent.Schedule GetSchedule(string scheduleId)
        {
            List<org.iringtools.AgentLibrary.Agent.Schedule> lstSchedule = new List<org.iringtools.AgentLibrary.Agent.Schedule>();

            using (var dc = new DataContext(_connSecurityDb))
            {
                lstSchedule = dc.ExecuteQuery<org.iringtools.AgentLibrary.Agent.Schedule>("spgSchedule @ScheduleId = {0}", scheduleId).ToList();
            }

            org.iringtools.AgentLibrary.Agent.Schedule schedule = new org.iringtools.AgentLibrary.Agent.Schedule();
            if (lstSchedule.Count > 0)
                schedule = lstSchedule.First();

            return schedule;
        }

        public Response InsertSchedule(XDocument xml)
        {
            Response response = new Response();

            try
            {
                org.iringtools.AgentLibrary.Agent.Schedule schedule = Utility.DeserializeDataContract<org.iringtools.AgentLibrary.Agent.Schedule>(xml.ToString());

                using (var dc = new DataContext(_connSecurityDb))
                {
                    NameValueList nvl = new NameValueList();
                    //nvl.Add(new ListItem() { Name = "@Schedule_Id", Value = Convert.ToString(schedule.Schedule_Id) });
                    nvl.Add(new ListItem() { Name = "@Created_DateTime", Value = schedule.Created_DateTime});
                    nvl.Add(new ListItem() { Name = "@Created_By", Value = schedule.Created_By });
                    nvl.Add(new ListItem() { Name = "@Occurance", Value = schedule.Occurance });
                    nvl.Add(new ListItem() { Name = "@Weekday", Value = schedule.Weekday });
                    nvl.Add(new ListItem() { Name = "@Start_DateTime", Value = schedule.Start_DateTime});
                    nvl.Add(new ListItem() { Name = "@End_DateTime", Value = schedule.End_DateTime});
                    nvl.Add(new ListItem() { Name = "@Status", Value = schedule.Status });
                 
                    string output = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spiSchedule", nvl);
                    switch (output)
                    {
                        case "1":
                            PrepareSuccessResponse(response, "scheduleAdded");
                            break;
                        case "0":
                            PrepareSuccessResponse(response, "duplicateSchedule");
                            break;
                        default:
                            PrepareErrorResponse(response, output);
                            break;
                    }

                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error adding Schedule: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public Response UpdateSchedule(string scheduleId, XDocument xml)
        {
            Response response = new Response();
            response.Messages = new Messages();
            try
            {
                string rawXml = xml.ToString().Replace("xmlns=", "xmlns1=");//this is done, because in stored procedure it causes problem
                org.iringtools.AgentLibrary.Agent.Schedule schedule = Utility.DeserializeDataContract<org.iringtools.AgentLibrary.Agent.Schedule>(xml.ToString());

                using (var dc = new DataContext(_connSecurityDb))
                {
                    if (schedule == null)
                        PrepareErrorResponse(response, "Please enter schedule!");
                    else
                    {
                        NameValueList nvl = new NameValueList();
                        nvl.Add(new ListItem() { Name = "@Created_DateTime", Value = scheduleId });
                        nvl.Add(new ListItem() { Name = "@Created_By", Value = schedule.Created_By });
                        nvl.Add(new ListItem() { Name = "@Occurance", Value = schedule.Occurance });
                        nvl.Add(new ListItem() { Name = "@Weekday", Value = schedule.Weekday });
                        nvl.Add(new ListItem() { Name = "@Start_DateTime", Value = schedule.Start_DateTime});
                        nvl.Add(new ListItem() { Name = "@End_DateTime", Value = schedule.End_DateTime});
                        nvl.Add(new ListItem() { Name = "@Status", Value = schedule.Status });

                        string output = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spuSchedule", nvl);

                        switch (output)
                        {
                            case "1":
                                PrepareSuccessResponse(response, "scheduleupdated");
                                break;
                            default:
                                PrepareErrorResponse(response, output);
                                break;
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                _logger.Error("Error updating Schedule: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }

        public Response DeleteSchedule(string scheduleId)
        {
            Response response = new Response();
            response.Messages = new Messages();

            try
            {
                using (var dc = new DataContext(_connSecurityDb))
                {
                    if (string.IsNullOrEmpty(scheduleId))
                        PrepareErrorResponse(response, "Please enter scheduleId!");
                    else
                    {
                        NameValueList nvl = new NameValueList();
                        nvl.Add(new ListItem() { Name = "@ScheduleId", Value = scheduleId });

                        string output = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spdSchedule", nvl);

                        switch (output)
                        {
                            case "1":
                                PrepareSuccessResponse(response, "scheduledeleted");
                                break;
                            default:
                                PrepareErrorResponse(response, output);
                                break;
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                _logger.Error("Error deleting Schedule: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
        }


        public XElement FormatIncomingMessage<T>(Stream stream, string format, bool isBase64encoded = false)
        {
            XElement xElement = null;

            if (format != null && (format.ToLower().Contains("xml") || format.ToLower().Contains("rdf") ||
              format.ToLower().Contains("dto")))
            {
                xElement = XElement.Load(stream);
            }
            else
            {
                T dataItems = FormatIncomingMessage<T>(stream);

                if (isBase64encoded)
                    xElement = dataItems.Serialize<T>();
                else
                    xElement = dataItems.ToXElement<T>();
            }

            return xElement;
        }

        public T FormatIncomingMessage<T>(Stream stream)
        {
            T dataItems;

            DataItemSerializer serializer = new DataItemSerializer(
                _settings["JsonIdField"], _settings["JsonLinksField"], bool.Parse(_settings["DisplayLinks"]));
            string json = Utility.ReadString(stream);
            dataItems = serializer.Deserialize<T>(json, true);
            stream.Close();

            return dataItems;
        }

        public void FormatOutgoingMessage<T>(T graph, string format, bool useDataContractSerializer)
        {
            if (format.ToUpper() == "JSON")
            {
                string json = Utility.SerializeJson<T>(graph, useDataContractSerializer);

                HttpContext.Current.Response.ContentType = "application/json; charset=utf-8";
                HttpContext.Current.Response.Write(json);
            }
            else
            {
                string xml = Utility.Serialize<T>(graph, useDataContractSerializer);

                HttpContext.Current.Response.ContentType = "application/xml";
                HttpContext.Current.Response.Write(xml);
            }
        }

        #region Private Methods
        private void PrepareErrorResponse(Response response, string errMsg)
        {
            Status status = new Status { Level = StatusLevel.Error };
            status.Messages = new Messages { errMsg };
            response.DateTimeStamp = DateTime.Now;
            response.Level = StatusLevel.Error;
            response.StatusList.Add(status);

        }
        private void PrepareSuccessResponse(Response response, string errMsg)
        {
            Status status;
            if (errMsg.IndexOf("duplicate") != -1)
                status = new Status { Level = StatusLevel.Warning };
            else
                status = new Status { Level = StatusLevel.Success };

            status.Messages = new Messages { errMsg };
            response.DateTimeStamp = DateTime.Now;
            if (errMsg.IndexOf("duplicate") != -1)
                response.Level = StatusLevel.Warning;
            else
                response.Level = StatusLevel.Success;
            response.StatusList.Add(status);
        }
        #endregion

    }
}
