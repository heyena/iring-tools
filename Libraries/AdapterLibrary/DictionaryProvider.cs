using System;
using log4net;
using System.Collections.Specialized;
using Ninject;
using org.iringtools.library;
using System.Web;
using org.iringtools.utility;
using System.Xml.Linq;
using System.Data.Linq;
using System.IO;


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
                //_siteID = Convert.ToInt32(settings["SiteId"]);
            }
            catch (Exception e)
            {
                _logger.Error("Error initializing adapter provider: " + e.Message);
            }
        }



        public org.iringtools.library.DatabaseDictionary GetDictionary(Guid applicationId)
        {
            org.iringtools.library.DatabaseDictionary dataDictionary = new org.iringtools.library.DatabaseDictionary();
            try
            {
                NameValueList nvl = new NameValueList();
                nvl.Add(new ListItem() { Name = "@ApplicationID", Value = applicationId });

                string xmlString = DBManager.Instance.ExecuteXmlQuery(_connSecurityDb, "spgDictionary", nvl)
                    .Replace("expressions xmlns=\"http://www.iringtools.org/library\"", "expressions xmlns=\"http://www.iringtools.org/data/filter\"")
                        .Replace("expression xmlns=\"http://www.iringtools.org/library\"", "expression xmlns=\"http://www.iringtools.org/data/filter\"")
                            .Replace("values xmlns=\"http://www.iringtools.org/library\"", "values xmlns=\"http://www.iringtools.org/data/filter\"")
                    .Replace("orderExpressions xmlns=\"http://www.iringtools.org/library\"", "orderExpressions xmlns=\"http://www.iringtools.org/data/filter\"")
                        .Replace("orderExpression xmlns=\"http://www.iringtools.org/library\"", "orderExpression xmlns=\"http://www.iringtools.org/data/filter\"")
                    .Replace("rollupExpressions xmlns=\"http://www.iringtools.org/library\"", "rollupExpressions xmlns=\"http://www.iringtools.org/data/filter\"")
                        .Replace("rollupExpression xmlns=\"http://www.iringtools.org/library\"", "rollupExpression xmlns=\"http://www.iringtools.org/data/filter\"")
                            .Replace("rollup xmlns=\"http://www.iringtools.org/library\"", "rollup xmlns=\"http://www.iringtools.org/data/filter\"")

                    .Replace("isAdmin xmlns=\"http://www.iringtools.org/library\"", "isAdmin xmlns=\"http://www.iringtools.org/data/filter\"")
                    .Replace("dataFilterId xmlns=\"http://www.iringtools.org/library\"", "dataFilterId xmlns=\"http://www.iringtools.org/data/filter\"")
                    .Replace("resourceId xmlns=\"http://www.iringtools.org/library\"", "resourceId xmlns=\"http://www.iringtools.org/data/filter\"")
                    .Replace("dataFilterTypeId xmlns=\"http://www.iringtools.org/library\"", "dataFilterTypeId xmlns=\"http://www.iringtools.org/data/filter\"")
                    .Replace("active xmlns=\"http://www.iringtools.org/library\"", "active xmlns=\"http://www.iringtools.org/data/filter\"")
                    ;



                dataDictionary = utility.Utility.Deserialize<org.iringtools.library.DatabaseDictionary>(xmlString, true);
            }
            catch (Exception ex)
            {
                _logger.Error("Error getting  dictionary: " + ex);
            }
            return dataDictionary;
        }

        public Response InsertDictionary(XDocument xml)
        {
            Response response = new Response();
            response.Messages = new Messages();

            try
            {
                string rawXml = xml.ToString().Replace("xmlns=", "xmlns1=");//this is done, because in stored procedure it causes problem

                using (var dc = new DataContext(_connSecurityDb))
                {
                    NameValueList nvl = new NameValueList();
                    nvl.Add(new ListItem() { Name = "@rawXml", Value = rawXml });

                    string output = DBManager.Instance.ExecuteScalarStoredProcedure(_connSecurityDb, "spiDictionary", nvl);

                    switch (output)
                    {
                        case "1":
                            PrepareSuccessResponse(response, "dictionaryadded");
                            break;
                        default:
                            PrepareErrorResponse(response, output);
                            break;
                    }

                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error adding dictionary: " + ex);

                Status status = new Status { Level = StatusLevel.Error };
                status.Messages = new Messages { ex.Message };

                response.DateTimeStamp = DateTime.Now;
                response.Level = StatusLevel.Error;
                response.StatusList.Add(status);
            }

            return response;
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
            Status status = new Status { Level = StatusLevel.Success };
            status.Messages = new Messages { errMsg };
            response.DateTimeStamp = DateTime.Now;
            response.Level = StatusLevel.Success;
            response.StatusList.Add(status);
        }
        private void PrepareWarningResponse(Response response, string errMsg)
        {
            Status status = new Status { Level = StatusLevel.Warning };
            status.Messages = new Messages { errMsg };
            response.DateTimeStamp = DateTime.Now;
            response.Level = StatusLevel.Warning;
            response.StatusList.Add(status);
        }
        #endregion

    }


}
