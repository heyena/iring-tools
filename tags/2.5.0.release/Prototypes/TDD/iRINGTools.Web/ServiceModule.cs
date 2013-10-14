using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using iRINGTools.Data;
using iRINGTools.Services;
using iRINGTools.Tests;

namespace iRINGTools.Web
{
  public class ServiceModule : Ninject.Modules.NinjectModule
  {
    public override void Load()
    {
      this.Bind<IDataLayerRepository>().To<TestDataLayerRepository>();
      this.Bind<IAdapterRepository>().To<TestAdapterRepository>();
      this.Bind<IProjectionEngineRepository>().To<TestProjectionEngineRepository>();

      this.Bind<IAdapterService>().To<AdapterService>();
      this.Bind<IDataService>().To<DataService>();
    }
  }
}