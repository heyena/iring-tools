using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using NUnit.Framework;
using org.iringtools.adapter;
using org.iringtools.library;
using org.iringtools.utility;
using StaticDust.Configuration;
using Ninject;
using Ninject.Extensions.Xml;

namespace iRINGTools.SDK.SP3DDataLayer
{
    [TestFixture]
    public class SP3DDataLayerTest
    {
        private string _baseDirectory = string.Empty;
        private IKernel _kernel = null;
        private NameValueCollection _settings;
        private AdapterSettings _adapterSettings;
        private IDataLayer2 _sp3dDataLayer;

        public SP3DDataLayerTest()
        {
            _settings = new NameValueCollection();

            _settings["XmlPath"] = @".\12345_000\";
            _settings["ProjectName"] = "12345_000";
            _settings["ApplicationName"] = "SP3D";

            _baseDirectory = Directory.GetCurrentDirectory();
            _baseDirectory = _baseDirectory.Substring(0, _baseDirectory.LastIndexOf("\\bin"));
            _settings["BaseDirectoryPath"] = _baseDirectory;
            Directory.SetCurrentDirectory(_baseDirectory);

            _adapterSettings = new AdapterSettings();
            _adapterSettings.AppendSettings(_settings);

            string appSettingsPath = String.Format("{0}12345_000.SPP3D.config",
                _adapterSettings["XmlPath"]
            );

            if (File.Exists(appSettingsPath))
            {
                AppSettingsReader appSettings = new AppSettingsReader(appSettingsPath);
                _adapterSettings.AppendSettings(appSettings);
            }

            var ninjectSettings = new NinjectSettings { LoadExtensions = false };
            _kernel = new StandardKernel(ninjectSettings);

            _kernel.Load(new XmlExtensionModule());

            string relativePath = String.Format(@"{0}BindingConfiguration.{1}.{2}.xml",
            _settings["XmlPath"],
            _settings["ProjectName"],
            _settings["ApplicationName"]
          );

            //Ninject Extension requires fully qualified path.
            string bindingConfigurationPath = Path.Combine(
              _settings["BaseDirectoryPath"],
              relativePath
            );

            _kernel.Load(bindingConfigurationPath);

            _sp3dDataLayer = _kernel.Get<IDataLayer2>();
        }

       // [Test]
        public void Create()
        {
        }

    }
}
