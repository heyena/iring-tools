using System;
using System.Text;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using org.iringtools.adapter;
using org.iringtools.library;
using org.iringtools.utility;
using org.iringtools.sql;
using org.iringtools.adapter.datalayer;
using Ninject;
using Ninject.Modules;

namespace DatabaseLibrary.Tests
{
  /// <summary>
  /// Summary description for UnitTest1
  /// </summary>
  [TestClass]
  public class SqlTests
  {
    private SqlDataLayer _dataLayer;

    public SqlTests()
    {
      NameValueCollection settings = new NameValueCollection();
      settings["ProjectName"] = "12345_000";
      settings["ApplicationName"] = "ORACLE";
      settings["XmlPath"] = @"C:\Projects\iring-tools\trunk\Prototypes\SqlLibrary\SqlLibrary.Tests\";

      AdapterSettings adapterSettings = new AdapterSettings();
      adapterSettings.AppendSettings(settings);

      SqlSettings SqlSettings = new SqlSettings();
      SqlSettings.AppendSettings(settings);

      _dataLayer = new SqlDataLayer(adapterSettings);
    }

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

    //[TestMethod]
    public void CreateConfiguration()
    {
      Configuration settings = new Configuration();
      settings.Connection = @"Data Source=MMSTST;User Id=hatch_sys;Password=manager;";
      settings.ConfigObjects = new List<ConfigObject>();            
            
      ConfigObject configObject = new ConfigObject();
      configObject.Name = "Valves";
      configObject.Identifier = "IDENT";

      ConfigCommand selectCmd = new ConfigCommand();      
      selectCmd.Query = @"select * from vw_iring_valves where rownum < 10";
      configObject.Select = selectCmd;

      ConfigCommand updateCmd = new ConfigCommand();
      updateCmd.Query = @"update vw_iring_valves set tag_number = @tag_number where ident = @identifier";
      updateCmd.Parameters = new List<ConfigParameter>();
      updateCmd.Parameters.Add(new ConfigParameter { Name = "@identifier", DataType = DataType.Int32 });
      updateCmd.Parameters.Add(new ConfigParameter { Name = "@tag_number", DataType = DataType.String });
      configObject.Update = updateCmd;
            
      ConfigCommand deleteCmd = new ConfigCommand();      
      deleteCmd.Query = @"delete from vw_iring_valves where ident = @identifier";
      deleteCmd.Parameters = new List<ConfigParameter>();
      deleteCmd.Parameters.Add(new ConfigParameter { Name = "@identifier", DataType = DataType.Int32 });
      configObject.Delete = deleteCmd;

      ConfigCommand insertCmd = new ConfigCommand();
      insertCmd.Query = @"insert into vw_iring_valves (ident, tag_number) values (@identifier, @tag_number)";
      insertCmd.Parameters = new List<ConfigParameter>();
      insertCmd.Parameters.Add(new ConfigParameter { Name = "@identifier", DataType = DataType.Int32 });
      insertCmd.Parameters.Add(new ConfigParameter { Name = "@tag_number", DataType = DataType.String });
      configObject.Insert = insertCmd;

      settings.ConfigObjects.Add(configObject);

      Utility.Write<Configuration>(settings, @"C:\Projects\iring-tools\trunk\Prototypes\SqlLibrary\SqlLibrary.Tests\sql-configuration.12345_000.ORACLE.xml", true);
    }

    [TestMethod]
    public void GetDictionary()
    {
      DataDictionary dictionary = _dataLayer.GetDictionary();

      Assert.IsNotNull(dictionary);

      Utility.Write<DataDictionary>(dictionary, @"C:\Projects\iring-tools\trunk\Prototypes\SqlLibrary\SqlLibrary.Tests\DataDictionary.xml", true);
    }

    //[TestMethod]
    public void Read()
    {      
      IList<string> identifiers = new List<string>() 
      { 
        "3534479",
        "3534480",
        "3534481",
        "3534482",
        "3534483",
        "3534484",
        "3534485",
        "3534486",
        "3534684",
        "3534685"
      };

      IList<IDataObject> dataObjects = _dataLayer.Get("Valves", identifiers);

      if (!(dataObjects.Count() > 0))
      {
        throw new AssertFailedException("No Rows returned.");
      }

      Assert.IsTrue(dataObjects.Count() == 10); 
      
      /*
      foreach (IDataObject dataObject in dataObjects)
      {
        Assert.IsNotNull(dataObject.GetPropertyValue("PumpType"));
        Assert.IsNotNull(dataObject.GetPropertyValue("PumpDriverType"));
        Assert.IsNotNull(dataObject.GetPropertyValue("DesignTemp"));
        Assert.IsNotNull(dataObject.GetPropertyValue("DesignPressure"));
        Assert.IsNotNull(dataObject.GetPropertyValue("Capacity"));
        Assert.IsNotNull(dataObject.GetPropertyValue("SpecificGravity"));
        Assert.IsNotNull(dataObject.GetPropertyValue("DifferentialPressure"));
      }*/
    }
  }
} 
