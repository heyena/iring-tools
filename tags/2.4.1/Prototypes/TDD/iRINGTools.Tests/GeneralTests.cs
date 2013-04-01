using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Ninject;
using Ninject.Parameters;

using iRINGTools.Data;
using iRINGTools.Services;

namespace iRINGTools.Tests
{
  [TestClass]
  public class GeneralTests: TestBase
  {
    #region DataLayerService Test

    [TestMethod]
    public void DataLayerRepository_Has_5_DataLayerTypes_From_Service()
    {
      List<DataLayerItem> dataLayers = _dataLayerRepository.GetDataLayers().ToList();
      Assert.AreEqual(1, dataLayers.Count);
    }

    #endregion

    #region AdapterService Tests

    [TestMethod]
    public void AdapterRepository_Repository_Scopes_IsNotNull()
    {
      IAdapterRepository rep = new TestAdapterRepository();
      Assert.IsNotNull(rep.GetScopes());
    }

    [TestMethod]
    public void AdapterService_Can_Get_Scopes_From_Service()
    {
      IList<Scope> scopes = _adapterService.GetScopes();
      Assert.IsTrue(scopes.Count > 0);
    }

    [TestMethod]
    public void AdapterService_Has_5_Scopes_From_Service()
    {
      IList<Scope> scopes = _adapterService.GetScopes();
      Assert.AreEqual(5, scopes.Count);
    }

    [TestMethod]
    public void AdapterService_Has_25_Applications_From_Service()
    {
      IList<Application> applications = _adapterService.GetApplications();
      Assert.AreEqual(25, applications.Count);
    }

    #endregion

    #region Application Tests

    [TestMethod]
    public void Application_ShouldHave_Name_Description_Fields()
    {
      Application p = new Application { Name = "TestName", Description = "TestDescription" };

      Assert.AreEqual("TestName", p.Name);
      Assert.AreEqual("TestDescription", p.Description);
    }

    [TestMethod]
    public void Repository_Returns_Single_Application_When_Filtered_By_Name_Application1()
    {
      IList<Application> applications = _adapterRepository.GetApplications()
          .WithApplicationName("Application1")
          .ToList();

      Assert.AreEqual(1, applications.Count);
    }

    #endregion

    #region Scope Tests

    [TestMethod]
    public void Scope_ShouldHave_Name_Description_Fields()
    {
      Scope s = new Scope("TestName", "TestDescription");

      Assert.AreEqual("TestName", s.Name);
      Assert.AreEqual("TestDescription", s.Description);
    }

    [TestMethod]
    public void Repository_Returns_Single_Scope_When_Filtered_By_Name_Scope1()
    {
      List<Scope> scopes = _adapterRepository.GetScopes()
          .WithScopeName("Scope1")
          .ToList();

      Assert.AreEqual(1, scopes.Count);
    }

    #endregion
  }
}
