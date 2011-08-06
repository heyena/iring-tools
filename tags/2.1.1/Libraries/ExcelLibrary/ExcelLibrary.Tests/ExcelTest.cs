using System;
using System.Text;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Specialized;
using org.iringtools.adapter;
using org.iringtools.library;
using org.iringtools.utility;
using org.iringtools.excel;

namespace org.iringtools.adapter.datalayer
{
    
  /// <summary>
  /// Summary description for UnitTest1
  /// </summary>
  [TestClass]
  public class ExcelTest
  {    

    private ExcelDataLayer _dataLayer;
    
    public ExcelTest()
    {
      NameValueCollection settings = new NameValueCollection();      
      settings["ProjectName"] = "12345_000";
      settings["ApplicationName"] = "EXCEL";
      settings["XmlPath"] = @"C:\Projects\iring-tools\trunk\Libraries\ExcelLibrary\ExcelLibrary.Tests\";
      
      AdapterSettings adapterSettings = new AdapterSettings();
      adapterSettings.AppendSettings(settings);

      ExcelSettings ExcelSettings = new ExcelSettings();
      ExcelSettings.AppendSettings(settings);

      _dataLayer = new ExcelDataLayer(adapterSettings);
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
    
    //[TestMethod]
    public void Create()
    {
      IList<string> identifiers = new List<string>() 
      { 
        "Equip-001", 
        "Equip-002", 
        "Equip-003", 
        "Equip-004" 
      };

      Random random = new Random();

      IList<IDataObject> dataObjects = _dataLayer.Create("Equipment", identifiers);
      foreach (IDataObject dataObject in dataObjects)
      {
        dataObject.SetPropertyValue("Description", "DESC-" + random.Next(2, 10));
        dataObject.SetPropertyValue("PumpType", "PT-" + random.Next(2, 10));
        dataObject.SetPropertyValue("PumpDriverType", "PDT-" + random.Next(2, 10));
        dataObject.SetPropertyValue("DesignTemp", (double)random.Next(2, 10));
        dataObject.SetPropertyValue("DesignPressure", (double)random.Next(2, 10));
        dataObject.SetPropertyValue("Capacity", (double)random.Next(2, 10));
        dataObject.SetPropertyValue("SpecificGravity", (double)random.Next(2, 10));
        dataObject.SetPropertyValue("DifferentialPressure", (double)random.Next(2, 10));
      }
      Response actual = _dataLayer.Post(dataObjects);

      if (actual.Level != StatusLevel.Success)
      {
        throw new AssertFailedException(Utility.SerializeDataContract<Response>(actual));
      }

      Assert.IsTrue(actual.Level == StatusLevel.Success);
    }

    //[TestMethod]
    public void Read()
    {
      IList<string> identifiers = new List<string>() 
      { 
        "Equip-001", 
        "Equip-002", 
        "Equip-003", 
        "Equip-004" 
      };

      IList<IDataObject> dataObjects = _dataLayer.Get("Equipment", identifiers);

      if (!(dataObjects.Count() > 0))
      {
        throw new AssertFailedException("No Rows returned.");
      }

      foreach (IDataObject dataObject in dataObjects)
      {
        Assert.IsNotNull(dataObject.GetPropertyValue("PumpType"));
        Assert.IsNotNull(dataObject.GetPropertyValue("PumpDriverType"));
        Assert.IsNotNull(dataObject.GetPropertyValue("DesignTemp"));
        Assert.IsNotNull(dataObject.GetPropertyValue("DesignPressure"));
        Assert.IsNotNull(dataObject.GetPropertyValue("Capacity"));
        Assert.IsNotNull(dataObject.GetPropertyValue("SpecificGravity"));
        Assert.IsNotNull(dataObject.GetPropertyValue("DifferentialPressure"));
      }
    }

    //[TestMethod]
    public void Delete()
    {
      IList<string> identifiers = new List<string>() 
      {         
        "Equip-002",         
        "Equip-004" 
      };

      Response response =_dataLayer.Delete("Equipment", identifiers);

      if (!(response.StatusList.Count < 2))
      {
        throw new AssertFailedException("No Rows Delete.");
      }

      Assert.IsTrue(response.StatusList.Count == 2);
    }

    [TestMethod]
    public void GetDictionary()
    {
      DataDictionary dictionary = _dataLayer.GetDictionary();

      Assert.IsNotNull(dictionary);

      Utility.Write<DataDictionary>(dictionary, @"C:\Projects\iring-tools\trunk\Libraries\ExcelLibrary\ExcelLibrary.Tests\DataDictionary.xml", true);
    }

    //[TestMethod]
    public void CreateConfiguration()
    {
      ExcelConfiguration settings = new ExcelConfiguration();
      settings.Location = @"C:\Projects\iring-tools\trunk\Libraries\ExcelLibrary\ExcelLibrary.Tests\excel-Source.12345_000.EXCEL.xlsx";
      settings.Worksheets = new List<ExcelWorksheet>();

      ExcelWorksheet worksheet = new ExcelWorksheet();
      worksheet.Name = "Equipment";
      worksheet.Identifier = "Tag";
      worksheet.Columns = new List<ExcelColumn>();

      worksheet.Columns.Add(new ExcelColumn { Name = "Tag", DataType = DataType.String, Index = 1 });
      worksheet.Columns.Add(new ExcelColumn { Name = "Description", DataType = DataType.String, Index = 2 });
      worksheet.Columns.Add(new ExcelColumn { Name = "PumpType", DataType = DataType.String, Index = 3 });
      worksheet.Columns.Add(new ExcelColumn { Name = "PumpDriverType", DataType = DataType.String, Index = 4 });
      worksheet.Columns.Add(new ExcelColumn { Name = "DesignTemp", DataType = DataType.String, Index = 5 });
      worksheet.Columns.Add(new ExcelColumn { Name = "DesignPressure", DataType = DataType.String, Index = 6 });
      worksheet.Columns.Add(new ExcelColumn { Name = "Capacity", DataType = DataType.String, Index = 7 });
      worksheet.Columns.Add(new ExcelColumn { Name = "SpecificGravity", DataType = DataType.String, Index = 8 });
      worksheet.Columns.Add(new ExcelColumn { Name = "DifferentialPressure", DataType = DataType.String, Index = 9 });

      settings.Worksheets.Add(worksheet);

      Utility.Write<ExcelConfiguration>(settings, @"C:\Projects\iring-tools\trunk\Libraries\ExcelLibrary\ExcelLibrary.Tests\excel-configuration.12345_000.EXCEL.xml", true);
    }    


  }
}
