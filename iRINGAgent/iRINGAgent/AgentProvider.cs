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

        //InitializeScope(project, application);
        //InitializeDataLayer();
    }
}
