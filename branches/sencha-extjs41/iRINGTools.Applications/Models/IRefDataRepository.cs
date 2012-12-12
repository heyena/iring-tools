using org.ids_adi.qmxf;
using org.iringtools.refdata;
using org.iringtools.refdata.federation;
using org.iringtools.refdata.response;


namespace org.iringtools.web.Models
{
  public interface IRefDataRepository
  {
    Federation GetFederation();

    RefdataResponse Search(string query);

    RefdataResponse Search(string query, int start, int limit);

    RefdataResponse SearchReset(string query);

    Entity GetClassLabel(string classId);

    Entities GetSuperClasses(string classId, Repository repository);

    Entities GetSubClasses(string classId, Repository repository);

    Entities GetSubClassesCount(string classId);

    Entities GetClassTemplatesCount(string classId);

    Entities GetClassTemplates(string classId);

    RefdataResponse GetClassMembers(string classId, Repository repository);

    QMXF GetClasses(string classId, Repository repository);

    QMXF GetTemplate(string id);
  }
}