using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iRINGTools.Data.DataAccess.Interfaces
{
  public interface IReferenceDataRepository
  {
    IQueryable<RDFRepository> GetRepositories();
    IQueryable<SPARQLQuery> GetQueries();
  }
}
