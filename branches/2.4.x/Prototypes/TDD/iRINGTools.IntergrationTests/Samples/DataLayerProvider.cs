using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using Ninject.Parameters;


using iRINGTools.Data;

namespace iRINGTools.IntergrationTests
{
  public class DataLayerProvider<T> : Ninject.Activation.Provider<T>
  {
    protected override T CreateInstance(Ninject.Activation.IContext context)
    {
      Application application = context.Kernel.Get<Application>();

      var datalayer = System.Activator.CreateInstance<T>();

      return datalayer;
    }
  }
}
