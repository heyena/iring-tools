using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject.Modules;
using org.iringtools.library;
using System.Collections.Specialized;

namespace org.iringtools.nhibernate
{
  class NHibernateModule : NinjectModule
  {
    public override void Load()
    {
      Bind<NHibernateSettings>().ToSelf().InSingletonScope();
    }
  }
}
