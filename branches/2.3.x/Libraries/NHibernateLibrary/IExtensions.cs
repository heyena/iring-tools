using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.library;

namespace org.iringtools.nhibernate
{
  public interface IAuthorization
  {
    DataFilter Authorize(DataFilter dataFilter);
  }

  public interface ISummary
  {
    IList<Object> GetSummary();
  }
}
