
using org.iringtools.library;

namespace iRINGTools.Web.Models
{
  public interface IGridRepository
  {
    DataDictionary GetDictionary(string relativeUrl);

    DataItems GetDataItems(string endpoint, string context, string graph, DataFilter dataFilter, int start, int limit);

    string DataServiceUri();
  }
}