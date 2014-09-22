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
        private System.Diagnostics.EventLog eventLog1;
        private static List<AgentCache> _cacheList = new List<AgentCache>();
        private static List<AgentExchange> _exchangeList = new List<AgentExchange>();
        private ServiceStatus _serviceStatus = new ServiceStatus();
        private static string _agentConnStr = null;
        public const string UPDATE_SQL_TPL = "UPDATE {0} SET {1} {2}";

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(IntPtr handle, ref ServiceStatus serviceStatus);

        public iRINGAgentService()
        {
            InitializeComponent();

            eventLog1 = new System.Diagnostics.EventLog();
            if (!System.Diagnostics.EventLog.SourceExists("iRINGAgentService"))
            {
                System.Diagnostics.EventLog.CreateEventSource("iRINGAgentService", "iRINGAgentServiceLog");
            }
            eventLog1.Source = "iRINGAgentService";
            eventLog1.Log = "iRINGAgentServiceLog";
           
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

            eventLog1.WriteEntry("In OnStart");
            if (!Initialize())
            {
                eventLog1.WriteEntry("Error getting configuration data.");
            }
            else
            {
                // Update the service state to Running.
                _serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
                SetServiceStatus(this.ServiceHandle, ref _serviceStatus);

                var parentTask = new Task(() =>
                {
                    //process cache list
                    int count = _cacheList.Count;
                    Task[] tasks = new Task[count];
                    int threadTimeout = 9000;

                    for (int i = 0; i < count; i++)
                    {
                        _logger.Debug("Start caching task " + _cacheList[i].TaskName);
                        eventLog1.WriteEntry("Start caching task " + _cacheList[i].TaskName);

                        //check for valid time and state
                        string taskStatus = _cacheList[i].Status;
                        string startDate = _cacheList[i].StartTime.ToString("hh:mm:ss tt", System.Globalization.DateTimeFormatInfo.InvariantInfo);
                        string currentDate = DateTime.Now.ToString("hh:mm:ss tt", System.Globalization.DateTimeFormatInfo.InvariantInfo);
                        DateTime startTime = DateTime.Parse(startDate, System.Globalization.CultureInfo.CurrentCulture);
                        DateTime currentTime = DateTime.Parse(currentDate, System.Globalization.CultureInfo.CurrentCulture);
                        if (taskStatus.Equals("Ready") && currentTime >= startTime)
                        {
                            tasks[i] = Task.Factory.StartNew(() => StartCacheProcess(i));

                            if (_cacheList[i].RequestTimeout > 0)
                                threadTimeout = _cacheList[i].RequestTimeout;

                            Thread.Sleep(threadTimeout);
                            if (!(tasks[i].Exception == null) || tasks[i].IsFaulted)
                            {
                                eventLog1.WriteEntry("Error processing cache task " + _cacheList[i].TaskName + " : " + tasks[i].Status);
                                throw new Exception("Error processing cache task " + _cacheList[i].TaskName + " : " + tasks[i].Status);
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
                            eventLog1.WriteEntry("Error processing cache task " + ex.Message);
                        
                        }
                    }
            
                    //process exchange list
                    int exCount = _exchangeList.Count;
                    Task[] exTasks = new Task[exCount];
                    for (int j = 0; j < exCount; j++)
                    {
                        _logger.Debug("Starting exchange task " + _exchangeList[j].TaskName);
                        eventLog1.WriteEntry("Starting exchange task " + _exchangeList[j].TaskName);
                        //check for valid time and state
                        string taskStatus = _exchangeList[j].Status;
                        string startDate = _exchangeList[j].StartTime.ToString("hh:mm:ss tt", System.Globalization.DateTimeFormatInfo.InvariantInfo);
                        string currentDate = DateTime.Now.ToString("hh:mm:ss tt", System.Globalization.DateTimeFormatInfo.InvariantInfo);
                        DateTime startTime = DateTime.Parse(startDate, System.Globalization.CultureInfo.CurrentCulture);
                        DateTime currentTime = DateTime.Parse(currentDate, System.Globalization.CultureInfo.CurrentCulture);

                        if (_cacheList[j].RequestTimeout > 0)
                            threadTimeout = _cacheList[j].RequestTimeout;

                        if (taskStatus.Equals("Ready") && currentTime >= startTime)
                        {
                            exTasks[j] = Task.Factory.StartNew(() => StartExchangeProcess(j));
                            Thread.Sleep(threadTimeout);
                            if (!(tasks[j].Exception == null) || tasks[j].IsFaulted)
                            {
                                eventLog1.WriteEntry("Error processing exchange task " + _exchangeList[j].TaskName + " : " + tasks[j].Status);
                                throw new Exception("Error processing exchange task " + _exchangeList[j].TaskName + " : " + tasks[j].Status);
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
                            eventLog1.WriteEntry("Error processing exchange task " + ex.Message);
                        }
                    }

                });
                parentTask.Start();
                parentTask.Wait();
                //parentTask.Dispose();
            }
        }

        protected override void OnStop()
        {
            // Update the service state to stop.
            _serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref _serviceStatus);

        }

        private void StartCacheProcess(int k)
        {
            AdapterSettings adapterSettings;
            NameValueCollection settings;
            Dictionary<string, string> idSQLMap = new Dictionary<string, string>();
            string updateSQL = null;

            settings = ConfigurationManager.AppSettings;
            
            
            if (int.Parse(_cacheList[k].CachePageSize) > 0)
                settings.Set("cachePage", _cacheList[k].CachePageSize);

            adapterSettings = new AdapterSettings();
            adapterSettings.AppendSettings(settings);

            //update status to Busy in DB 
            updateSQL = string.Format(UPDATE_SQL_TPL, "SCHEDULECACHE", "Status = 'Busy'", " where Schedule_Cache_Id = '" + _cacheList[k].ScheduleCacheId + "'");
            idSQLMap[_cacheList[k].ScheduleCacheId] = updateSQL;
            DBManager.Instance.ExecuteUpdate(_agentConnStr, idSQLMap);
            

            //Call agent provider
            try
            {
                AgentProvider agentProvider = new AgentProvider(adapterSettings);
                AgentCache cacheConfig = (AgentCache)_cacheList[k];
                _logger.Debug("Calling cache process task with config no. " + k);
                agentProvider.ProcessTask(cacheConfig);
            }
            catch (Exception ex)
            {
                eventLog1.WriteEntry("Error in StartCacheProcess " + _cacheList[k].TaskName + " : " + ex.Message);
            }

            //update status and completion time in DB 
            updateSQL = string.Format(UPDATE_SQL_TPL, "SCHEDULECACHE", "Status = 'Ready'", " where Schedule_Cache_Id = '" + _cacheList[k].ScheduleCacheId + "'");
            idSQLMap[_cacheList[k].ScheduleCacheId] = updateSQL;
            DBManager.Instance.ExecuteUpdate(_agentConnStr, idSQLMap);
            updateSQL = string.Format(UPDATE_SQL_TPL, "SCHEDULECACHE", "End_Time = '" + DateTime.Now + "'", " where Schedule_Cache_Id = '" + _cacheList[k].ScheduleCacheId + "'");
            idSQLMap[_cacheList[k].ScheduleCacheId] = updateSQL;
            DBManager.Instance.ExecuteUpdate(_agentConnStr, idSQLMap);
        }

        private void StartExchangeProcess(int l)
        {
            AdapterSettings adapterSettings;
            NameValueCollection settings;
            Dictionary<string, string> idSQLMap = new Dictionary<string, string>();
            string updateSQL = null;

            settings = ConfigurationManager.AppSettings;

            adapterSettings = new AdapterSettings();
            adapterSettings.AppendSettings(settings);

            //Call agent provider
            try
            {
                AgentProvider agentProvider = new AgentProvider(adapterSettings);
                AgentExchange exchangeConfig = (AgentExchange)_exchangeList[l];
                _logger.Debug("Calling exchange process task with config no. " + l);

                agentProvider.ProcessTask(exchangeConfig);
            }
            catch (Exception ex)
            {
                eventLog1.WriteEntry("Error in StartExchangeProcess " + _exchangeList[l].TaskName + " : " + ex.Message);
            }

            //update status and completion time in DB 
            updateSQL = string.Format(UPDATE_SQL_TPL, "SCHEDULEEXCHANGE", "Status = 'Ready'", " where Schedule_Exchange_Id = '" + _exchangeList[l].ScheduleExchangeId + "'");
            idSQLMap[_cacheList[l].ScheduleCacheId] = updateSQL;
            DBManager.Instance.ExecuteUpdate(_agentConnStr, idSQLMap);
            updateSQL = string.Format(UPDATE_SQL_TPL, "SCHEDULEEXCHANGE", "End_Time = '" + DateTime.Now + "'", " where Schedule_Exchange_Id = '" + _exchangeList[l].ScheduleExchangeId + "'");
            idSQLMap[_cacheList[l].ScheduleCacheId] = updateSQL;
            DBManager.Instance.ExecuteUpdate(_agentConnStr, idSQLMap);

        }

        static bool Initialize()
        {
            _logger.Debug("Initialize ...");
            NameValueCollection settings;

            try
            {
                //connect to database and setup config object
                settings = ConfigurationManager.AppSettings;
                _agentConnStr = settings.Get("iRINGAgentConnStr");
                _agentConnStr = EncryptionUtility.Decrypt(_agentConnStr);

                string cacheSQL = "SELECT * FROM SCHEDULECACHE WHERE ACTIVE = 1";
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
                                Status = dataRow["Status"].ToString(),
                                Active = Convert.ToInt32(dataRow["Active"])
                            });
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(string.Format("Error getting cache configuration data.", ex));
                            throw ex;
                        }
                    }
                }

                string exchangeSQL = "SELECT * FROM SCHEDULEEXCHANGE WHERE ACTIVE = 1";
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
                                Status = dataRow["Status"].ToString(),
                                Active = Convert.ToInt32(dataRow["Active"])
                            });
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(string.Format("Error getting exchange configuration data.", ex));
                            throw ex;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.Error("Initialization failed: " + ex.Message, ex);
                return false;
            }
            _logger.Debug("Successfully retrieved configuration data.");
            return true;
        }

       
    }
}
