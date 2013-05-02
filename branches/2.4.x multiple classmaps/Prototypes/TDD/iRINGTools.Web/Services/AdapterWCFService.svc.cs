using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;

using iRINGTools.Data;
using iRINGTools.Services;
using iRINGTools.Tests;

namespace iRINGTools.Web.Services
{
  [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
  [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
  public class AdapterWCFService : IAdapterWCFService
  {
    private IAdapterService _adapterService;

    public AdapterWCFService()
    {
      IDataLayerRepository dataLayerRepository = new TestDataLayerRepository();
      IAdapterRepository adapterRepository = new TestAdapterRepository(dataLayerRepository);

      _adapterService = new AdapterService(adapterRepository);
    }

    public AdapterWCFService(IAdapterService adapterService)
    {
      _adapterService = adapterService;
    }

    public void DoWork()
    {
    }
  }
}
