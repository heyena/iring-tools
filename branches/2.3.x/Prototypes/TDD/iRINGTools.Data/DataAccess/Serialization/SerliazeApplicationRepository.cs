using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace iRINGTools.Data.Serliaze
{
  public class SerliazeApplicationRepository : IApplicationRepository
  {
    private List<IScope> _scopeList;
    private List<IApplication> _applicationList;
    private string _fileName;
    private int nextScopeId = 1;
    private int nextApplicationId = 1;

    public SerliazeApplicationRepository(string uri)
    {
      _scopeList = new List<IScope>();
      _applicationList = new List<IApplication>();

      _fileName = uri;

      if (File.Exists(_fileName))
      {
        var scopes = Utility.Read<ScopeProjects>(uri, true);

        foreach (var scopeItem in scopes)
        {
          var scope = new Scope
          {
            Id = nextScopeId++
            ,Name = scopeItem.Name
            ,Description = scopeItem.Description
          };

          _scopeList.Add(scope);

          foreach (var applicationItem in scopeItem.Applications)
          {
            var application = new Application
            {
              Id = nextApplicationId++
              ,Name = applicationItem.Name
              ,Description = applicationItem.Description

              ,Scope = scopeItem.Name
              ,DataLayerName = applicationItem.DataLayerName
              ,DictionaryName = string.Format("DataDictionary.{0}.{1}.xml", scopeItem.Name, applicationItem.Name)
              ,MappingName = string.Format("Mapping.{0}.{1}.xml",scopeItem.Name, applicationItem.Name)
              
            };

            _applicationList.Add(application);
          }
        }
      }
    }

    #region Application Repository

    public IQueryable<IScope> GetScopes()
    {
      throw new NotImplementedException();
    }

    public IQueryable<IApplication> GetApplications()
    {
      throw new NotImplementedException();
    }

    public IScope SaveScope(IScope scope)
    {
      throw new NotImplementedException();
    }

    public IScope RemoveScope(IScope scope)
    {
      throw new NotImplementedException();
    }

    public IApplication SaveApplication(IApplication application)
    {
      throw new NotImplementedException();
    }

    public IApplication RemoveApplication(IApplication application)
    {
      throw new NotImplementedException();
    }

    public void SaveChanges()
    {
      throw new NotImplementedException();
    }

    #endregion
  }
}
