using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject.Modules;
using org.iringtools.adapter.rules;
using org.iringtools.adapter.projection;
using org.iringtools.library;

namespace org.iringtools.adapter
{
  class AdapterModule : NinjectModule
  {
    public override void Load()
    {
      Bind<AdapterSettings>().ToSelf().InSingletonScope();
      Bind<ApplicationSettings>().ToSelf().InThreadScope();
      Bind<IProjectionEngine>().To<SemWebEngine>().InThreadScope().Named("SemWeb");
      Bind<IProjectionEngine>().To<SPARQLEngine>().InThreadScope().Named("Sparql");
    }
  }
}
