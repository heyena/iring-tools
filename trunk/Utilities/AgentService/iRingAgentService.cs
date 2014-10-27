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
        private static List<AgentCache> _cacheList = new List<AgentCache>();
        private static List<AgentExchange> _exchangeList = new List<AgentExchange>();
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
                    int count = _cacheList.Count;
                    Task[] tasks = new Task[count];

                    for (int i = 0; i < count; i++)
                    {
                        //check for valid time and state
                        string taskStatus = _cacheList[i].Status;
                        string startDate = _cacheList[i].StartTime.ToString("hh:mm:ss tt", System.Globalization.DateTimeFormatInfo.InvariantInfo);
                        string currentDate = DateTime.Now.ToString("hh:mm:ss tt", System.Globalization.DateTimeFormatInfo.InvariantInfo);
                        DateTime startTime = DateTime.Parse(startDate, System.Globalization.CultureInfo.CurrentCulture);
                        DateTime currentTime = DateTime.Parse(currentDate, System.Globalization.CultureInfo.CurrentCulture);

                        //check for occurance validity
                        isValid = CheckValidOccurance(i, "cache");

                        if (isValid)
                        {
                            if ((taskStatus.Equals("Ready")) && (_cacheList[i].Active).ToString().Equals("1"))
                            {
                                _eventLog1.WriteEntry("Starting cache process " + _cacheList[i].TaskName);
                                tasks[i] = Task.Factory.StartNew(() => StartCacheProcess(i));

                                if (_cacheList[i].RequestTimeout > 0)
                                    threadTimeout = _cacheList[i].RequestTimeout;

                                _eventLog1.WriteEntry(_cacheList[i].TaskName + " thread Timeout " + threadTimeout);
                                Thread.Sleep(threadTimeout);
                                if (!(tasks[i].Exception == null) || tasks[i].IsFaulted)
                                {
                                    _eventLog1.WriteEntry("Error processing cache task " + _cacheList[i].TaskName + " : " + tasks[i].Status);
                                    throw new Exception("Error processing cache task " + _cacheList[i].TaskName + " : " + tasks[i].Status);
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
                            _eventLog1.WriteEntry("Error processing cache task " + ex.Message);
                        }
                    }

                    //process exchange list
                    int exCount = _exchangeList.Count;
                    Task[] exTasks = new Task[exCount];
                    for (int j = 0; j < exCount; j++)
                    {
                        //check for valid time and state
                        string taskStatus = _exchangeList[j].Status;
                        string startDate = _exchangeList[j].StartTime.ToString("hh:mm:ss tt", System.Globalization.DateTimeFormatInfo.InvariantInfo);
                        string currentDate = DateTime.Now.ToString("hh:mm:ss tt", System.Globalization.DateTimeFormatInfo.InvariantInfo);
                        DateTime startTime = DateTime.Parse(startDate, System.Globalization.CultureInfo.CurrentCulture);
                        DateTime currentTime = DateTime.Parse(currentDate, System.Globalization.CultureInfo.CurrentCulture);

                        if (_exchangeList[j].RequestTimeout > 0)
                            threadTimeout = _exchangeList[j].RequestTimeout;

                        isValid = CheckValidOccurance(j, "exchange");

                        if (isValid)
                        {
                            if (taskStatus.Equals("Ready") && (_exchangeList[j].Active).ToString().Equals("1"))
                            {
                                _eventLog1.WriteEntry("Starting exchange process " + _exchangeList[j].TaskName);
                                exTasks[j] = Task.Factory.StartNew(() => StartExchangeProcess(j));

                                if (_exchangeList[j].RequestTimeout > 0)
                                    threadTimeout = _exchangeList[j].RequestTimeout;

                                _eventLog1.WriteEntry(_cacheList[j].TaskName + " thread Timeout " + threadTimeout);
                                Thread.Sleep(threadTimeout);

                                if (!(tasks[j].Exception == null) || tasks[j].IsFaulted)
                                {
                                    _eventLog1.WriteEntry("Error processing exchange task " + _exchangeList[j].TaskName + " : " + tasks[j].Status);
                                    throw new Exception("Error processing exchange task " + _exchangeList[j].TaskName + " : " + tasks[j].Status);
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
                            _eventLog1.WriteEntry("Error processing exchange task " + ex.Message);
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

        private Boolean CheckValidOccurance(int i, string taskType)
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
                
                if (taskType.Equals("cache"))
                {
                    occurance = _cacheList[i].Occurance;
                    nextStartDateTime = DateTime.Parse(_cacheList[i].NextStartDateTime.ToString(), System.Globalization.CultureInfo.CurrentCulture);
                    endDateTime = DateTime.Parse(_cacheList[i].EndDateTime.ToString(), System.Globalization.CultureInfo.CurrentCulture);
                }
                else 
                {
                    occurance = _exchangeList[i].Occurance;
                    nextStartDateTime = DateTime.Parse(_exchangeList[i].NextStartDateTime.ToString(), System.Globalization.CultureInfo.CurrentCulture);
                    endDateTime = DateTime.Parse(_exchangeList[i].EndDateTime.ToString(), System.Globalization.CultureInfo.CurrentCulture);
                }

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

        private void StartCacheProcess(int k)
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
                if (int.Parse(_cacheList[k].CachePageSize) > 0)
                {
                    settings.Set("cachePage", _cacheList[k].CachePageSize);
                    settings.Set("CachePageSize", _cacheList[k].CachePageSize);
                }

                adapterSettings = new AdapterSettings();
                adapterSettings.AppendSettings(settings);

                //update status to Busy in DB 
                updateSQL = string.Format(UPDATE_SQL_TPL, "SCHEDULECACHE", "Status = 'Busy'", " where Schedule_Cache_Id = '" + _cacheList[k].ScheduleCacheId + "'");
                idSQLMap[_cacheList[k].ScheduleCacheId] = updateSQL;
                DBManager.Instance.ExecuteUpdate(_agentConnStr, idSQLMap);
            
                //Call agent provider
                AgentProvider agentProvider = new AgentProvider(adapterSettings);
                AgentCache cacheConfig = (AgentCache)_cacheList[k];
                _logger.Debug("Calling cache process task with config no. " + k);
                agentProvider.ProcessTask(cacheConfig);
           
                //update status and completion time in DB 
                updateSQL = string.Format(UPDATE_SQL_TPL, "SCHEDULECACHE", "Status = 'Ready'", " where Schedule_Cache_Id = '" + _cacheList[k].ScheduleCacheId + "'");
                idSQLMap[_cacheList[k].ScheduleCacheId] = updateSQL;
                DBManager.Instance.ExecuteUpdate(_agentConnStr, idSQLMap);

                //update nextstartdatetime
                string currentDate = DateTime.Now.ToString("hh:mm:ss tt", System.Globalization.DateTimeFormatInfo.InvariantInfo);
                currentDateTime = DateTime.Parse(currentDate, System.Globalization.CultureInfo.CurrentCulture);
                endTime = DateTime.Parse(_cacheList[k].EndTime.ToString(), System.Globalization.CultureInfo.CurrentCulture);
                endDateTime = DateTime.Parse(_cacheList[k].EndDateTime.ToString(), System.Globalization.CultureInfo.CurrentCulture);
                startDateTime = DateTime.Parse(_cacheList[k].StartTime.ToString(), System.Globalization.CultureInfo.CurrentCulture);
                DateTime nextStartDateTime = currentDateTime;
                string occurance = _cacheList[k].Occurance;
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
                updateSQL = string.Format(UPDATE_SQL_TPL, "SCHEDULECACHE", "NextStart_Date_Time = '" + nextStartDateTime + "'", " where Schedule_Cache_Id = '" + _cacheList[k].ScheduleCacheId + "'");
                idSQLMap[_cacheList[k].ScheduleCacheId] = updateSQL;
                DBManager.Instance.ExecuteUpdate(_agentConnStr, idSQLMap);

                updateSQL = string.Format(UPDATE_SQL_TPL, "SCHEDULECACHE", "End_Time = '" + DateTime.Now + "'", " where Schedule_Cache_Id = '" + _cacheList[k].ScheduleCacheId + "'");
                idSQLMap[_cacheList[k].ScheduleCacheId] = updateSQL;
                DBManager.Instance.ExecuteUpdate(_agentConnStr, idSQLMap);

                //if currenttime is greater than enddatetime then set active flag to 0
                if (currentDateTime >= endDateTime)
                {
                    updateSQL = string.Format(UPDATE_SQL_TPL, "SCHEDULECACHE", "Active = 0 " , " where Schedule_Cache_Id = '" + _cacheList[k].ScheduleCacheId + "'");
                    idSQLMap[_cacheList[k].ScheduleCacheId] = updateSQL;
                    DBManager.Instance.ExecuteUpdate(_agentConnStr, idSQLMap);
                }

            }
            catch (Exception ex)
            {
                _eventLog1.WriteEntry("Error in StartCacheProcess " + _cacheList[k].TaskName + " : " + ex.Message);
            }
        }

        private void StartExchangeProcess(int l)
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
                adapterSettings = new AdapterSettings();
                adapterSettings.AppendSettings(settings);

                //update status to Busy in DB 
                updateSQL = string.Format(UPDATE_SQL_TPL, "SCHEDULEEXCHANGE", "Status = 'Busy'", " where Schedule_Exchange_Id = '" + _exchangeList[l].ScheduleExchangeId + "'");
                idSQLMap[_exchangeList[l].ScheduleExchangeId] = updateSQL;
                DBManager.Instance.ExecuteUpdate(_agentConnStr, idSQLMap);

                //Call agent provider
                AgentProvider agentProvider = new AgentProvider(adapterSettings);
                AgentExchange exchangeConfig = (AgentExchange)_exchangeList[l];
                _logger.Debug("Calling exchange process task with config no. " + l);

                agentProvider.ProcessTask(exchangeConfig);
           
                //update status and completion time in DB 
                updateSQL = string.Format(UPDATE_SQL_TPL, "SCHEDULEEXCHANGE", "Status = 'Ready'", " where Schedule_Exchange_Id = '" + _exchangeList[l].ScheduleExchangeId + "'");
                idSQLMap[_exchangeList[l].ScheduleExchangeId] = updateSQL;
                DBManager.Instance.ExecuteUpdate(_agentConnStr, idSQLMap);

                //update nextstartdatetime
                string currentDate = DateTime.Now.ToString("hh:mm:ss tt", System.Globalization.DateTimeFormatInfo.InvariantInfo);
                currentDateTime = DateTime.Parse(currentDate, System.Globalization.CultureInfo.CurrentCulture);
                endTime = DateTime.Parse(_exchangeList[l].EndTime.ToString(), System.Globalization.CultureInfo.CurrentCulture);
                endDateTime = DateTime.Parse(_exchangeList[l].EndDateTime.ToString(), System.Globalization.CultureInfo.CurrentCulture);
                startDateTime = DateTime.Parse(_exchangeList[l].StartTime.ToString(), System.Globalization.CultureInfo.CurrentCulture);
                DateTime nextStartDateTime = currentDateTime;

                string occurance = _exchangeList[l].Occurance;
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
                updateSQL = string.Format(UPDATE_SQL_TPL, "SCHEDULEEXCHANGE", "NextStart_Date_Time = '" + nextStartDateTime + "'", " where Schedule_Exchange_Id = '" + _exchangeList[l].ScheduleExchangeId + "'");
                idSQLMap[_exchangeList[l].ScheduleExchangeId] = updateSQL;
                DBManager.Instance.ExecuteUpdate(_agentConnStr, idSQLMap);

                updateSQL = string.Format(UPDATE_SQL_TPL, "SCHEDULEEXCHANGE", "End_Time = '" + DateTime.Now + "'", " where Schedule_Exchange_Id = '" + _exchangeList[l].ScheduleExchangeId + "'");
                idSQLMap[_exchangeList[l].ScheduleExchangeId] = updateSQL;
                DBManager.Instance.ExecuteUpdate(_agentConnStr, idSQLMap);

                //if currenttime is greater than enddatetime then set active flag to 0
                if (currentDateTime >= endDateTime)
                {
                    updateSQL = string.Format(UPDATE_SQL_TPL, "SCHEDULEEXCHANGE", "Active = 0 ", " where Schedule_Exchange_Id = '" + _exchangeList[l].ScheduleExchangeId + "'");
                    idSQLMap[_exchangeList[l].ScheduleExchangeId] = updateSQL;
                    DBManager.Instance.ExecuteUpdate(_agentConnStr, idSQLMap);
                }
            }
            catch (Exception ex)
            {
                _eventLog1.WriteEntry("Error in StartExchangeProcess " + _exchangeList[l].TaskName + " : " + ex.Message);
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
                //_timerInterval = long.Parse(settings.Get("TimerInterval"));


                string cacheSQL = "SELECT * FROM SCHEDULECACHE ";
                DataTable cacheTable = DBManager.Instance.ExecuteQuery(_agentConnStr, cacheSQL);
                if (cacheTable != null && cacheTable.Rows.Count > 0)
                {
                    foreach (DataRow dataRow in cacheTable.Rows)
                    {
                        try
                        {
                            _cacheList.Add(new AgentCache
                            {
                                ScheduleCacheId = dataRow["Schedule_Cache_Id"].ToString(),
                                TaskName = dataRow["Task_Name"].ToString(),
                                Project = dataRow["Project"].ToString(),
                                App = dataRow["App"].ToString(),
                                CachePageSize = dataRow["Cache_Page_Size"].ToString(),
                                SsoUrl = dataRow["Sso_Url"].ToString(),
                                ClientId = dataRow["Client_Id"].ToString(),
                                ClientSecret = dataRow["Client_Secret"].ToString(),
                                GrantType = dataRow["Grant_Type"].ToString(),
                                AppKey = dataRow["App_Key"].ToString(),
                                AccessToken = dataRow["Access_Token"].ToString(),
                                RequestTimeout = Convert.ToInt32(dataRow["Request_Timeout"]),
                                StartTime = Convert.ToDateTime(dataRow["Start_Time"]),
                                EndTime = Convert.ToDateTime(dataRow["End_Time"]),
                                CreatedDate = Convert.ToDateTime(dataRow["Created_Date"]),
                                CreatedBy = dataRow["Created_By"].ToString(),
                                Occurance = dataRow["Occurance"].ToString(),
                                NextStartDateTime = Convert.ToDateTime(dataRow["NextStart_Date_Time"]),
                                EndDateTime = Convert.ToDateTime(dataRow["End_Date_Time"]),
                                Status = dataRow["Status"].ToString(),
                                Active = Convert.ToInt32(dataRow["Active"])
                            });
                        }
                        catch (Exception ex)
                        {
                            _eventLog1.WriteEntry(string.Format("Error getting cache configuration data.", ex));
                            throw ex;
                        }
                    }
                }

                string exchangeSQL = "SELECT * FROM SCHEDULEEXCHANGE ";
                DataTable exchangeTable = DBManager.Instance.ExecuteQuery(_agentConnStr, exchangeSQL);
                if (exchangeTable != null && exchangeTable.Rows.Count > 0)
                {
                    foreach (DataRow dataRow in exchangeTable.Rows)
                    {
                        try
                        {
                            _exchangeList.Add(new AgentExchange
                            {
                                ScheduleExchangeId = dataRow["Schedule_Exchange_Id"].ToString(),
                                TaskName = dataRow["Task_Name"].ToString(),
                                Scope = dataRow["Scope"].ToString(),
                                BaseUrl = dataRow["Base_Url"].ToString(),
                                ExchangeId = dataRow["Exchange_Id"].ToString(),
                                SsoUrl = dataRow["Sso_Url"].ToString(),
                                ClientId = dataRow["Client_Id"].ToString(),
                                ClientSecret = dataRow["Client_Secret"].ToString(),
                                GrantType = dataRow["Grant_Type"].ToString(),
                                RequestTimeout = Convert.ToInt32(dataRow["Request_Timeout"]),
                                StartTime = Convert.ToDateTime(dataRow["Start_Time"]),
                                EndTime = Convert.ToDateTime(dataRow["End_Time"]),
                                CreatedDate = Convert.ToDateTime(dataRow["Created_Date"]),
                                CreatedBy = dataRow["Created_By"].ToString(),
                                Occurance = dataRow["Occurance"].ToString(),
                                NextStartDateTime = Convert.ToDateTime(dataRow["NextStart_Date_Time"]),
                                EndDateTime = Convert.ToDateTime(dataRow["End_Date_Time"]),
                                Status = dataRow["Status"].ToString(),
                                Active = Convert.ToInt32(dataRow["Active"])
                            });
                        }
                        catch (Exception ex)
                        {
                            _eventLog1.WriteEntry(string.Format("Error getting exchange configuration data.", ex));
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
