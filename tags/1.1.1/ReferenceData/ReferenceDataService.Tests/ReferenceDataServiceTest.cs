using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using org.ids_adi.iring.referenceData;
using org.ids_adi.qmxf;
using org.iringtools.library;

namespace ReferenceDataService.Tests
{
  /// <summary>
  /// Summary description for ReferenceDataServiceTest
  /// </summary>
  [TestClass]
  public class ReferenceDataServiceTest
  {
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
      RefDataEntities actual = target.Search("in-line piping");
      Assert.AreEqual(8, actual.Count);
    }

    [TestMethod()]
    public void SearchResetTest()
    {
      ReferenceDataProxy target = new ReferenceDataProxy();
      RefDataEntities actual = target.SearchReset("in-line piping");
      Assert.AreEqual(8, actual.Count);
    }

    [TestMethod()]
    public void SearchPageTest()
    {
      ReferenceDataProxy target = new ReferenceDataProxy();
      RefDataEntities actual = target.SearchPage("in-line piping", "1");
      Assert.AreEqual(8, actual.Count);
    }

    [TestMethod()]
    public void SearchPageResetTest()
    {
      ReferenceDataProxy target = new ReferenceDataProxy();
      RefDataEntities actual = target.SearchPageReset("in-line piping", "1");
      Assert.AreEqual(8, actual.Count);
    }

    [TestMethod()]
    public void FindTest()
    {
      ReferenceDataProxy target = new ReferenceDataProxy();
      List<Entity> actual = target.Find("in-line piping component");
      Assert.AreEqual(1, actual.Count);
    }

    [TestMethod()]
    public void GetClassTest()
    {
      ReferenceDataProxy target = new ReferenceDataProxy();
      QMXF actual = target.GetClass("R45162754880");
      Assert.AreEqual(1, actual.classDefinitions.Count);
    }

    [TestMethod()]
    public void GetClassLabelTest()
    {
      ReferenceDataProxy target = new ReferenceDataProxy();
      string actual = target.GetClassLabel("R45162754880");
      Assert.AreNotEqual(string.Empty, actual);
    }

    [TestMethod()]
    public void GetSuperClassesTest()
    {
      ReferenceDataProxy target = new ReferenceDataProxy();
      List<Entity> actual = target.GetSuperClasses("R45162754880");
      Assert.AreEqual(1, actual.Count);
    }

    [TestMethod()]
    public void GetSubClassesTest()
    {
      ReferenceDataProxy target = new ReferenceDataProxy();
      List<Entity> actual = target.GetSubClasses("R45162754880");
      Assert.AreEqual(0, actual.Count);
    }

    [TestMethod()]
    public void GetClassTemplatesTest()
    {
      ReferenceDataProxy target = new ReferenceDataProxy();
      List<Entity> actual = target.GetClassTemplates("R45162754880");
      Assert.AreEqual(9, actual.Count);
    }

    [TestMethod()]
    public void GetTemplateTest()
    {
      ReferenceDataProxy target = new ReferenceDataProxy();
      QMXF actual = target.GetTemplate("R41551704022");
      Assert.AreEqual(1, actual.templateQualifications.Count);
    }

    //[TestMethod()]
    //public void PostTemplateTest()
    //{
    //  Assert.Inconclusive();
    //}

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
