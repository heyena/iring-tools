using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Ninject;

namespace org.iringtools.adapter.datalayer
{
  public class SqlModule : Ninject.Modules.NinjectModule
  {
    public override void Load()
    {
      Bind<System.Data.IDbConnection>().ToMethod(x => new System.Data.SqlClient.SqlConnection());
      Bind<System.Data.IDbCommand>().ToMethod(x => new System.Data.SqlClient.SqlCommand());
      Bind<System.Data.IDbDataParameter>().ToMethod(x => new System.Data.SqlClient.SqlParameter());
    }
  }

  public class ServiceLocator
  {
    private static IKernel _kernel;

    public static void LoadModules(IKernel kernel)
    {
      kernel.Load(new SqlModule());
    }

    public static IKernel Kernel
    {
      get
      {
        if (_kernel == null)
        {
          _kernel = new Ninject.StandardKernel();
          LoadModules(_kernel);
        }

        return _kernel;
      }
    }

    public static T Get<T>()
    {
      return Kernel.Get<T>();
    }

  }
}
