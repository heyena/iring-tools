using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject.Modules;
using ClassLibrary;

namespace TestConsole
{
  public class TestModule : NinjectModule
  {
    public override void Load()
    {
      Bind<GlobalSettings>().ToSelf().InSingletonScope();
      Bind<Projection>().ToSelf().InRequestScope();
      Bind<IDataLayer>().To<BarDataLayer>().Named("bar");
    }
  }
}
