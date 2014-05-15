using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using log4net;
using Newtonsoft.Json.Linq;
using org.iringtools.library;
using org.iringtools.adapter;
using org.iringtools.utility;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using org.iringtools.agent;


namespace org.iringtools.agent
{
    class Program
    {
        private static ILog _logger = LogManager.GetLogger(typeof(Program));
        private static TaskSequence _sequence = null;
       
        static void Main(string[] args)
        {
            try
            {
                log4net.Config.XmlConfigurator.Configure();
                //GenerateSampleAgentSequence();
                string config = args[0]; //"D:\\Dev\\svn\\branches\\agent\\iring-tools\\iRINGAgent\\iRINGAgent\\bin\\App_Data\\AgentConfig.xml"; 
                if (Initialize(config))
                {
                    RunTask();
                }

                _logger.Info("Agent task completed.");
            }
            catch (Exception ex)
            {
                _logger.Error("Agent task failed: " + ex.Message, ex);
            }
        }

        static void RunTask()
        {
            _logger.Debug("RunTask ...");
            AdapterSettings adapterSettings;
            NameValueCollection settings;

            foreach (Task task in _sequence.Tasks)
            {
                string project = task.Project;
                string app = task.App;
                settings = ConfigurationManager.AppSettings;

                adapterSettings = new AdapterSettings();
                adapterSettings.AppendSettings(settings);
                adapterSettings["ProjectName"] = project;
                adapterSettings["ApplicationName"] = app;
                adapterSettings["Scope"] = project + "." + app;

                //Call agent provider
                AgentProvider agentProvider = new AgentProvider(adapterSettings);
                agentProvider.ProcessTask(task);
            }
                    
            _logger.Info("Task finished: " );
        }

        static bool Initialize(string configPath)
        {
            _logger.Debug("Initialize ...");

            try
            {
                if (File.Exists(configPath))
                {
                    AgentConfig config = Utility.Read<AgentConfig>(configPath, true);
                    foreach (TaskSequence sequence in config)
                    {
                        _sequence = sequence;
                        break;
                    }
                }
                else
                {
                    _logger.Error("Agent Configuration [" + configPath + "] not found.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Initialization failed: " + ex.Message, ex);
                return false;
            }

            return true;
        }

        static void GenerateSampleAgentSequence()
        {
            TaskSequence sequence = new TaskSequence
            {
                Name = "Test",
                Tasks = new List<Task>
                {
                    new Task
                    {
                        TaskType = "cache",
                        BaseURL = "",
                        Project = "99999_000",
                        App = "iw",
                        Scope = "99999_000",
                        ExchangeId = ""
                    }
                }
            };

            //Stream stream = Utility.SerializeToBinaryStream<AgentSequence>(sequence);

            //Utility.WriteStream(stream, "SampleSequence.dat");

            Stream stream = Utility.ReadStream("SampleAgentSequence.dat");

            sequence = Utility.DeserializeBinaryStream<TaskSequence>(stream);

            Utility.Write<TaskSequence>(sequence, "SampleAgentSequence.xml", true);
        }

       
    }
}
