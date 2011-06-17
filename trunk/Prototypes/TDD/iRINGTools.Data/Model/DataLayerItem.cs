using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace iRINGTools.Data
{
  [Serializable]
  public class DataLayerItem
  {
    public string Name { get; set; }
    public Type DataLayerType { get; set; }
    public bool IsDefault { get; set; }

    public DataLayerItem(string name, Type dataLayer)
      : this(dataLayer)
    {
      this.Name = name;
    }

    public DataLayerItem(Type dataLayer)
    {
      DataLayerType = dataLayer;

      if (DataLayerType == null)
        throw new InvalidOperationException("DataLayer cannot be null");

      IDataLayer instance = (IDataLayer)Activator.CreateInstance(DataLayerType);
      Name = instance.Name;
    }

    public Guid Guid
    {
      get
      {
        return DataLayerType != null ? DataLayerType.GUID : Guid.Empty;
      }
    }

    public IDataLayer CreateDataLayer(Configuration configuration)
    {
      //IDataLayer dataLayer = (IDataLayer)Activator.CreateInstance(DataLayerType, new object[] { configuration });

      ConstructorInfo ctor = DataLayerType.GetConstructor(new[] { typeof(Configuration) });
      object instance = ctor.Invoke(new object[] { configuration });
      IDataLayer dataLayer = (IDataLayer)instance;

      return dataLayer;
    }
  }
}
