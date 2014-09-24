using System;
using log4net;
using System.Collections.Specialized;
using Ninject;
using org.iringtools.library;
using System.Web;
using org.iringtools.utility;


namespace org.iringtools.adapter
{
    public class DictionaryProvider : BaseProvider
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(DictionaryProvider));
        private string _connSecurityDb;
        private int _siteID;
        
        [Inject]
        public DictionaryProvider(NameValueCollection settings)
            : base(settings)
        {
            try
            {
                // We have _settings collection available here.
                _connSecurityDb = settings["SecurityConnection"];
                _siteID = Convert.ToInt32(settings["SiteId"]);
            }
            catch (Exception e)
            {
                _logger.Error("Error initializing adapter provider: " + e.Message);
            }
        }


        public DatabaseDictionary GetDBDictionary(string applicatyionId)
        {
            DatabaseDictionary databaseDictionary = new DatabaseDictionary();
            try
            {
                NameValueList nvl = new NameValueList();
                nvl.Add(new ListItem() { Name = "@ApplicationID", Value = applicatyionId });

                string xmlString = DBManager.Instance.ExecuteXmlQuery(_connSecurityDb, "spgDataBaseDictionary", nvl);
                databaseDictionary = utility.Utility.Deserialize<DatabaseDictionary>(xmlString, true);
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting  Applications: " + ex);
            }
            return databaseDictionary;
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

    }


}
