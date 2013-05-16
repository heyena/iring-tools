using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace org.iringtools.library
{
  public interface ILightweightDataLayer
  {
    /// <summary>
    ///  Provides information types to expose to iRING
    /// </summary>
    /// <param name="refresh">whether to rebuild or use previous dictionary</param>
    /// <param name="objectType">refresh a specific object type or all if null</param>
    /// <param name="inFilter">refresh a specific object type with some criteria if any</param>
    /// <param name="outFilter">any filter to apply to outbound data objects if any</param>
    /// <returns>objects schema</returns>
    DataDictionary Dictionary(bool refresh, DataObject objectType, DataFilter inFilter, out DataFilter outFilter);

    /// <summary>
    /// Gets all data records for a given object type
    /// </summary>
    /// <param name="objectType">required - one of the object types in data dictionary</param>
    /// <returns>list of data objects</returns>
    IList<IDataObject> Get(DataObject objectType);

    /// <summary>
    /// Updates list of data objects being modified and their related objects if configured
    /// </summary>
    /// <param name="dataObjects">list of data objects</param>
    /// <returns>detail status of each data record being modified</returns>
    Response Update(IList<IDataObject> dataObjects);

    /// <summary>
    /// Gets a list of content objects by identifiers and optionally their renditions
    /// </summary>
    /// <param name="idFormats">list of identifiers and renditions</param>
    /// <returns>binary contents with mime types and metadata</returns>
    IList<IContentObject> GetContents(DataObject objectType, IDictionary<string, string> idFormats);
  }
}
