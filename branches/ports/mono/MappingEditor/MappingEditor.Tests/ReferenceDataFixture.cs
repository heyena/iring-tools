using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using org.ids_adi.iring.referenceData;

namespace IRingFramework.Tests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class ReferenceDataFixture
    {
        public ReferenceDataFixture(){}

        ReferenceDataProxy proxy = null;
        RefDataEntities results = null;

        [TestInitialize]
        public void InitTests()
        {
            proxy = new ReferenceDataProxy();
        }


        /// <summary>
        /// Determines whether this instance [can search for pump and retrieve data].
        /// </summary>
        [TestMethod]
        public void CanSearchForPumpAndRetrieveData()
        {
            results = proxy.Search("pump");

            // Assert pump data returned
            Assert.IsNotNull(results, "No values returned for Pump");
            Assert.IsNotNull(results.FirstOrDefault(r => r.Key == "ALKALINE PUMP"),
                "Could not find ALKALINE PUMP in list!");
            Assert.AreEqual("ReferenceData",
                results.FirstOrDefault(r => r.Key == "ALKALINE PUMP").Value.repository,
                "Value.repository value should be \"ReferenceData\"");

        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

    }
}
