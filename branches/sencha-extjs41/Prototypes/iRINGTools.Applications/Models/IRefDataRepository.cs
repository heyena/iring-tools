using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using org.iringtools.library;
using org.ids_adi.qmxf;

namespace iRINGTools.Web.Models
{
  public interface IRefDataRepository
  {

    RefDataEntities Search(string query);

    RefDataEntities Search(string query, int start, int limit);

    RefDataEntities SearchReset(string query);

    Entity GetClassLabel(string classId);

    Entities GetSuperClasses(string classId);

    Entities GetSubClasses(string classId);

    Entities GetSubClassesCount(string classId);

    Entities GetClassTemplatesCount(string classId);

    Entities GetClassTemplates(string classId);

    Entities GetClassMembers(string classId);

    QMXF GetClasses(string classId);

    QMXF GetTemplate(string id);
  }
}