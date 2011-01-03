﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject.Modules;
using org.iringtools.adapter.semantic;
using org.iringtools.library;
using org.iringtools.adapter.projection;
using System.Collections.Specialized;
using org.iringtools.adapter.identity;

namespace org.iringtools.adapter
{
  class AdapterModule : NinjectModule
  {
    public override void Load()
    {
      Bind<AdapterSettings>().ToSelf().InSingletonScope();
      Bind<ISemanticLayer>().To<dotNetRDFEngine>().Named("dotNetRDF");
      Bind<IProjectionLayer>().To<RdfProjectionEngine>().Named("rdf");
      Bind<IProjectionLayer>().To<DtoProjectionEngine>().Named("dto");
      Bind<IProjectionLayer>().To<XmlProjectionEngine>().Named("xml");
      Bind<IProjectionLayer>().To<DataProjectionEngine>().Named("data");
      Bind<IIdentityLayer>().To<WindowsAutheticationProvider>().Named("IdentityLayer");
      Bind<IIdentityLayer>().To<STSProvider>().Named("IdentityLayer");
    }
  }
}
