using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Reflection;

namespace iRINGTools.Data
{
  public class Application
  {
    private string _graphBaseUri;

    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsDefault { get; set; }

    public Scope Scope { get; set; }
    public Configuration Configuration { get; set; }
    public Dictionary Dictionary { get; set; }
    public Mapping Mapping { get; set; }
    public DataLayerItem DataLayerItem { get; set; }

    public Application()
    {
    }

    public Application(string name, string description)
    {
      Name = name;
      Description = description;
    }

    public string GraphBaseUri
    {
      get
      {
        if (String.IsNullOrEmpty(_graphBaseUri))
        {
          if (OperationContext.Current != null)
          {
            string baseAddress = OperationContext.Current.Host.BaseAddresses[0].ToString();

            if (!baseAddress.EndsWith("/"))
              baseAddress = baseAddress + "/";

            return baseAddress;
          }
          else
          {
            return @"http://localhost:54321/data/";
          }
        }
        else
        {
          return _graphBaseUri;
        }
      }
      set
      {
        _graphBaseUri = value;
      }
    }

    #region object overrides
    public override bool Equals(object obj)
    {
      if (obj is Application)
      {
        Application compareTo = (Application)obj;
        return compareTo.Id == this.Id;
      }
      else
      {
        return base.Equals(obj);
      }
    }

    public override string ToString()
    {
      return this.Name;
    }
    public override int GetHashCode()
    {
      return this.Id.GetHashCode();
    }
    #endregion

  }
}
