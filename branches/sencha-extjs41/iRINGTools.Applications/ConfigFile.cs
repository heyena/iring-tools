using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Configuration;
using System.Xml;
using log4net;

namespace org.iringtools.web
{
    public static class ConfigFile
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ConfigFile));
    
        public const string AppSettings = "appSettings";


        public static string GetConfigString(this string filePath)
        {
            var configList = filePath.GetSection(ConfigFile.AppSettings);

            return configList.GetConfigString();
        }

        public static string GetConfigString(this IDictionary<string, string> configCol)
        {
            var retVal = new StringBuilder();
            foreach (var record in configCol)
            {
                if (retVal.Length > 0)
                    retVal.Append("~");
                retVal.AppendFormat("{0}#{1}", record.Key, record.Value);
            }
            return retVal.ToString();
        }

        public static IDictionary<string, string> GetSection(this string fullFilePath, string section)
        {
            return GetSection(fullFilePath, section, new Dictionary<string, string>());
        }

        public static IDictionary<string, string> GetSection(this string fullFilePath, string section, IDictionary<string, string> appendList)
        {
            IDictionary<string, string> retList = new Dictionary<string, string>();
            string filePath = fullFilePath;
            var xmlDoc = new System.Xml.XmlDocument();
            var dataCol = new NameValueCollection();
            try
            {
                var fileMap = new ExeConfigurationFileMap();
                fileMap.ExeConfigFilename = filePath;
                var config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
                var xmlString = config.GetSection(section).SectionInformation.GetRawXml();
                xmlDoc.LoadXml(xmlString);

                var nodeList = xmlDoc.ChildNodes[0];
                foreach (System.Xml.XmlNode node in nodeList)
                    retList.Add(node.Attributes[0].Value, node.Attributes[1].Value);
            }
            catch (Exception ex) {
                _logger.Error("Error in GetSection: " + ex);
            }

            // If an append list is provided then add it to the list we are returning
            if (appendList != null && appendList.Count > 0)
            {
                foreach (var record in appendList)
                    retList.Add(record.Key, record.Value);
            }
            return retList;
        }

        public static string GetSection(this string fullFilePath, string section, string key)
        {
            var filePath = fullFilePath;
            var xmlDoc = new System.Xml.XmlDocument();
            var dataCol = new NameValueCollection();
            try
            {
                var fileMap = new ExeConfigurationFileMap();
                fileMap.ExeConfigFilename = filePath;
                var config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
                var xmlString = config.GetSection(section).SectionInformation.GetRawXml();
                xmlDoc.LoadXml(xmlString);

                var nodeList = xmlDoc.ChildNodes[0];
                foreach (var node in nodeList.Cast<XmlNode>().Where(node => node.Attributes[0].Value.ToLower() == key.ToLower()))
                    return node.Attributes[1].Value;
            }
            catch (Exception ex){
                _logger.Error("Error in GetSection: " + ex);
            }

            return "";
        }
    }
}

