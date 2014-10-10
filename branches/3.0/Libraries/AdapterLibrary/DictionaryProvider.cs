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

        public DataDictionary GetDataDictionary(string applicatyionId)
        {
            DataDictionary dataDictionary = new DataDictionary();
            try
            {
                NameValueList nvl = new NameValueList();
                nvl.Add(new ListItem() { Name = "@ApplicationID", Value = applicatyionId });

                string xmlString = DBManager.Instance.ExecuteXmlQuery(_connSecurityDb, "spgDataDictionary", nvl);
                //Note: As different namespaces are specified for below classes, hence this repleace statement is required
                xmlString = xmlString.Replace("<dataDictionary ", "<dataDictionary xmlns=\"http://www.iringtools.org/library\"  ")
                    .Replace("<expressions>", "<expressions xmlns=\"http://www.iringtools.org/data/filter\">")
                    .Replace("<orderExpressions>", "<orderExpressions xmlns=\"http://www.iringtools.org/data/filter\">")
                    .Replace("<values>", "<values xmlns=\"http://www.iringtools.org/data/filter\">");
                dataDictionary = utility.Utility.Deserialize<DataDictionary>(xmlString, true);
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting  DataDictionary: " + ex);
            }
            return dataDictionary;
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
