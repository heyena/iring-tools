using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using iRINGTools.Data;

namespace iRINGTools.Services
{
  public class ReferenceDataService: IReferenceDataService
  {
    public IList<RDFRepository> GetRepositories()
    {
      throw new NotImplementedException();
    }

    public IList<SPARQLQuery> GetQueries()
    {
      throw new NotImplementedException();
    }

    public QMXF GetClass(string id, string @namespace)
    {
      throw new NotImplementedException();

      foreach (RDFRepository rdfRepository in GetRepositories())
      {
        //return rdfRepository.QuerySet.GetClass(string id);
      }
    }

    public QMXF GetTemplate(string id)
    {
      throw new NotImplementedException();
    }

    public Response PostClass(QMXF qmxf)
    {
      throw new NotImplementedException();
    }

    public Response PostTemplate(QMXF qmxf)
    {
      throw new NotImplementedException();
    }

    public IList<RDFEntity> GetAllSuperClasses(string id)
    {
      throw new NotImplementedException();
    }

    public RDFEntity GetClassLabel(string id)
    {
      throw new NotImplementedException();
    }

    public IList<RDFEntity> GetClassMembers(string id)
    {
      throw new NotImplementedException();
    }

    public IList<RDFEntity> GetClassTemplates(string id)
    {
      throw new NotImplementedException();
    }

    public IList<RDFEntity> GetClassTemplatesCount(string id)
    {
      throw new NotImplementedException();
    }

    public IList<RDFEntity> GetSubClasses(string id)
    {
      throw new NotImplementedException();
    }

    public IList<RDFEntity> GetSubClassesCount(string id)
    {
      throw new NotImplementedException();
    }

    public IList<RDFEntity> GetSuperClasses(string id)
    {
      throw new NotImplementedException();
    }

    public IList<RDFEntity> Search(string query)
    {
      throw new NotImplementedException();
    }

    public IList<RDFEntity> Search(string query, int index, int pageSize)
    {
      throw new NotImplementedException();
    }
  }
}
