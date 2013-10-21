using System.Text;
using System.IO;
using org.iringtools.adapter;
using org.iringtools.library;
using NUnit.Framework;
using log4net;
using StaticDust.Configuration;
using org.iringtools.adapter.datalayer;
using System;
using System.Linq;

namespace LWDLSampleCode.Test
{       
    public class Test
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Test));
        private ILightweightDataLayer _dataLayer;
        private string _objectType;
        private DataObject _objectDefinition;
        private string _projectName = string.Empty;
        private string _xmlPath = string.Empty;
        private string _applicationName = string.Empty;
        private string _baseDirectory = string.Empty;
        private string _appConfigXML;
        AdapterSettings _settings;
       
        
        public Test()
        {
            string baseDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(baseDir.Substring(0, baseDir.LastIndexOf("\\bin")));
            _settings = new AdapterSettings();
            _xmlPath = _settings["xmlPath"];
            _appConfigXML = String.Format("{0}{1}.{2}.config", _xmlPath, "TestNDDL", "LWDLSample");

            AppSettingsReader datalayerSettings = new AppSettingsReader(_appConfigXML);
            _settings.AppendSettings(datalayerSettings);
            _projectName = _settings["ProjectName"];
            _applicationName = _settings["ApplicationName"];
           

            _dataLayer = new org.iringtools.adapter.datalayer.LWDLSampleCode(_settings);

        }

        [Test]
        public void TestCreateDictionary()
        {
            DataFilter df = new DataFilter();
           Assert.IsNotNull(_dataLayer.Dictionary(true, "1", out df));
        }

         [Test]
        public void TestGet()
        {   
             DataFilter df = new DataFilter();
             DataDictionary _dictionary = _dataLayer.Dictionary(true, "1", out df);
             DataObject dataObject = _dictionary.dataObjects.Where<DataObject>(p => p.objectName == _settings["ObjectName"]).FirstOrDefault();
             Assert.AreNotEqual( (_dataLayer.Get(dataObject)).Count,0);
            
        }

    }
}
