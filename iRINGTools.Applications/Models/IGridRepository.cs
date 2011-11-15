using System.Collections.Generic;
using System.Linq;
using System.Web;

using org.iringtools.library;
using org.iringtools.mapping;
using iRINGTools.Web.Helpers;

namespace iRINGTools.Web.Models
{
  public interface IGridRepository
  {
		Grid getGrid(string scope, string app, string graph, string filter, string sort, string dir, string start, string limit);
		string getResponse();
  }
}