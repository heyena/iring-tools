using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using iRINGTools.Data;
using iRINGTools.Services;

using Ninject;

namespace iRINGTools.IntergrationTests
{
  [TestClass]
  public class IntergrationTests
  {
    //[TestMethod]
    //public void AdapterService_XMLMappingRepository_IsNotNull() 
    //{
    //  string searchPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\XML\");

    //  IMappingRepository repository = new XMLMappingRepository(searchPath, "Mapping.*", ".xml");
    //  Assert.IsNotNull(repository.GetMappings());
    //}

    //[TestMethod]
    //public void AdapterService_XMLApplicationRepository_Save_To_New_File_Then_Read_Existing_File()
    //{
    //  XMLApplicationRepository repository;
    //  ApplicationService service;

    //  string uri = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\XML\Scopes.xml");
    //  if (File.Exists(uri))
    //  {
    //    File.Delete(uri);
    //  };

    //  repository = new XMLApplicationRepository(uri);
    //  service = new AdapterService(repository, null, null, null);

    //  var scopeId = service.SaveScope(new Scope { Name = "SaveName", Description = "SaveDescription" });
    //  var appId = service.SaveApplication(new Application { Name = "SaveName", Description = "SaveDescription" });

    //  service.SaveChanges();

    //  Assert.AreEqual(1, service.GetScopes().Count);
    //  Assert.AreEqual(1, service.GetApplications().Count);

    //  repository = new XMLApplicationRepository(uri);
    //  service = new AdapterService(repository, null, null, null);

    //  var scope = service.GetScope("SaveName");

    //  Assert.IsNotNull(scope, "Failed to fetch Scope with id equals to 1");
    //  Assert.AreEqual("SaveName", scope.Name);
    //  Assert.AreEqual("SaveDescription", scope.Description);

    //  var application = service.GetApplication("SaveName");

    //  Assert.IsNotNull(application, "Failed to fetch Application with id equals to 1");
    //  Assert.AreEqual("SaveName", application.Name);
    //  Assert.AreEqual("SaveDescription", application.Description);

    //}

    //[TestMethod]
    //public void IDataLayerTypeRepository_ReflectionDataLayerTypeRepository_Has_Atleast_1_DataLayer()
    //{
    //  Ninject.IKernel kernel = new Ninject.StandardKernel();
    //  kernel.Load(new SampleDataLayerModule());

    //  IScope scope = new Scope("ScopeName", "ScopeDescription");
    //  Application application = new Application("AppName", "AppDescription", "ScopeName", "DataLayerName", "DictionaryName", "MappingName");

    //  IDataLayer dataLayer = kernel.Get<IDataLayer>(new Ninject.Parameters.Parameter("application", application, true));
    //  Assert.IsNotNull(dataLayer, "Failed to get SampleDataLayer");

    //  kernel.Bind<IDataLayerTypeRepository>().To<DataLayerTypeRepository>();

    //  IDataLayerTypeRepository repository = kernel.Get<IDataLayerTypeRepository>();
    //  Assert.IsNotNull(repository, "Failed to get DataLayerRepository");
    //}

    //[TestMethod]
    //public void DataLayerTypeRepository()
    //{
      



    //  DataLayerTypeRepository repository = new DataLayerTypeRepository(AppDomain.CurrentDomain.GetAssemblies());

    //  Assert.IsNotNull(repository.GetDataLayerTypes());

    //}
  }
}
