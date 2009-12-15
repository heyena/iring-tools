using System;
using System.Reflection;
using System.ServiceModel;
using System.Web.Compilation;
using System.ServiceModel.Activation;

namespace org.iringtools.adapter
{
  class AdapterServiceHostFactory : ServiceHostFactory
  {
    protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
    {
      foreach (Assembly assembly in BuildManager.CodeAssemblies)
      {
        KnownTypeProvider.RegisterDerivedTypesOf<org.iringtools.adapter.DataTransferObject>(assembly);
      }

      return new ServiceHost(serviceType, baseAddresses);
    }
  }
}
