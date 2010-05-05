using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace org.iringtools.adapter
{
  public static class KnownTypeProvider
  {

    private static HashSet<Type> knownTypes = new HashSet<Type>();



    public static void ClearAllKnownTypes()
    {

      knownTypes = new HashSet<Type>();

    }



    public static void Register<T>()
    {

      Register(typeof(T));

    }



    public static void Register(Type type)
    {

      knownTypes.Add(type);

    }



    public static void RegisterDerivedTypesOf<T>(Assembly assembly)
    {

      RegisterDerivedTypesOf(typeof(T), assembly);

    }



    public static void RegisterDerivedTypesOf<T>(IEnumerable<Type> types)
    {

      RegisterDerivedTypesOf(typeof(T), types);

    }



    public static void RegisterDerivedTypesOf(Type type, Assembly assembly)
    {

      RegisterDerivedTypesOf(type, assembly.GetTypes());

    }



    public static void RegisterDerivedTypesOf(Type type, IEnumerable<Type> types)
    {

      knownTypes.UnionWith(GetDerivedTypesOf(type, types));

    }



    public static IEnumerable<Type> GetKnownTypes(ICustomAttributeProvider provider)
    {

      return knownTypes;

    }



    private static List<Type> GetDerivedTypesOf(Type baseType, IEnumerable<Type> types)
    {

      return types.Where(t => !t.IsAbstract && t.IsSubclassOf(baseType)).ToList();

    }

  }
}
