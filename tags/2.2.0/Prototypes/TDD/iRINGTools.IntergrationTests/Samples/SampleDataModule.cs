using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using iRINGTools.Data;

namespace iRINGTools.IntergrationTests
{
  public class SampleDataLayerModule : Ninject.Modules.NinjectModule
  {
    public override void Load()
    {
      Bind<IDataLayer>().ToProvider<DataLayerProvider<SampleDataLayer>>();
    }
  }
}
