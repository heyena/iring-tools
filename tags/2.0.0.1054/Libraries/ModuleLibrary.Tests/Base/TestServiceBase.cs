using System.Collections.Generic;
using System.IO;
using System.Threading;
using MappingEditor.Base;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModuleLibrary.Tests.Mocks;
using PrismContrib.Base;

namespace ModuleLibrary.Tests.Base
{
    public class TestServiceBase
    {
        public object Data { get; set; }

        /// <summary>
        /// Sleeps for specified number of seconds as
        /// long as Data is null
        /// </summary>
        /// <param name="seconds">The seconds.</param>
        public void SleepForSeconds(int seconds)
        {
            for (int i = 0; i < seconds; i++)
                if (Data == null)
                    Thread.Sleep(1000);
        }

        /// <summary>
        /// Gets or sets the container.
        /// </summary>
        /// <value>The container.</value>
        public IUnityContainer Container { get; set; }

        /// <summary>
        /// Initializes the test.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <param name="testDir">The test dir.</param>
        public void InitializeTest(string port, string testDir)
        {
            // C:\org-ids-adi\camelot\Code\IRingFramework\IRingFramework.Web\IRingFramework.SilverlightTestPage.aspx
            int offset = testDir.IndexOf("TestResult");
            string webConfig = testDir.Substring(0, offset) + @"IRingFramework.Web\Web.Config";

            if (!File.Exists(webConfig))
                Assert.Fail("Could not locate Web.Config at " + webConfig);

            // Load all Web.Config AppSettings and ConnectionStrings
            string configString = webConfig.GetConfigString();

            // Load application specific variables
            IDictionary<string, string> configItems = new Dictionary<string, string>();
            configItems.Add("WebServerURL", string.Format("http://localhost:{0}", port));
            configItems.Add("WebSiteName", "IRingFramework.Web");


            // PrismContrib contains the bootstrapper and handles wireup
            Bootstrapper<MockPage> bootStrapper = new Bootstrapper<MockPage>();

            IDictionary<string, string> initParams = new Dictionary<string, string>();
            initParams.Add("InitParameters",
                string.Format("InitParameters={0}~{1}",
                    configString, configItems.GetConfigString()));

            bootStrapper.InitParams = initParams;

            // Configure application in ConfigureApp
            ConfigureApp configureApp = new ConfigureApp();

            // Wireup events prior to Run() 
            bootStrapper.OnConfigureContainer +=
                configureApp.OnConfigureContainer;

            bootStrapper.OnInitializeModules +=
                configureApp.OnInitializeModules;

            // Execute the bootStrapper.Run() launching application
            // as configured in ConfigureApp
            bootStrapper.Run();

            // For test usage
            Container = bootStrapper.Container;
        }

    }
}
