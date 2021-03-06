﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject.Modules;
using org.iringtools.library;
using org.iringtools.adapter.projection;
using System.Collections.Specialized;

namespace org.iringtools.adapter
{
  public class AdapterModule : NinjectModule
  {
    public override void Load()
    {
      Bind<AdapterSettings>().ToSelf().InSingletonScope();
      Bind<IProjectionLayer>().To<DataProjectionEngine>().Named("xml");
      Bind<IProjectionLayer>().To<JsonProjectionEngine>().Named("json");
      Bind<IProjectionLayer>().To<DtoProjectionEngine>().Named("dto");
      Bind<IProjectionLayer>().To<RdfProjectionEngine>().Named("rdf");
      Bind<IProjectionLayer>().To<JsonLDProjectionEngine>().Named("jsonld");
    }
  }
}
