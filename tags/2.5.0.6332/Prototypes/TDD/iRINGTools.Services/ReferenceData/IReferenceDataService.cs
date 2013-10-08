using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using iRINGTools.Data;

namespace iRINGTools.Services
{
  public interface IReferenceDataService
  {
    IList<RDFRepository> GetRepositories();
    IList<SPARQLQuery> GetQueries();

    QMXF GetClass(string id, string @namespace);
    QMXF GetTemplate(string id);
    Response PostClass(QMXF qmxf);
    Response PostTemplate(QMXF qmxf);
    IList<RDFEntity> GetAllSuperClasses(string id);
    RDFEntity GetClassLabel(string id);
    IList<RDFEntity> GetClassMembers(string id);
    IList<RDFEntity> GetClassTemplates(string id);
    IList<RDFEntity> GetClassTemplatesCount(string id);
    IList<RDFEntity> GetSubClasses(string id);
    IList<RDFEntity> GetSubClassesCount(string id);
    IList<RDFEntity> GetSuperClasses(string id);

    IList<RDFEntity> Search(string query);
    IList<RDFEntity> Search(string query, int index, int pageSize);
  }
}
