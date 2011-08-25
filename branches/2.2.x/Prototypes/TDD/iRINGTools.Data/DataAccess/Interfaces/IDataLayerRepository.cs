using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iRINGTools.Data
{
  public interface IDataLayerRepository
  {
    IQueryable<DataLayerItem> GetDataLayers();
  }
}
