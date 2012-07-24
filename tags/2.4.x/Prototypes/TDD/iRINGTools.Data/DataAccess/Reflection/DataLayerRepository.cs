using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using Ninject;

namespace iRINGTools.Data
{
  public class DataLayerRepository
  {
    private IList<DataLayerItem> _dataLayerTypes;
    private readonly Type REPOSITORY_TYPE = typeof(IDataLayer);

    public DataLayerRepository(IList<Assembly> assemblies)
    {
      _dataLayerTypes = new List<DataLayerItem>();

      foreach (var assembly in assemblies)
      {
        foreach (var type in assembly.GetTypes())
        {
          if (!type.IsInterface && !type.IsAbstract && REPOSITORY_TYPE.IsAssignableFrom(type))
          {
            _dataLayerTypes.Add(new DataLayerItem(type));
          }
        }
      }
    }

    public DataLayerRepository(IList<DataLayerItem> dataLayerTypes)
    {
      _dataLayerTypes = dataLayerTypes;
    }

    public IQueryable<DataLayerItem> GetDataLayerTypes()
    {
      return _dataLayerTypes.AsQueryable();
    }
  }
}
