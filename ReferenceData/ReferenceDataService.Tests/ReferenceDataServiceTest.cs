using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using org.ids_adi.iring.referenceData;
using org.ids_adi.qmxf;
using org.iringtools.library;
using System.IO;

namespace ReferenceDataService.Tests
{
  /// <summary>
  /// Summary description for ReferenceDataServiceTest
  /// </summary>
  [TestClass]
  public class ReferenceDataServiceTest
  {
    public ReferenceDataServiceTest()
    {
      string path = @"C:\iring-tools\ReferenceData\ReferenceDataService\XML\";

      if (!File.Exists(path + "Repositories.xml"))
      {
        File.Copy(path + "Repositories_Test.xml", path + "Repositories.xml");
      }
    }

    [TestMethod()]
    public void GetRepositoriesTest()
    {
      ReferenceDataProxy target = new ReferenceDataProxy();
      List<Repository> actual = target.GetRepositories();
      Assert.AreEqual(4, actual.Count);
    }

    [TestMethod()]
    public void SearchTest()
    {
      ReferenceDataProxy target = new ReferenceDataProxy();
      RefDataEntities actual = target.Search("^vortex");
      Assert.AreEqual(20, actual.Count);
    }

    [TestMethod()]
    public void SearchResetTest()
    {
      ReferenceDataProxy target = new ReferenceDataProxy();

      RefDataEntities initial = target.Search("^vortex");

      //TODO: We need to fix RefDataService to enable SemWeb TripleStore.
      //QMXF @class = new QMXF();

      //ClassDefinition classDefinition = new ClassDefinition()
      //{
      //  identifier = "TestClassVortex",
      //  name = new List<QMXFName>() 
      //  {
      //    new QMXFName { lang = "en-us", value = "Vortex Test Class" },
      //  },
      //};

      //@class.classDefinitions.Add(classDefinition);

      //Response response = target.PostClass(@class);

      //Assert.AreNotEqual(response.Level, StatusLevel.Error);

      RefDataEntities actual = target.SearchReset("^vortex");

      Assert.AreEqual(initial.Count, actual.Count);
    }

    [TestMethod()]
    public void SearchPageTest()
    {
      ReferenceDataProxy target = new ReferenceDataProxy();
      RefDataEntities actual = target.SearchPage("valve", "15");
      Assert.AreEqual(100, actual.Count);
    }

    [TestMethod()]
    public void SearchPageResetTest()
    {
      ReferenceDataProxy target = new ReferenceDataProxy();

      RefDataEntities initial = target.SearchPage("valve", "15");

      //TODO: We need to fix RefDataService to enable SemWeb TripleStore.
      //QMXF @class = new QMXF();

      //ClassDefinition classDefinition = new ClassDefinition()
      //{
      //  identifier = "TestClassValve",
      //  name = new List<QMXFName>() 
      //  {
      //    new QMXFName { lang = "en-us", value = "Test Class Valve" },
      //  },
      //};

      //@class.classDefinitions.Add(classDefinition);

      //Response response = target.PostClass(@class);

      //Assert.AreNotEqual(response.Level, StatusLevel.Error);

      RefDataEntities actual = target.SearchPageReset("valve", "15");

      Assert.AreEqual(initial.Count, actual.Count);
    }

    [TestMethod()]
    public void FindTest()
    {
      ReferenceDataProxy target = new ReferenceDataProxy();
      List<Entity> actual = target.Find("transmitter");
      Assert.AreEqual(2, actual.Count);
    }

    [TestMethod()]
    public void GetClassTest()
    {
      //Transmitter
      ReferenceDataProxy target = new ReferenceDataProxy();
      QMXF actual = target.GetClass("R19535665699");
      Assert.AreEqual(1, actual.classDefinitions.Count);
    }

    [TestMethod()]
    public void GetClassLabelTest()
    {
      //Transmitter
      ReferenceDataProxy target = new ReferenceDataProxy();
      string actual = target.GetClassLabel("R19535665699");
      Assert.AreNotEqual(string.Empty, actual);
    }

    [TestMethod()]
    public void GetSuperClassesTest()
    {
      //Transmitter - 1
      ReferenceDataProxy target = new ReferenceDataProxy();
      List<Entity> actual = target.GetSuperClasses("R19535665699");
      Assert.AreEqual(1, actual.Count);
    }

    [TestMethod()]
    public void GetSubClassesTest()
    {
      //Transmitter - 19
      ReferenceDataProxy target = new ReferenceDataProxy();
      List<Entity> actual = target.GetSubClasses("R19535665699");
      Assert.AreEqual(19, actual.Count);
    }

    [TestMethod()]
    public void GetClassTemplatesTest()
    {
      //ISO 15926-4 POSSIBLE INDIVIDUAL - > 0
      ReferenceDataProxy target = new ReferenceDataProxy();
      List<Entity> actual = target.GetClassTemplates("R99781532089");
      Assert.IsTrue(actual.Count > 0);
    }

    [TestMethod()]
    public void GetTemplateTest()
    {
      //ClassifiedIdentification & PipingNetworkSystemHasSegment
      ReferenceDataProxy target = new ReferenceDataProxy();
      QMXF actual = target.GetTemplate("R30193386273");
      Assert.AreEqual(1, actual.templateDefinitions.Count);

      actual = target.GetTemplate("R55260901367");
      Assert.AreEqual(1, actual.templateQualifications.Count);
    }

    //TODO: We need to fix RefDataService to enable SemWeb TripleStore.
    //[TestMethod()]
    //public void PostTemplateTest()
    //{
    //  Assert.Inconclusive();
    //}

    //TODO: We need to fix RefDataService to enable SemWeb TripleStore.
    //[TestMethod()]
    //public void PostClassTest()
    //{
    //  Assert.Inconclusive();
    //} 

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

  }
}
