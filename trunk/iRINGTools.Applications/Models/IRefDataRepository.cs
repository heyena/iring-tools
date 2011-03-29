using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using org.iringtools.library;

namespace iRINGTools.Web.Models
{
  public interface IRefDataRepository
  {

    RefDataEntities Search(string query);

    RefDataEntities Search(string query, string start, string limit);

    RefDataEntities SearchReset(string query);

    Entity GetClassLabel(string classId);

    Entities GetSuperClasses(string classId);

    Entities GetSubClasses(string classId);

    Entities GetClassTemplates(string classId);
  }
}