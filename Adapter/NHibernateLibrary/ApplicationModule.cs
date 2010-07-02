using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject.Modules;
using org.iringtools.library;
using System.Collections.Specialized;

namespace org.iringtools.application
{
  class ApplicationModule : NinjectModule
  {
    public override void Load()
    {
      Bind<ApplicationSettings>().ToSelf().InSingletonScope();
    }
  }
}
