
using org.iringtools.library;

namespace iRINGTools.Web.Models
{
  public interface IGridRepository
  {
		//Grid getGrid(string scope, string app, string graph, string filter, string sort, string dir, string start, string limit);

    DataDictionary GetDictionary(string relativeUrl);

    DataItems GetDataItems(string app, string scope, string graph, DataFilter dataFilter, int start, int limit);
  }
}

