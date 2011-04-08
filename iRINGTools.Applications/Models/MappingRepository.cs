﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Xml.Linq;
using System.Web;
using Ninject;


using org.iringtools.library;
using org.iringtools.utility;
using org.iringtools.mapping;

namespace iRINGTools.Web.Models
{
    public class MappingRepository : IMappingRepository
    {

        private NameValueCollection _settings = null;
        private WebHttpClient _client = null;
        private string _refDataServiceURI = string.Empty;

        [Inject]
        public MappingRepository()
        {
            _settings = ConfigurationManager.AppSettings;
            _client = new WebHttpClient(_settings["AdapterServiceUri"]);
        }

        public Mapping GetMapping(string scopeName, string applicationName)
        {
            Mapping obj = null;

            try
            {
                obj = _client.Get<Mapping>(String.Format("/{0}/{1}/mapping", scopeName, applicationName), true);
            }
            catch (Exception ex)
            {
            }

            return obj;
        }

        public Mapping UpdateMapping(string scopeName, string applicationName)
        {
            Mapping obj = null;
            try
            {
            }
            catch (Exception ex)
            {
            }
            return obj;
        }
    }
}