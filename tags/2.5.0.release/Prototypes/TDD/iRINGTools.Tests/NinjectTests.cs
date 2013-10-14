using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Ninject;

using iRINGTools.Data;
using iRINGTools.Services;
using System.Xml.Linq;

namespace iRINGTools.Tests
{
  [TestClass]
  public class NinjectTests
  {
    private IKernel _kernel;

    [TestInitialize]
    public void Startup()
    {
      _kernel = new StandardKernel();

      _kernel.Bind<IDataLayerRepository>().To<TestDataLayerRepository>();
      _kernel.Bind<IAdapterRepository>().To<TestAdapterRepository>();
      _kernel.Bind<IProjectionEngineRepository>().To<TestProjectionEngineRepository>();

      _kernel.Bind<IAdapterService>().To<AdapterService>();
      _kernel.Bind<IDataService>().To<DataService>();
    }

    [TestMethod]
    public void AdapterService_GetApplication()
    {
      var service = _kernel.Get<IAdapterService>();

      Application application = service.GetApplication("Scope1", "Application1");
      Mapping mapping = application.Mapping;

      //string x = Utility.Serialize<Mapping>(mapping, false);



      //_kernel.Bind<Application>().ToConstant(application);
      //var data = kernel.Get<IDataService>();
    }

    [TestMethod]
    public void DataService_ToXML()
    {
      var service = _kernel.Get<IDataService>();

      XDocument xml = service.GetDataProjection("Scope1", "Application1", "Lines", null, "ProjectionEngine1", 0, 100, true);

      //_kernel.Bind<Application>().ToConstant(application);
      //var data = kernel.Get<IDataService>();
    }
  }
}
