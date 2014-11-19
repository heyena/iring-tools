using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Data.SqlClient;
using System.Collections.Specialized;
using System.Configuration;
using log4net;
using Newtonsoft.Json.Linq;
using org.iringtools.library;
using org.iringtools.adapter;
using org.iringtools.utility;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace iRINGAgentService
{
    public enum ServiceState
    {
        SERVICE_STOPPED = 0x00000001,
        SERVICE_START_PENDING = 0x00000002,
        SERVICE_STOP_PENDING = 0x00000003,
        SERVICE_RUNNING = 0x00000004,
        SERVICE_CONTINUE_PENDING = 0x00000005,
        SERVICE_PAUSE_PENDING = 0x00000006,
        SERVICE_PAUSED = 0x00000007,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ServiceStatus
    {
        public long dwServiceType;
        public ServiceState dwCurrentState;
        public long dwControlsAccepted;
        public long dwWin32ExitCode;
        public long dwServiceSpecificExitCode;
        public long dwCheckPoint;
        public long dwWaitHint;
    };

    public partial class iRINGAgentService : ServiceBase
    {
        private static ILog _logger = LogManager.GetLogger(typeof(iRINGAgentService));
        private System.Diagnostics.EventLog _eventLog1;
        private static List<AgentConfig> _configList = new List<AgentConfig>();
        private ServiceStatus _serviceStatus = new ServiceStatus();
        private static string _agentConnStr = null;
        private static long _timerInterval = 600000;
        public const string UPDATE_SQL_TPL = "UPDATE {0} SET {1} {2}";
        System.Timers.Timer _timer = new System.Timers.Timer();

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(IntPtr handle, ref ServiceStatus serviceStatus);

        public iRINGAgentService()
        {
            InitializeComponent();

            _eventLog1 = new System.Diagnostics.EventLog();
            if (!System.Diagnostics.EventLog.SourceExists("iRINGAgentService"))
            {
                System.Diagnostics.EventLog.CreateEventSource("iRINGAgentService", "iRINGAgentServiceLog");
            }
            _eventLog1.Source = "iRINGAgentService";
            _eventLog1.Log = "iRINGAgentServiceLog";
           
        }

        public void RunConsole(string[] args)
        {
            this.OnStart(args);
            Console.WriteLine("Press ENTER to stop.");
            Console.ReadLine();
            this.OnStop();
            Console.WriteLine("Service stopped.");
            Console.ReadLine();
        }

        protected override void OnStart(string[] args)
        {
            // Update the service state to Start Pending.

            _serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            _serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref _serviceStatus);

            _eventLog1.WriteEntry("In OnStart");
            //if (!Initialize())
            //{
            //    _eventLog1.WriteEntry("Error getting configuration data.");
            //}
            //else
            //{
            // Update the service state to Running.
            _serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref _serviceStatus);

            NameValueCollection settings;
            settings = ConfigurationManager.AppSettings;
            _timerInterval = long.Parse(settings.Get("TimerInterval"));

            _timer.Enabled = true;
            _timer.Interval = _timerInterval;
            _timer.Elapsed += new System.Timers.ElapsedEventHandler(TimerElapsed);

            //}
        }

        protected void TimerElapsed(object source, System.Timers.ElapsedEventArgs eventArgs)
        {
            int threadTimeout = 9000;
            Boolean isValid = false;
            _eventLog1.WriteEntry("The Elapsed event was raised at " + eventArgs.SignalTime);

            //Perform caching and Exchange Tasks
            var parentTask = new Task(() =>
            {
                if (!Initialize())
                {
                    _eventLog1.WriteEntry("Error getting configuration data.");
                }
                else
                {
                    //process cache list
                    int count = _configList.Count;
                    Task[] tasks = new Task[count];

                    for (int i = 0; i < count; i++)
                    {
                        //check for valid time and state
                        string taskStatus = _configList[i].Status;
                        string startDate = _configList[i].StartDateTime.ToString("hh:mm:ss tt", System.Globalization.DateTimeFormatInfo.InvariantInfo);
                        string currentDate = DateTime.Now.ToString("hh:mm:ss tt", System.Globalization.DateTimeFormatInfo.InvariantInfo);
                        DateTime startTime = DateTime.Parse(startDate, System.Globalization.CultureInfo.CurrentCulture);
                        DateTime currentTime = DateTime.Parse(currentDate, System.Globalization.CultureInfo.CurrentCulture);

                        //check for occurance validity
                        isValid = CheckValidOccurance(i);

                        if (isValid)
                        {
                            if ((taskStatus.Equals("Ready")) && (_configList[i].Active).ToString().Equals("1"))
                            {
                                _eventLog1.WriteEntry("Starting process for job " + _configList[i].JobId);
                                tasks[i] = Task.Factory.StartNew(() => StartProcess(i));

                                if (_configList[i].RequestTimeout > 0)
                                    threadTimeout = _configList[i].RequestTimeout;

                                _eventLog1.WriteEntry(_configList[i].JobId + " thread Timeout " + threadTimeout);
                                Thread.Sleep(threadTimeout);
                                if (!(tasks[i].Exception == null) || tasks[i].IsFaulted)
                                {
                                    _eventLog1.WriteEntry("Error processing task " + _configList[i].JobId + " : " + tasks[i].Status);
                                    throw new Exception("Error processing task " + _configList[i].JobId + " : " + tasks[i].Status);
                                }
                            }
                        }
                    }

                    try
                    {
                        for (int l = 0; l < count; l++)
                        {
                            if (!(tasks[l] == null))
                            {
                                tasks[l].Wait();
                            }
                        }
                    }
                    catch (AggregateException ex)
                    {
                        foreach (var exception in ex.Flatten().InnerExceptions)
                        {
                            _eventLog1.WriteEntry("Error processing task " + ex.Message);
                        }
                    }
                }
            });
            parentTask.Start();
            parentTask.Wait();
            //parentTask.Dispose();
        }

        protected override void OnStop()
        {
            // Update the service state to stop.
            _serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref _serviceStatus);
        }

        private Boolean CheckValidOccurance(int i)
        {
            Boolean bReturn = false;
            string occurance = "";
            DateTime nextStartDateTime;
            DateTime endDateTime;
            DateTime currentDateTime;

            try
            {
                string currentDate = DateTime.Now.ToString("hh:mm:ss tt", System.Globalization.DateTimeFormatInfo.InvariantInfo);
                currentDateTime = DateTime.Parse(currentDate, System.Globalization.CultureInfo.CurrentCulture);
                
                occurance = _configList[i].Occurance;
                nextStartDateTime = DateTime.Parse(_configList[i].NextStartDateTime.ToString(), System.Globalization.CultureInfo.CurrentCulture);
                endDateTime = DateTime.Parse(_configList[i].EndDateTime.ToString(), System.Globalization.CultureInfo.CurrentCulture);
                
                if (occurance.Equals("Immediate"))
                {
                    bReturn = true;
                }
                else if ((currentDateTime > nextStartDateTime) && (currentDateTime < endDateTime))
                {
                    bReturn = true;
                }
            }
            catch (Exception ex)
            {
                _eventLog1.WriteEntry("Error checking Occurance Validity for Task " + ex.Message);
            }

            return bReturn;
        }

        private void StartProcess(int k)
        {
            AdapterSettings adapterSettings;
            NameValueCollection settings;
            Dictionary<string, string> idSQLMap = new Dictionary<string, string>();
            string updateSQL = null;
            DateTime currentDateTime;
            DateTime endTime;
            DateTime endDateTime;
            DateTime startDateTime;

            try
            {
                settings = ConfigurationManager.AppSettings;
                if (!string.IsNullOrEmpty(_configList[k].CachePageSize))
                {
                    if (int.Parse(_configList[k].CachePageSize) > 0)
                    {
                        settings.Set("cachePage", _configList[k].CachePageSize);
                        settings.Set("CachePageSize", _configList[k].CachePageSize);
                    }
                }

                adapterSettings = new AdapterSettings();
                adapterSettings.AppendSettings(settings);

                //update status to Busy in DB 
                updateSQL = string.Format(UPDATE_SQL_TPL, "Schedule", "Status = 'Busy'", " where Schedule_Id = '" + _configList[k].ScheduleId + "'");
                idSQLMap[_configList[k].JobId] = updateSQL;
                DBManager.Instance.ExecuteUpdate(_agentConnStr, idSQLMap);

                updateSQL = string.Format(UPDATE_SQL_TPL, "JobSchedule", "Last_Start_DateTime = '" + DateTime.Now + "'", " where Schedule_Id = '" + _configList[k].ScheduleId + "' and Job_Id = '" + _configList[k].JobId + "'");
                idSQLMap[_configList[k].JobId] = updateSQL;
                DBManager.Instance.ExecuteUpdate(_agentConnStr, idSQLMap);

                //Call agent provider
                AgentProvider agentProvider = new AgentProvider(adapterSettings);
                AgentConfig agentConfig = (AgentConfig)_configList[k];
                _logger.Debug("Calling task with config no. " + k);
                agentProvider.ProcessTask(agentConfig);

                //update status and completion time in DB 
                updateSQL = string.Format(UPDATE_SQL_TPL, "Schedule", "Status = 'Ready'", " where Schedule_Id = '" + _configList[k].ScheduleId + "'");
                idSQLMap[_configList[k].JobId] = updateSQL;
                DBManager.Instance.ExecuteUpdate(_agentConnStr, idSQLMap);

                //update nextstartdatetime
                string currentDate = DateTime.Now.ToString("hh:mm:ss tt", System.Globalization.DateTimeFormatInfo.InvariantInfo);
                currentDateTime = DateTime.Parse(currentDate, System.Globalization.CultureInfo.CurrentCulture);
                endTime = DateTime.Parse(_configList[k].EndDateTime.ToString(), System.Globalization.CultureInfo.CurrentCulture);
                endDateTime = DateTime.Parse(_configList[k].EndDateTime.ToString(), System.Globalization.CultureInfo.CurrentCulture);
                startDateTime = DateTime.Parse(_configList[k].StartDateTime.ToString(), System.Globalization.CultureInfo.CurrentCulture);
                DateTime nextStartDateTime = currentDateTime;
                string occurance = _configList[k].Occurance;
                switch (occurance)
                {
                    case "Immediate":
                        nextStartDateTime = currentDateTime;
                        break;
                    case "Daily":
                        nextStartDateTime = currentDateTime.AddHours(24);
                        break;
                    case "Weekly":
                        nextStartDateTime = currentDateTime.AddDays(7);
                        break;
                    case "Monthly":
                        nextStartDateTime = currentDateTime.AddMonths(1);
                        break;
                }
                updateSQL = string.Format(UPDATE_SQL_TPL, "JobSchedule", "Next_Start_DateTime = '" + nextStartDateTime + "'", " where Schedule_Id = '" + _configList[k].ScheduleId + "' and Job_Id = '" + _configList[k].JobId + "'");
                idSQLMap[_configList[k].JobId] = updateSQL;
                DBManager.Instance.ExecuteUpdate(_agentConnStr, idSQLMap);

                //if currenttime is greater than enddatetime then set active flag to 0
                if (currentDateTime >= endDateTime)
                {
                    updateSQL = string.Format(UPDATE_SQL_TPL, "JobSchedule", "Active = 0 ", " where Schedule_Id = '" + _configList[k].ScheduleId + "' and Job_Id = '" + _configList[k].JobId + "'");
                    idSQLMap[_configList[k].JobId] = updateSQL;
                    DBManager.Instance.ExecuteUpdate(_agentConnStr, idSQLMap);
                }

            }
            catch (Exception ex)
            {
                _eventLog1.WriteEntry("Error in StartProcess " + _configList[k].JobId + " : " + ex.Message);
            }
        }

        private bool Initialize()
        {
            _logger.Debug("Initialize ...");
            NameValueCollection settings;

            try
            {
                //connect to database and setup config object
                settings = ConfigurationManager.AppSettings;
                _agentConnStr = settings.Get("iRINGAgentConnStr");
                _agentConnStr = EncryptionUtility.Decrypt(_agentConnStr);
                _configList.Clear();

                string sSql = "select a.job_id,a.is_exchange,a.scope,a.app,a.dataobject,a.xid,a.exchange_url,a.cache_page_size, " +
                               " b.sso_url,b.client_id,b.client_secret,b.access_token,b.app_key,b.grant_type,b.request_timeout, " +
                               " c.schedule_id,c.occurance,c.start_datetime,c.end_datetime,c.status, " +
                               " d.next_start_datetime,d.last_start_datetime,d.active " +
                               " from job a, job_client_info b, schedule c, jobschedule d " +
                               " where a.job_id = b.job_id and a.job_id = d.job_id and c.schedule_id = d.schedule_id ";

                DataTable agentConfig = DBManager.Instance.ExecuteQuery(_agentConnStr, sSql);
                if (agentConfig != null && agentConfig.Rows.Count > 0)
                {
                    foreach (DataRow dataRow in agentConfig.Rows)
                    {
                        try
                        {
                            _configList.Add(new AgentConfig
                            {

                                JobId = dataRow["Job_Id"].ToString(),
                                IsExchange = Convert.ToInt32(dataRow["is_exchange"]),
                                Scope = dataRow["Scope"].ToString(),
                                App = dataRow["App"].ToString(),
                                DataObject = dataRow["DataObject"].ToString(),
                                ExchangeId = dataRow["xid"].ToString(),
                                ExchangeUrl = dataRow["exchange_url"].ToString(),
                                CachePageSize = dataRow["cache_page_size"].ToString(),
                                SsoUrl = dataRow["sso_url"].ToString(),
                                ClientId = dataRow["client_id"].ToString(),
                                ClientSecret = dataRow["client_secret"].ToString(),
                                AccessToken = dataRow["access_token"].ToString(),
                                AppKey = dataRow["app_key"].ToString(),
                                GrantType = dataRow["grant_type"].ToString(),
                                RequestTimeout = Convert.ToInt32(dataRow["request_timeout"]),
                                ScheduleId = dataRow["schedule_id"].ToString(),
                                Occurance = dataRow["occurance"].ToString(),
                                StartDateTime = Convert.ToDateTime(dataRow["start_datetime"]),
                                EndDateTime = Convert.ToDateTime(dataRow["end_datetime"]),
                                Status = dataRow["status"].ToString(),
                                NextStartDateTime = Convert.ToDateTime(dataRow["next_start_datetime"]),
                                LastStartDateTime = Convert.ToDateTime(dataRow["last_start_datetime"]),
                                Active = Convert.ToInt32(dataRow["active"])
                            });
                        }
                        catch (Exception ex)
                        {
                            _eventLog1.WriteEntry(string.Format("Error getting configuration data.", ex));
                            throw ex;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _eventLog1.WriteEntry(string.Format("Initialization failed: " + ex.Message, ex));
                return false;
            }
            _logger.Debug("Successfully retrieved configuration data.");
            return true;
        }

       
    }
}
