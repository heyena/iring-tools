using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using org.iringtools.refdata;
using org.ids_adi.qmxf;
using org.iringtools.library;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting.Web;
using System.Configuration;
using System.Collections.Specialized;

namespace ReferenceDataService.Tests
{
  /// <summary>
  /// Summary description for ReferenceDataServiceTest
  /// </summary>
  [TestClass]
  public class ReferenceDataServiceTest
  {
    private ReferenceDataSettings _settings = null;

    public ReferenceDataServiceTest()
    {
      _settings = new ReferenceDataSettings();
      _settings.AppendSettings(ConfigurationManager.AppSettings);

      Directory.SetCurrentDirectory(_settings["BaseDirectoryPath"]);

      if (!File.Exists(_settings["XmlPath"] + "Repositories.xml"))
      {
          File.Copy(
              _settings["XmlPath"] + "Repositories_Development.xml", 
              _settings["XmlPath"] + "Repositories.xml");
      }
    }

    [TestMethod()]
    public void GetRepositoriesTest()
    {
      ReferenceDataProxy target = new ReferenceDataProxy();
      Repositories actual = target.GetRepositories();
      Assert.AreNotEqual(0, actual.Count);
    }

    [TestMethod()]
    public void SearchTest()
    {
      ReferenceDataProxy target = new ReferenceDataProxy();
      RefDataEntities actual = target.Search("^vortex");
      Assert.AreEqual(8, actual.Entities.Count);
    }

    [TestMethod()]
    public void SearchPageTest()
    {
      ReferenceDataProxy target = new ReferenceDataProxy();
      RefDataEntities actual = target.SearchPage("valve", 1400, 100);
      Assert.AreEqual(74, actual.Entities.Count);
    }

    [TestMethod()]
    public void FindTest()
    {
      ReferenceDataProxy target = new ReferenceDataProxy();
      List<Entity> actual = target.Find("transmitter");
      Assert.AreEqual(1, actual.Count);
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
  }
}
