using org.iringtools.adapter;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using org.iringtools.utility;
using org.iringtools.library;
using org.iringtools.mapping;
using log4net;
using Newtonsoft.Json;
using Ninject;
using Ninject.Extensions.Xml;

namespace org.iringtools.agent
{
    class AgentProvider : BaseProvider
    {
        private static ILog _logger = LogManager.GetLogger(typeof(AgentProvider));
        private string[] arrSpecialcharlist;
        private string[] arrSpecialcharValue;
        

        [Inject]
        public AgentProvider(NameValueCollection settings)
            : base(settings)
        {

            try
            {
                if (_settings["SpCharList"] != null && _settings["SpCharValue"] != null)
                {
                    arrSpecialcharlist = _settings["SpCharList"].ToString().Split(',');
                    arrSpecialcharValue = _settings["SpCharValue"].ToString().Split(',');
                }

                if (_settings["LdapConfiguration"] != null && _settings["LdapConfiguration"].ToLower() == "true")
                {
                    //utility.Utility.isLdapConfigured = true;
                    //utility.Utility.InitializeConfigurationRepository(new Type[] { 
            //typeof(DataDictionary), 
            //typeof(DatabaseDictionary),
            //typeof(XElementClone),
            //typeof(AuthorizedUsers),
            //typeof(Mapping)
          //});
                }
            }
            catch (Exception e)
            {
                _logger.Error("Error initializing adapter provider: " + e.Message);
            }
        }

        public void ProcessTask(Task task)
        {
            //DataFilter filter = null;
            int pageSize = 0;
            //List<IDataObject> dataObjects = null;

            try
            {
                InitializeScope(task.Project, task.App);
                ScopeProject project = _scopes.Find(x => x.Name.ToLower() == task.Project.ToLower());
                InitializeDataLayer();
                _dictionary = _dataLayerGateway.GetDictionary();

                if (task.taskParams.ContainsKey("pagesize"))
                {
                    foreach (var valItem in task.taskParams["pagesize"])
                    {
                        pageSize = Convert.ToInt32(valItem.ToString());
                    }
                }
                if(pageSize == 0)
                    pageSize = 1000;

                foreach (DataObject objDef in _dictionary.dataObjects)
                {
                    string objectType = objDef.objectName;
                    //int start = 0;

                    RefreshCache(task.Scope, task.App, false);
                    //UpdateCacheInfo(task.Scope, task.App, objectType);

                 
                }
            }
            catch (Exception e)
            {
                _logger.Error("Error processing task: " + task.TaskType + " for " + task.Scope + e.Message);
            }
        }
      
    }
}
