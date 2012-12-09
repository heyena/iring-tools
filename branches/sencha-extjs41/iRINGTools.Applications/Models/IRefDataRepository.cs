using org.ids_adi.qmxf;
using org.iringtools.refdata;
using org.iringtools.refdata.federation;
using org.iringtools.refdata.response;
using Entities = org.iringtools.refdata.response.Entities;
using Entity = org.iringtools.refdata.response.Entity;
using Response = org.iringtools.refdata.response.Response;

namespace org.iringtools.web.Models
{
  public interface IRefDataRepository
  {
    Federation GetFederation();

    Response Search(string query);

    Response Search(string query, int start, int limit);

    Response SearchReset(string query);

    Entity GetClassLabel(string classId);

    Entities GetSuperClasses(string classId, Repository repository);

    Entities GetSubClasses(string classId, Repository repository);

    Entities GetSubClassesCount(string classId);

    Entities GetClassTemplatesCount(string classId);

    Entities GetClassTemplates(string classId);

    Entities GetClassMembers(string classId, Repository repository);

    QMXF GetClasses(string classId, Repository repository);

    QMXF GetTemplate(string id);
  }
}