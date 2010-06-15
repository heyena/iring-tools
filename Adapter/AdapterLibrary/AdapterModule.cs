using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject.Modules;
using org.iringtools.adapter.semantic;
using org.iringtools.library;
using org.iringtools.adapter.projection;

namespace org.iringtools.adapter
{
  class AdapterModule : NinjectModule
  {
    public override void Load()
    {
      Bind<AdapterSettings>().ToSelf().InSingletonScope();
      Bind<ApplicationSettings>().ToSelf().InThreadScope();
      Bind<IProjectionLayer>().To<RdfProjectionEngine>().Named("rdf");
      Bind<IProjectionLayer>().To<QtxfProjectionEngine>().Named("qtxf");
      Bind<IProjectionLayer>().To<DtoProjectionEngine>().Named("dto");
      Bind<IProjectionLayer>().To<XmlProjectionEngine>().Named("xml");
      //Bind<IProjectionLayer>().To<SparqlResultsProjectionEngine>().Named("sparql");
    }
  }
}
