using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iRINGTools.Data
{
  public static class ApplicationFilters
  {
    /// <summary>
    /// Filters The query by ScopeId
    /// </summary>
    public static IQueryable<Scope> WithScopeId(this IQueryable<Scope> qry, int id)
    {
      return qry.Where(i => i.Id == id);
    }

    /// <summary>
    /// Filters The query by Scope Name
    /// </summary>
    public static IQueryable<Scope> WithScopeName(this IQueryable<Scope> qry, string name)
    {
      return qry.Where(i => i.Name == name);
    }

    public static IQueryable<Application> ForScopeId(this IQueryable<Application> qry, int scopeId)
    {
      return qry.Where(i => i.Scope != null && i.Scope.Id == scopeId);
    }

    /// <summary>
    /// Filters The query by ApplicationId
    /// </summary>
    public static IQueryable<Application> WithApplicationId(this IQueryable<Application> qry, int id)
    {
      return qry.Where(i => i.Id == id);
    }

    /// <summary>
    /// Filters The query by ApplicationName
    /// </summary>
    public static IQueryable<Application> WithApplicationName(this IQueryable<Application> qry, string name)
    {
      return qry.Where(i => i.Name == name);
    }

    /// <summary>
    /// Filters The query by DataLayerGuid
    /// </summary>
    public static IQueryable<DataLayerItem> WithDataLayerGuid(this IQueryable<DataLayerItem> qry, Guid guid)
    {
      return qry.Where(i => i.Guid == guid);
    }

    /// <summary>
    /// Filters The query by DataLayerName
    /// </summary>
    public static IQueryable<DataLayerItem> WithDataLayerName(this IQueryable<DataLayerItem> qry, string name)
    {
      return qry.Where(i => i.Name == name);
    }

    /// <summary>
    /// Filters an IList of DataLayer and returns the default DataLayer
    /// </summary>
    public static DataLayerItem DefaultDataLayer(this IEnumerable<DataLayerItem> list)
    {
      return (from s in list
              where s.IsDefault
              select s).SingleOrDefault();
    }

    /// <summary>
    /// Filters The query by ConfigurationId
    /// </summary>
    public static IQueryable<Configuration> WithConfigurationId(this IQueryable<Configuration> qry, int id)
    {
      return qry.Where(i => i.Id == id);
    }

    /// <summary>
    /// Filters The query by ConfigurationName
    /// </summary>
    public static IQueryable<Configuration> WithConfigurationName(this IQueryable<Configuration> qry, string name)
    {
      return qry.Where(i => i.Name == name);
    }

    /// <summary>
    /// Filters The query by DictionaryId
    /// </summary>
    public static IQueryable<Dictionary> WithDictionaryId(this IQueryable<Dictionary> qry, int id)
    {
      return qry.Where(i => i.Id == id);
    }

    /// <summary>
    /// Filters The query by DictionaryName
    /// </summary>
    public static IQueryable<Dictionary> WithDictionaryName(this IQueryable<Dictionary> qry, string name)
    {
      return qry.Where(i => i.Name == name);
    }

    /// <summary>
    /// Filters The query by MappingId
    /// </summary>
    public static IQueryable<Mapping> WithMappingId(this IQueryable<Mapping> qry, int id)
    {
      return qry.Where(i => i.Id == id);
    }

    /// <summary>
    /// Filters The query by MappingName
    /// </summary>
    public static IQueryable<Mapping> WithMappingName(this IQueryable<Mapping> qry, string name)
    {
      return qry.Where(i => i.Name == name);
    }

    /// <summary>
    /// Filters The query by ProjectionEngineId
    /// </summary>
    public static IQueryable<IProjectionEngine> WithProjectionEngineId(this IQueryable<IProjectionEngine> qry, int id)
    {
      return qry.Where(i => i.Id == id);
    }

    /// <summary>
    /// Filters The query by ProjectionEngineFormat
    /// </summary>
    public static IQueryable<IProjectionEngine> WithProjectionEngineFormat(this IQueryable<IProjectionEngine> qry, string format)
    {
      return qry.Where(i => i.Format == format);
    }

    /// <summary>
    /// Filters an IList of Scope and returns a Scope by name
    /// </summary>
    public static Scope DefaultScope(this IEnumerable<Scope> list)
    {
      return (from s in list
              where s.IsDefault
              select s).SingleOrDefault();
    }

    /// <summary>
    /// Filters an IList of Application and returns a Application by name
    /// </summary>
    public static Application DefaultApplication(this IEnumerable<Application> list)
    {
      return (from s in list
              where s.IsDefault
              select s).SingleOrDefault();
    }

  }
}
