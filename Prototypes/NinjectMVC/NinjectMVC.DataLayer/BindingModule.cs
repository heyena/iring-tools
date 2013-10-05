using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using Ninject.Modules;
using NinjectMVC.Library;

namespace NinjectMVC.DataLayer
{
  public class BindingModule : NinjectModule, INinjectModule
  {
    public override void Load()
    {
      Bind<IDataRepository>().To<DataRepository>();
      Bind<IDataLayer>().To<DataDataLayer>().Named(typeof(DataDataLayer).Name);
    }
  }
}
