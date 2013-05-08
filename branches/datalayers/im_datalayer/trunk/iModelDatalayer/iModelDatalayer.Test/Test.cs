using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using org.iringtools.adapter;
using org.iringtools.library;
using org.iringtools.utility;
using System.Data;
using System.Text;
using org.iringtools.adapter.datalayer;
using log4net;
using NUnit.Framework;
using StaticDust.Configuration;
using System.Runtime.Serialization;

namespace org.iringtools.adapter.datalayer.test
{
     [TestFixture]
    public class Test
    {
          private static readonly ILog _logger = LogManager.GetLogger(typeof(Test));
        private iModelDatalayer _dataLayer = null;

        public Test()
        {
            string baseDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(baseDir.Substring(0, baseDir.LastIndexOf("\\bin")));

            AdapterSettings adapterSettings = new AdapterSettings();
            adapterSettings.AppendSettings(new AppSettingsReader("App.config"));

            string iModelConfigFile = String.Format("{0}{1}.{2}.config",
                  adapterSettings["AppDataPath"],
                  adapterSettings["ProjectName"],
                  adapterSettings["ApplicationName"]
                  );

            AppSettingsReader twSettings = new AppSettingsReader(iModelConfigFile);
            adapterSettings.AppendSettings(twSettings);


            _dataLayer = new iModelDatalayer(adapterSettings);
        }

        [Test]
        public void TestDictionary()
        {          
          
            DatabaseDictionary _dictionaryTest = _dataLayer.GetDatabaseDictionary();
            
            Assert.IsNotNull(_dictionaryTest);
         
        }
    }
}
