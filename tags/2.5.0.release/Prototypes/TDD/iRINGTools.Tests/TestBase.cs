using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using iRINGTools.Data;
using iRINGTools.Services;

namespace iRINGTools.Tests
{
  [TestClass]
  public class TestBase
  {
    protected IAdapterRepository _adapterRepository;
    protected IDataLayerRepository _dataLayerRepository;
    protected IProjectionEngineRepository _projectionEngineRepository;

    protected IAdapterService _adapterService;
    protected IDataService _dataService;

    [TestInitialize]
    public void Startup()
    {
      _dataLayerRepository = new TestDataLayerRepository();
      _adapterRepository = new TestAdapterRepository(_dataLayerRepository);
      _adapterService = new AdapterService(_adapterRepository);

      _projectionEngineRepository = new TestProjectionEngineRepository();
      _dataService = new DataService(_projectionEngineRepository, _adapterService);
    }
  }
}
