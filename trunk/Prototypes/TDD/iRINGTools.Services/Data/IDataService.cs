using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using iRINGTools.Data;

namespace iRINGTools.Services
{
  public interface IDataService
  {
    #region ProjectionEngine Services

    IList<IProjectionEngine> GetProjectionEngines();

    IProjectionEngine GetProjectionEngine(string name);

    #endregion

    #region Data Services

    XDocument GetDataProjection(
      string scopeName,
      string applicationName,
      string graphName,
      DataFilter filter,
      string formatName,
      int start,
      int limit,
      bool fullIndex);

    XDocument GetDataProjection(
      string scopeName,
      string applicationName,
      string graphName,
      string className,
      string classIdentifier,
      string formatName,
      bool fullIndex);

    XDocument GetDataProjection(
      string scopeName,
      string applicationName,
      string graphName,
      string formatName,
      int start,
      int limit,
      bool fullIndex,
      NameValueCollection parameters);

    #endregion
  }
}
