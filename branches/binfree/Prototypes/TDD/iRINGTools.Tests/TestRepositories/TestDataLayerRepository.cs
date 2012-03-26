using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using iRINGTools.Data;

namespace iRINGTools.Tests
{
  public class TestDataLayerRepository: IDataLayerRepository
  {
    private List<DataLayerItem> _dataLayerList;

    public TestDataLayerRepository()
    {
      _dataLayerList = new List<DataLayerItem>();
      _dataLayerList.Add(new DataLayerItem(typeof(TestDataLayer)) { IsDefault = true });
    }

    public IQueryable<DataLayerItem> GetDataLayers()
    {
      return _dataLayerList.AsQueryable();
    }
  }
}
