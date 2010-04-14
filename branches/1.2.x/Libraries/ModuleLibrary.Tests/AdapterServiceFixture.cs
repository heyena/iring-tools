using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModuleLibrary.Tests.Base;
using System.Linq;

using org.ids_adi.iring;
using ModuleLibrary.Events;

namespace ModuleLibrary.Tests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class AdapterServiceFixture : TestServiceBase
    {

        public AdapterServiceFixture() { }

        [TestInitialize]
        public void InitTests()
        {
            // Initialize with port and TestContext directory
          InitializeTest("51382", TestContext.TestDir);
        }

        #region Generated Code
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
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
        #endregion

        [TestMethod]
        public void CanGetDataDictionaryFromIRingFrameworkWebService()
        {
            IAdapter dal = Container.Resolve<IAdapter>("AdapterProxyDAL");
            dal.OnDataArrived += new System.EventHandler<System.EventArgs>(OnDataArrivedHandler);

            DataDictionary dictionary = dal.GetDictionary();
            SleepForSeconds(5);

            Assert.IsNotNull(Data);
            DataDictionary dataDictionary = (DataDictionary) Data;
            Assert.IsTrue(dataDictionary.dataObjects.Count > 1);

            DataObject data = dataDictionary.dataObjects.FirstOrDefault(d => d.objectName == "KOPot");
            Assert.IsNotNull(data);
            Assert.IsNotNull(data.dataProperties.FirstOrDefault(t => t.propertyName == "tag"));
            
        }

        /// <summary>
        /// Called when [data arrived handler].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void OnDataArrivedHandler(object sender, System.EventArgs e)
        {
            Data = ((CompletedEventArgs)e).Data;
        }
    }
}
