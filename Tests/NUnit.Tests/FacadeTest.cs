using org.iringtools.adapter;
using org.iringtools.exchange;
using org.iringtools.library;
using org.iringtools.utility;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Xml;
using System.Linq;
using System.Xml.Linq;
using System;
using System.Configuration;
using NUnit.Framework;
using System.Net;
namespace NUnit.Tests
{
    [TestFixture]
    class FacadeTest
    {
        private AdapterProvider _facadeProvider = null;
        private AdapterSettings _settings = null;
        private ExchangeProvider _exchangeProvider = null;
        private string _baseDirectory = string.Empty;

        public FacadeTest()
        {
            _settings = new AdapterSettings();
            _settings.AppendSettings(ConfigurationManager.AppSettings);

            //_settings["BaseDirectoryPath"] = @"E:\iring-tools\branches\2.0.x\Tests\NUnit.Tests";
            _settings["ProjectName"] = "12345_000";
            _settings["ApplicationName"] = "ABC";
            _settings["GraphName"] = "Lines";

            _baseDirectory = Directory.GetCurrentDirectory();
            _baseDirectory = _baseDirectory.Substring(0, _baseDirectory.LastIndexOf("\\Bin"));
            _settings["BaseDirectoryPath"] = _baseDirectory;
            Directory.SetCurrentDirectory(_baseDirectory);


            _facadeProvider = new AdapterProvider(_settings);
            _exchangeProvider = new ExchangeProvider(_settings);
        }
        [Test]
        public void FacadeRefresh()
        {
            Response updatedFacade = _facadeProvider.RefreshAll("12345_000", "ABC");
            Assert.AreNotEqual(0, updatedFacade.StatusList.Count);
        }

        [Test]
        public void Pull()
        {
       
            Request _newRequest = new Request();

            WebCredentials targetCredentials = new WebCredentials();
            string targetCredentialsXML = Utility.Serialize<WebCredentials>(targetCredentials, true);

            _newRequest.Add("targetUri", @"http://localhost:65432/InterfaceService/query");
            _newRequest.Add("targetCredentials", targetCredentialsXML);
            _newRequest.Add("targetGraphBaseUri", @"http://localhost:65432/AdapterService/12345_000/ABC/Lines");
            _newRequest.Add("graphName", "Lines");
            _newRequest.Add("filter", "");
            _newRequest.Add("ProxyHost", "172.21.161.170");
            _newRequest.Add("ProxyPort", "8080");
            _newRequest.Add("networkCredential", "");



            Response getData = _exchangeProvider.Pull("12345_000", "DEF", "Lines", _newRequest);
            Assert.AreNotEqual(0, getData.StatusList.Count);

        }
    }
}

