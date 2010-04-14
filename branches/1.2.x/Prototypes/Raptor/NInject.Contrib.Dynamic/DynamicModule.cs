using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Ninject;
using System.Linq;
using System.Collections;
using System.Globalization;
using Ninject.Modules;

namespace Ninject.Contrib.Dynamic
{
  [XmlRoot]
  public class BindingConfiguration
  {
    public List<Binding> Bindings { get; set; }
  }

  [XmlRoot]
  public class Binding
  {
    [XmlAttribute]
    public string Name { get; set; }

    [XmlAttribute]
    public string Interface { get; set; }

    [XmlAttribute]
    public string Implementation { get; set; }
  }

  public class DynamicModule : NinjectModule
  {
    private const string Self = "Self";

    private readonly List<Binding> _bindings = new List<Binding>();

    public DynamicModule(BindingConfiguration configuration)
    {
      _bindings = configuration.Bindings;
    }

    public override void Load()
    {
      try
      {
        foreach (Binding binding in _bindings)
        {
          bool isToSelf = (binding.Implementation.ToUpper() == Self.ToUpper());
          bool isNamed = (binding.Name != String.Empty && binding.Name != null);
          Type interfaceType = Type.GetType(binding.Interface);

          if (isToSelf)
          { 
            if (isNamed)
            {
              Bind(interfaceType)
                .ToSelf()
                .Named(binding.Name);
            }
            else
            {
              Bind(interfaceType)
                .ToSelf();
            }
          }
          else
          {
            Type implementationType = Type.GetType(binding.Implementation);

            if (isNamed)
            {
              Bind(interfaceType)
                .To(implementationType)
                .Named(binding.Name);
            }
            else
            {
              Bind(interfaceType)
                .To(implementationType);
            }
          }
        }
      }
      catch (Exception ex)
      {
        throw new Exception(ex.Message);
      }
    }
  }
}
